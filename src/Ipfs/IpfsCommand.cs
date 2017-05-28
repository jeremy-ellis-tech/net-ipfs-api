﻿using Ipfs.Json;
using Ipfs.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ipfs
{
    public abstract class IpfsCommand
    {
        private readonly Uri _commandUri;
        private readonly HttpClient _httpClient;
        protected readonly IJsonSerializer _jsonSerializer;

        protected IpfsCommand(Uri commandUri, HttpClient httpClient, IJsonSerializer jsonSerializer)
        {
            _commandUri = commandUri;
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
        }

        #region ExecuteGetAsync() Overrides
        protected async Task<HttpContent> ExecuteGetAsync(string methodName, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            return await ExecuteGetAsync(methodName, (IEnumerable<string>)null, null, cts);
        }

        protected async Task<HttpContent> ExecuteGetAsync(string methodName, string arg, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            return await ExecuteGetAsync(methodName, ToEnumerable(arg), null, cts);
        }

        protected async Task<HttpContent> ExecuteGetAsync(string methodName, IDictionary<string,string> flags, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            return await ExecuteGetAsync(methodName, (IEnumerable<string>)null, flags, cts);
        }

        protected async Task<HttpContent> ExecuteGetAsync(string methodName, string arg, IDictionary<string, string> flags, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            return await ExecuteGetAsync(methodName, ToEnumerable(arg), flags, cts);
        }

        protected async Task<HttpContent> ExecuteGetAsync(string methodName, IEnumerable<string> args, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            return await ExecuteGetAsync(methodName, args, null, cts);
        }
        #endregion

        protected async Task<HttpContent> ExecuteGetAsync(string methodName, IEnumerable<string> args, IDictionary<string, string> flags, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            Uri commandUri = GetSubCommandUri(methodName, args, flags);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, commandUri);

            return await ExecuteAsync(request, cts);
        }

        #region ExecutePostAsync() Overrides
        protected async Task<HttpContent> ExecutePostAsync(string methodName, IDictionary<string, string> flags, HttpContent content)
        {
            return await ExecutePostAsync(methodName, null, flags, content);
        }
        #endregion

        protected async Task<HttpContent> ExecutePostAsync(string methodName, IEnumerable<string> args, IDictionary<string, string> flags, HttpContent content)
        {
            Uri commandUri = GetSubCommandUri(methodName, args, flags);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, commandUri)
            {
                Content = content,
            };

            return await ExecuteAsync(request);
        }

        private async Task<HttpContent> ExecuteAsync(HttpRequestMessage request, CancellationTokenSource cts = default(CancellationTokenSource))
        {
            Debug.WriteLine(String.Format("IpfsCommand.ExecuteAsync: {0} {1}", request.Method.ToString(), request.RequestUri.ToString()));

            HttpResponseMessage response = await _httpClient.SendAsync(request, cts.Token);

            response.EnsureSuccessStatusCode();

            return response.Content;
        }

        private Uri GetSubCommandUri(string methodName, IEnumerable<string> args, IDictionary<string, string> flags)
        {
            Uri commandUri = new Uri(_commandUri.ToString());

            if (!String.IsNullOrEmpty(methodName))
            {
                commandUri = UriHelper.AppendPath(commandUri, methodName);
            }

            if (args != null && args.Count() > 0)
            {
                commandUri = UriHelper.AppendQuery(commandUri, args.Select(x=> new Tuple<string,string>("arg", x)));
            }

            if (flags != null && flags.Count > 0)
            {
                commandUri = UriHelper.AppendQuery(commandUri, flags.Select(x=> new Tuple<string,string>(x.Key, x.Value)));
            }

            return commandUri;
        }

        /// <summary>
        /// Helper method to return values in Enumerable
        /// </summary>
        /// <typeparam name="T">The type of the enumerable elements</typeparam>
        /// <param name="values">Element of the singleton enumerable</param>
        /// <returns>Enumerable containing values</returns>
        protected static IEnumerable<T> ToEnumerable<T>(params T[] values)
        {
            foreach (var value in values)
            {
                yield return value;
            }
        }
    }
}
