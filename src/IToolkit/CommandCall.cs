// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;

namespace IToolkit
{
    /// <summary>
    /// Options for a CL command (<c>cl</c> type).
    /// </summary>
    public class ClOptions
    {
        /// <summary>
        /// How to run the command. Valid values: <c>cmd</c>, <c>system</c>, <c>rexx</c>.
        /// Defaults to <c>cmd</c>, or <c>rexx</c> when the command string contains a
        /// question mark (<c>?</c>).
        /// </summary>
        public string? Exec { get; set; }

        /// <summary>
        /// Action on error. Valid values: <c>on</c>, <c>off</c>, <c>fast</c>. Default: <c>fast</c>.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>Output in hex format. Valid values: <c>on</c>, <c>off</c>.</summary>
        public string? Hex { get; set; }

        /// <summary>CCSID to convert to before the command call.</summary>
        public string? Before { get; set; }

        /// <summary>CCSID to convert to after the command call.</summary>
        public string? After { get; set; }
    }

    /// <summary>
    /// Options for shell commands (<c>sh</c> and <c>qsh</c> types).
    /// </summary>
    public class ShOptions
    {
        /// <summary>
        /// Split output row by row. Valid values: <c>on</c>, <c>off</c>. Default: <c>off</c>.
        /// </summary>
        public string? Rows { get; set; }

        /// <summary>
        /// Action on error. Valid values: <c>on</c>, <c>off</c>, <c>fast</c>. Default: <c>fast</c>.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>Output in hex format. Valid values: <c>on</c>, <c>off</c>.</summary>
        public string? Hex { get; set; }

        /// <summary>CCSID to convert to before the command call.</summary>
        public string? Before { get; set; }

        /// <summary>CCSID to convert to after the command call.</summary>
        public string? After { get; set; }
    }

    /// <summary>
    /// Configuration for a <see cref="CommandCall"/>.
    /// </summary>
    public class CommandCallConfig
    {
        /// <summary>
        /// The command string to execute.
        /// </summary>
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Command type. Valid values: <c>cl</c>, <c>sh</c>, <c>qsh</c>.
        /// NOTE: <c>qsh</c> requires XMLSERVICE >= 1.9.8.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Options specific to the command type. Pass a <see cref="ClOptions"/>
        /// for CL commands, or a <see cref="ShOptions"/> for sh/qsh commands.
        /// </summary>
        public object? Options { get; set; }
    }

    /// <summary>
    /// Builds the XML element for a single CL, QSH, or SH command call.
    /// </summary>
    public class CommandCall
    {
        private static readonly string[] AvailableCommands = { "sh", "cl", "qsh" };

        private readonly string _command;
        private readonly string _type;
        private readonly ClOptions? _clOptions;
        private readonly ShOptions? _shOptions;

        /// <summary>
        /// Creates a new <see cref="CommandCall"/> object.
        /// </summary>
        /// <param name="config">Command configuration.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="CommandCallConfig.Command"/> or
        /// <see cref="CommandCallConfig.Type"/> is missing or invalid.
        /// </exception>
        public CommandCall(CommandCallConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Command))
                throw new ArgumentException("Please specify a command.", nameof(config));

            if (string.IsNullOrWhiteSpace(config.Type))
                throw new ArgumentException("Please specify a type (cl, qsh, sh).", nameof(config));

            if (!Array.Exists(AvailableCommands, t => t == config.Type))
                throw new ArgumentException(
                    $"Invalid command type '{config.Type}'. Valid types: {string.Join(", ", AvailableCommands)}.",
                    nameof(config));

            _command = config.Command;
            _type    = config.Type;

            if (config.Options is ClOptions cl)
                _clOptions = cl;
            else if (config.Options is ShOptions sh)
                _shOptions = sh;
        }

        /// <summary>Returns the command serialised as an XMLSERVICE XML element.</summary>
        public string ToXml()
        {
            // CL uses <cmd>; shell variants use their own tag name.
            var tag = _type == "cl" ? "cmd" : _type;
            var xml = new System.Text.StringBuilder();

            xml.Append($"<{tag}");

            if (_type == "cl")
            {
                var defaultExec = _command.IndexOf('?') >= 0 ? "rexx" : "cmd";
                xml.Append($" exec='{_clOptions?.Exec ?? defaultExec}'");

                if (_clOptions?.Hex    != null) xml.Append($" hex='{_clOptions.Hex}'");
                if (_clOptions?.Before != null) xml.Append($" before='{_clOptions.Before}'");
                if (_clOptions?.After  != null) xml.Append($" after='{_clOptions.After}'");

                xml.Append($" error='{_clOptions?.Error ?? "fast"}'");
            }
            else
            {
                if (_shOptions?.Rows   != null) xml.Append($" rows='{_shOptions.Rows}'");
                if (_shOptions?.Hex    != null) xml.Append($" hex='{_shOptions.Hex}'");
                if (_shOptions?.Before != null) xml.Append($" before='{_shOptions.Before}'");
                if (_shOptions?.After  != null) xml.Append($" after='{_shOptions.After}'");

                xml.Append($" error='{_shOptions?.Error ?? "fast"}'");
            }

            xml.Append($">{_command}</{tag}>");
            return xml.ToString();
        }
    }
}
