// Copyright (c) 2026 caco0516
// Based on the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Data.Odbc;
using System.Text;
using System.Threading.Tasks;

namespace IToolkit.Transports
{
    /// <summary>
    /// Transport that calls XMLSERVICE via an IBM i Access ODBC connection,
    /// invoking the <c>iPLUGR512K</c> stored procedure.
    /// <para>
    /// Requires the IBM i Access ODBC Driver to be installed on the machine
    /// running the application.
    /// </para>
    /// </summary>
    public class OdbcTransport : ITransport
    {
        /// <inheritdoc />
        public Task<string> CallAsync(TransportOptions options, string xmlInput)
        {
            // ODBC does not have true async support; run on a thread-pool thread.
            return Task.Run(() => Execute(options, xmlInput));
        }

        private static string Execute(TransportOptions options, string xmlInput)
        {
            var connectionString = BuildConnectionString(options);
            var sql = $"call {options.XsLib}.iPLUGR512K(?,?,?)";

            using var conn = new OdbcConnection(connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            // Positional parameters matching: ipc, ctl, xmlin
            cmd.Parameters.Add(new OdbcParameter("p1", OdbcType.Char)  { Value = options.Ipc });
            cmd.Parameters.Add(new OdbcParameter("p2", OdbcType.Char)  { Value = options.Ctl });
            cmd.Parameters.Add(new OdbcParameter("p3", OdbcType.Text)  { Value = xmlInput });

            var sb = new StringBuilder();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sb.Append(reader["OUT151"]?.ToString() ?? string.Empty);
            }

            if (sb.Length == 0)
                throw new InvalidOperationException("ODBC transport: empty result set returned by iPLUGR512K.");

            return sb.ToString();
        }

        private static string BuildConnectionString(TransportOptions options)
        {
            // If a DSN is provided, use it directly.
            if (!string.IsNullOrWhiteSpace(options.Dsn))
                return $"DSN={options.Dsn};";

            var sb = new StringBuilder();
            sb.Append("DRIVER=IBM i Access ODBC Driver;");
            sb.Append($"SYSTEM={options.Host};");

            if (!string.IsNullOrEmpty(options.Username))
                sb.Append($"UID={options.Username};");

            if (!string.IsNullOrEmpty(options.Password))
                sb.Append($"PWD={options.Password};");

            return sb.ToString();
        }
    }
}
