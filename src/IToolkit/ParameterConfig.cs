// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace IToolkit
{
    /// <summary>
    /// Describes a single parameter (or data-structure parameter) passed to
    /// <see cref="ProgramCall.AddParam"/> or used inside a data-structure.
    /// Set <see cref="Type"/> to <c>"ds"</c> and populate <see cref="Fields"/>
    /// to define a data structure.
    /// </summary>
    public class ParameterConfig
    {
        /// <summary>
        /// XMLSERVICE data type (e.g. <c>10i0</c>, <c>32a</c>, <c>8f</c>) or
        /// <c>"ds"</c> for a data structure.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Value of the data element. Ignored when <see cref="Type"/> is <c>"ds"</c>.</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>Optional name annotation for the parameter or data element.</summary>
        public string? Name { get; set; }

        // ── ProgramCall.AddParam-only ────────────────────────────────────────────

        /// <summary>
        /// I/O direction. Valid values: <c>in</c>, <c>out</c>, <c>both</c>, <c>omit</c>.
        /// </summary>
        public string? Io { get; set; }

        /// <summary>
        /// Pass-by convention. Valid values: <c>ref</c>, <c>val</c>.
        /// Pass by value requires XMLSERVICE >= 1.9.9.3.
        /// </summary>
        public string? By { get; set; }

        // ── Data-structure-specific ──────────────────────────────────────────────

        /// <summary>Array dimension for a <c>ds</c> parameter.</summary>
        public string? Dim { get; set; }

        /// <summary>Do-until label for a <c>ds</c> parameter.</summary>
        public string? Dou { get; set; }

        /// <summary>Length label for a <c>ds</c> parameter.</summary>
        public string? Len { get; set; }

        /// <summary>
        /// Child data elements when <see cref="Type"/> is <c>"ds"</c>.
        /// Each element may itself be a nested <c>ds</c>.
        /// </summary>
        public List<ParameterConfig>? Fields { get; set; }

        // ── Data-element options ─────────────────────────────────────────────────

        /// <summary>
        /// Marks the data as a varying-length character type. Valid values:
        /// <c>on</c> (= <c>2</c>), <c>off</c>, <c>2</c>, <c>4</c>.
        /// </summary>
        public string? Varying { get; set; }

        /// <summary>Label that marks the end of the matching <c>dou</c>.</summary>
        public string? Enddo { get; set; }

        /// <summary>Label used to set the length from the matching <c>len</c>.</summary>
        public string? Setlen { get; set; }

        /// <summary>Interpret data as hex. Valid values: <c>on</c>, <c>off</c>.</summary>
        public string? Hex { get; set; }

        /// <summary>Allow whitespace trimming. Valid values: <c>on</c>, <c>off</c>.</summary>
        public string? Trim { get; set; }
    }
}
