// Copyright (c) 2026 caco0516
// Based on the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Text;

namespace IToolkit
{
    /// <summary>
    /// Options for creating a <see cref="ProgramCall"/>.
    /// </summary>
    public class ProgramCallOptions
    {
        /// <summary>Library that contains the program or service program.</summary>
        public string? Lib { get; set; }

        /// <summary>
        /// Target exported function of a service program.
        /// </summary>
        public string? Func { get; set; }

        /// <summary>
        /// Action on error. Valid values: <c>on</c>, <c>off</c>, <c>fast</c>. Default: <c>fast</c>.
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// Builds the XML element for an IBM i program or service-program call.
    /// </summary>
    public class ProgramCall
    {
        private readonly StringBuilder _xml;

        /// <summary>
        /// Creates a new <see cref="ProgramCall"/> for the given program name.
        /// </summary>
        /// <param name="program">Program or service program name.</param>
        /// <param name="options">Optional configuration.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="program"/> is null or empty.
        /// </exception>
        public ProgramCall(string program, ProgramCallOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(program))
                throw new ArgumentException("Program name must not be empty.", nameof(program));

            _xml = new StringBuilder();
            _xml.Append($"<pgm name='{program}'");

            if (options?.Lib  != null) _xml.Append($" lib='{options.Lib}'");
            if (options?.Func != null) _xml.Append($" func='{options.Func}'");

            _xml.Append($" error='{options?.Error ?? "fast"}'>");
        }

        /// <summary>
        /// Adds a parameter (or data-structure parameter) to the program call.
        /// </summary>
        /// <param name="parameter">
        /// Parameter configuration. Set <see cref="ParameterConfig.Type"/> to <c>"ds"</c>
        /// and populate <see cref="ParameterConfig.Fields"/> for data structures.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parameter"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="ParameterConfig.Type"/> is not set.
        /// </exception>
        public void AddParam(ParameterConfig parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            if (string.IsNullOrEmpty(parameter.Type))
                throw new ArgumentException("Expected 'Type' to be set on the parameter.", nameof(parameter));

            _xml.Append("<parm");
            if (parameter.Name != null) _xml.Append($" name='{parameter.Name}'");
            if (parameter.Io   != null) _xml.Append($" io='{parameter.Io}'");
            if (parameter.By   != null) _xml.Append($" by='{parameter.By}'");
            _xml.Append('>');

            AppendData(_xml, parameter);

            _xml.Append("</parm>");
        }

        /// <summary>
        /// Specifies the type of the return value for a service-program function call.
        /// </summary>
        /// <param name="data">Return data configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <see cref="ParameterConfig.Type"/> is not set.
        /// </exception>
        public void AddReturn(ParameterConfig data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.Type))
                throw new ArgumentException("Expected 'Type' to be set on the return data.", nameof(data));

            _xml.Append("<return>");
            AppendData(_xml, data);
            _xml.Append("</return>");
        }

        /// <summary>Returns the generated program XML including the closing <c>&lt;/pgm&gt;</c> tag.</summary>
        public string ToXml() => _xml.ToString() + "</pgm>";

        // ── Private helpers ──────────────────────────────────────────────────────

        private static void AppendData(StringBuilder sb, ParameterConfig data)
        {
            if (data.Type == "ds")
            {
                sb.Append("<ds");
                if (data.Name != null) sb.Append($" name='{data.Name}'");
                if (data.Dim  != null) sb.Append($" dim='{data.Dim}'");
                if (data.Dou  != null) sb.Append($" dou='{data.Dou}'");
                if (data.Len  != null) sb.Append($" len='{data.Len}'");
                sb.Append('>');

                if (data.Fields != null)
                {
                    foreach (var field in data.Fields)
                        AppendData(sb, field);
                }

                sb.Append("</ds>");
            }
            else
            {
                sb.Append($"<data type='{data.Type}'");
                if (data.Name    != null) sb.Append($" name='{data.Name}'");
                if (data.Varying != null) sb.Append($" varying='{data.Varying}'");
                if (data.Enddo   != null) sb.Append($" enddo='{data.Enddo}'");
                if (data.Setlen  != null) sb.Append($" setlen='{data.Setlen}'");
                if (data.Hex     != null) sb.Append($" hex='{data.Hex}'");
                if (data.Trim    != null) sb.Append($" trim='{data.Trim}'");
                sb.Append($">{data.Value}</data>");
            }
        }
    }
}
