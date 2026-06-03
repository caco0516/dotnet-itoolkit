// Copyright (c) 2026 caco0516
// Based on the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

namespace IToolkit.Transports
{
    /// <summary>
    /// Options shared across all transports. Set only the properties relevant to your
    /// chosen transport; unused properties are ignored.
    /// </summary>
    public class TransportOptions
    {
        // ── Common ──────────────────────────────────────────────────────────────

        /// <summary>Username for authentication.</summary>
        public string? Username { get; set; }

        /// <summary>Password for authentication.</summary>
        public string? Password { get; set; }

        /// <summary>
        /// Key name / security route to the XMLSERVICE job. Default: <c>*NA</c>.
        /// </summary>
        public string Ipc { get; set; } = "*NA";

        /// <summary>
        /// Control options for XMLSERVICE jobs. Default: <c>*here</c>.
        /// </summary>
        public string Ctl { get; set; } = "*here";

        // ── HTTP / ODBC / idb ───────────────────────────────────────────────────

        /// <summary>
        /// Database / data source to connect to. Default: <c>*LOCAL</c>.
        /// Used by the REST and idb transports.
        /// </summary>
        public string Database { get; set; } = "*LOCAL";

        /// <summary>
        /// Full URL to the xmlcgi endpoint.
        /// Example: <c>http://myhost:80/cgi-bin/xmlcgi.pgm</c>.
        /// Required for the REST transport.
        /// </summary>
        public string? Url { get; set; }

        // ── ODBC ────────────────────────────────────────────────────────────────

        /// <summary>
        /// DSN to use for ODBC connections. When set, individual host/user/password
        /// fields are ignored and the DSN connection string is used instead.
        /// </summary>
        public string? Dsn { get; set; }

        /// <summary>
        /// Hostname of the IBM i server. Used by the ODBC transport when no DSN is
        /// specified. Default: <c>localhost</c>.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// XMLSERVICE library on the IBM i system. Default: <c>QXMLSERV</c>.
        /// </summary>
        public string XsLib { get; set; } = "QXMLSERV";

        // ── SSH ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Filesystem path to the private key file for SSH public-key authentication.
        /// When set, <see cref="Password"/> is used as the key passphrase (if any);
        /// see <see cref="Passphrase"/> for a dedicated property.
        /// </summary>
        public string? PrivateKey { get; set; }

        /// <summary>
        /// Passphrase used to decrypt <see cref="PrivateKey"/>.
        /// </summary>
        public string? Passphrase { get; set; }

        // ── Diagnostics ─────────────────────────────────────────────────────────

        /// <summary>Enables verbose diagnostic output to stdout.</summary>
        public bool Verbose { get; set; }
    }
}
