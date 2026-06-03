// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;

namespace IToolkit.Transports
{
    /// <summary>
    /// Creates <see cref="ITransport"/> instances by name.
    /// </summary>
    internal static class TransportFactory
    {
        /// <summary>
        /// Returns the transport implementation for the given name.
        /// </summary>
        /// <param name="transport">
        /// Name of the transport. Supported values: <c>rest</c>, <c>ssh</c>, <c>odbc</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when an unsupported transport name is supplied.
        /// </exception>
        public static ITransport Create(string transport)
        {
            return transport switch
            {
                "rest" => new HttpTransport(),
                "ssh"  => new SshTransport(),
                "odbc" => new OdbcTransport(),
                _      => throw new ArgumentException(
                               $"'{transport}' is not a valid transport. " +
                               "Supported transports: rest, ssh, odbc.")
            };
        }
    }
}
