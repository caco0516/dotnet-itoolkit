// Copyright (c) 2026 caco0516
// Based on the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace IToolkit.Transports
{
    /// <summary>
    /// Transport that connects to IBM i over SSH and invokes
    /// <c>/QOpenSys/pkgs/bin/xmlservice-cli</c>, feeding the XML document on
    /// stdin and reading the XML response from stdout.
    /// </summary>
    public class SshTransport : ITransport
    {
        /// <inheritdoc />
        public Task<string> CallAsync(TransportOptions options, string xmlInput)
        {
            // SSH I/O is inherently blocking in SSH.NET; run on a thread-pool
            // thread so the calling async context is not blocked.
            return Task.Run(() => Execute(options, xmlInput));
        }

        private static string Execute(TransportOptions options, string xmlInput)
        {
            if (string.IsNullOrWhiteSpace(options.Host))
                throw new ArgumentException("Host is required for the SSH transport.", nameof(options));

            if (string.IsNullOrWhiteSpace(options.Username))
                throw new ArgumentException("Username is required for the SSH transport.", nameof(options));

            var authMethods = BuildAuthMethods(options);
            var connInfo = new ConnectionInfo(options.Host, options.Username, authMethods);

            using var client = new SshClient(connInfo);
            client.Connect();

            using var command = client.CreateCommand("/QOpenSys/pkgs/bin/xmlservice-cli");

            // SSH.NET 2024.x: CreateInputStream() must be called BEFORE BeginExecute.
            // It returns a pipe whose write-end is connected to the remote process stdin.
            using var stdinStream = command.CreateInputStream();

            var asyncResult = command.BeginExecute();

            var inputBytes = Encoding.UTF8.GetBytes(xmlInput);
            stdinStream.Write(inputBytes, 0, inputBytes.Length);
            stdinStream.Flush();
            stdinStream.Close();

            var output = command.EndExecute(asyncResult);

            if (command.ExitStatus != 0)
            {
                var errorText = command.Error ?? string.Empty;
                throw new InvalidOperationException(
                    $"xmlservice-cli exited with code {command.ExitStatus}. stderr: {errorText}");
            }

            return output;
        }

        private static AuthenticationMethod[] BuildAuthMethods(TransportOptions options)
        {
            var methods = new List<AuthenticationMethod>();

            if (!string.IsNullOrEmpty(options.PrivateKey))
            {
                var keyFile = string.IsNullOrEmpty(options.Passphrase)
                    ? new PrivateKeyFile(options.PrivateKey!)
                    : new PrivateKeyFile(options.PrivateKey!, options.Passphrase);

                methods.Add(new PrivateKeyAuthenticationMethod(options.Username!, keyFile));
            }

            if (!string.IsNullOrEmpty(options.Password))
            {
                methods.Add(new PasswordAuthenticationMethod(options.Username!, options.Password));
            }

            if (methods.Count == 0)
                throw new ArgumentException(
                    "Either Password or PrivateKey must be provided for the SSH transport.",
                    nameof(options));

            return methods.ToArray();
        }
    }
}
