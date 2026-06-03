// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using IToolkit.Transports;

namespace IToolkit
{
    /// <summary>
    /// Configuration object passed to <see cref="Connection"/>.
    /// </summary>
    public class ConnectionOptions
    {
        /// <summary>
        /// The transport to use. Valid values: <c>rest</c>, <c>ssh</c>, <c>odbc</c>.
        /// </summary>
        public string Transport { get; set; } = string.Empty;

        /// <summary>
        /// Transport-specific connection options.
        /// </summary>
        public TransportOptions TransportOptions { get; set; } = new TransportOptions();

        /// <summary>
        /// When <c>true</c>, the XML input and output are printed to stdout.
        /// Default: <c>false</c>.
        /// </summary>
        public bool Verbose { get; set; }
    }
}
