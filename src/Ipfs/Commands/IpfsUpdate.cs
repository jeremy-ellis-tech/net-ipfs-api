﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ipfs.Commands
{
    public class IpfsUpdate : IpfsCommand
    {
        internal IpfsUpdate()
        {
        }

        internal IpfsUpdate(string address) : base(address)
        {
        }

        internal IpfsUpdate(string address, HttpClient httpClient) : base(address, httpClient)
        {
        }

        private Uri _baseUri;
        protected override Uri CommandUri
        {
            get
            {
                if (_baseUri == null)
                {
                    UriBuilder builder = new UriBuilder(_address);
                    builder.Path += "/api/v0/update/";
                    _baseUri = builder.Uri;
                }

                return _baseUri;
            }
        }

        /// <summary>
        /// Checks if updates are available
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> Check()
        {
            return await ExecuteAsync("check");
        }

        /// <summary>
        /// List the changelog for the latest versions of IPFS
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> Log()
        {
            return await ExecuteAsync("log");
        }
    }
}