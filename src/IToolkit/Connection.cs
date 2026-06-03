// Copyright (c) 2026 caco0516
// Based on the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IToolkit.Transports;

namespace IToolkit
{
    /// <summary>
    /// Manages a list of <see cref="CommandCall"/> and <see cref="ProgramCall"/> objects,
    /// serialises them into an XMLSERVICE document, and dispatches the document to IBM i
    /// via the configured transport.
    /// </summary>
    public class Connection
    {
        private readonly List<string> _commandList = new List<string>();
        private readonly ITransport _transport;
        private readonly TransportOptions _transportOptions;

        /// <summary>
        /// When <c>true</c>, the XML input and output are printed to stdout for debugging.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Creates a <see cref="Connection"/> from a <see cref="ConnectionOptions"/> object.
        /// The transport implementation is resolved by name from
        /// <see cref="ConnectionOptions.Transport"/>.
        /// </summary>
        /// <param name="options">Connection configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="options"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="ConnectionOptions.Transport"/> is not a recognised transport name.
        /// </exception>
        public Connection(ConnectionOptions options)
            : this(
                TransportFactory.Create(
                    options?.Transport ?? throw new ArgumentNullException(nameof(options))),
                options.TransportOptions ?? new TransportOptions(),
                options.Verbose)
        { }

        /// <summary>
        /// Creates a <see cref="Connection"/> with an explicit transport implementation.
        /// Useful for testing or for providing a custom transport.
        /// </summary>
        /// <param name="transport">Transport implementation to use.</param>
        /// <param name="transportOptions">Transport connection options.</param>
        /// <param name="verbose">Whether to enable verbose output.</param>
        public Connection(ITransport transport, TransportOptions transportOptions, bool verbose = false)
        {
            _transport        = transport        ?? throw new ArgumentNullException(nameof(transport));
            _transportOptions = transportOptions ?? throw new ArgumentNullException(nameof(transportOptions));
            Verbose           = verbose;
        }

        /// <summary>
        /// Enables or disables verbose output and returns the current state.
        /// When <paramref name="flag"/> is omitted, only the current state is returned.
        /// </summary>
        /// <param name="flag">Optional new value for <see cref="Verbose"/>.</param>
        /// <returns>The current value of <see cref="Verbose"/>.</returns>
        public bool Debug(bool? flag = null)
        {
            if (flag.HasValue) Verbose = flag.Value;
            return Verbose;
        }

        /// <summary>Returns the <see cref="TransportOptions"/> in use.</summary>
        public TransportOptions GetTransportOptions() => _transportOptions;

        /// <summary>
        /// Adds a <see cref="CommandCall"/> to the pending command list.
        /// </summary>
        public void Add(CommandCall command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _commandList.Add(command.ToXml());
        }

        /// <summary>
        /// Adds a <see cref="ProgramCall"/> to the pending command list.
        /// </summary>
        public void Add(ProgramCall program)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));
            _commandList.Add(program.ToXml());
        }

        /// <summary>
        /// Adds a raw XML string to the pending command list.
        /// </summary>
        public void Add(string xml)
        {
            if (!string.IsNullOrEmpty(xml))
                _commandList.Add(xml);
        }

        /// <summary>
        /// Serialises the command list into an XMLSERVICE document, sends it via the
        /// configured transport, and returns the raw XML response.
        /// The command list is cleared after this call.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the command list is empty.
        /// </exception>
        public async Task<string> RunAsync()
        {
            if (_commandList.Count == 0)
                throw new InvalidOperationException(
                    "Command list is empty. Add at least one command before calling Run.");

            var xmlInput = $"<?xml version='1.0'?><myscript>{string.Join(",", _commandList)}</myscript>";
            _commandList.Clear();

            _transportOptions.Verbose = Verbose;

            if (Verbose)
            {
                Console.WriteLine("============\nINPUT XML\n============");
                Console.WriteLine(xmlInput);
            }

            var xmlOutput = await _transport.CallAsync(_transportOptions, xmlInput).ConfigureAwait(false);

            if (Verbose)
            {
                Console.WriteLine("============\nOUTPUT XML\n============");
                Console.WriteLine(xmlOutput);
            }

            return xmlOutput;
        }

        /// <summary>
        /// Synchronous wrapper around <see cref="RunAsync"/>.
        /// Prefer <see cref="RunAsync"/> in async contexts to avoid potential deadlocks.
        /// </summary>
        public string Run() => RunAsync().GetAwaiter().GetResult();
    }
}
