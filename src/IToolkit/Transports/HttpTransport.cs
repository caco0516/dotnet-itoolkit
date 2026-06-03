// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace IToolkit.Transports
{
    /// <summary>
    /// Transport that calls XMLSERVICE via the HTTP CGI endpoint
    /// (<c>xmlcgi.pgm</c>). Corresponds to the <c>rest</c> transport in the
    /// Node.js itoolkit.
    /// </summary>
    public class HttpTransport : ITransport
    {
        // A single shared HttpClient is the recommended pattern.
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <inheritdoc />
        public async Task<string> CallAsync(TransportOptions options, string xmlInput)
        {
            if (string.IsNullOrWhiteSpace(options.Username))
                throw new ArgumentException("Username is required for the REST transport.", nameof(options));

            if (string.IsNullOrWhiteSpace(options.Password))
                throw new ArgumentException("Password is required for the REST transport.", nameof(options));

            if (string.IsNullOrWhiteSpace(options.Url))
                throw new ArgumentException("Url is required for the REST transport.", nameof(options));

            var parameters = new Dictionary<string, string>
            {
                ["db2"]    = options.Database,
                ["uid"]    = options.Username!,
                ["pwd"]    = options.Password!,
                ["ipc"]    = options.Ipc,
                ["ctl"]    = options.Ctl,
                ["xmlin"]  = xmlInput,
                ["xmlout"] = "15728640"   // max output buffer: 15 MiB
            };

            using var content = new FormUrlEncodedContent(parameters);
            var response = await _httpClient.PostAsync(options.Url, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}
