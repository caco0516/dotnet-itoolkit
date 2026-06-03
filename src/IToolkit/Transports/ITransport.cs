// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System.Threading.Tasks;

namespace IToolkit.Transports
{
    /// <summary>
    /// Defines the contract for an XMLSERVICE transport.
    /// </summary>
    public interface ITransport
    {
        /// <summary>
        /// Sends an XML request to XMLSERVICE and returns the XML response.
        /// </summary>
        /// <param name="options">Transport-specific connection options.</param>
        /// <param name="xmlInput">The XML document to send.</param>
        /// <returns>The XML response from XMLSERVICE.</returns>
        Task<string> CallAsync(TransportOptions options, string xmlInput);
    }
}
