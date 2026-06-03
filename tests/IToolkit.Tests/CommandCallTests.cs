// Copyright (c) 2026 caco0516
// Based on the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using Xunit;

namespace IToolkit.Tests
{
    public class CommandCallTests
    {
        // ── Constructor validation ───────────────────────────────────────────────

        [Fact]
        public void Constructor_ThrowsWhenCommandMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new CommandCall(new CommandCallConfig { Type = "cl" }));
            Assert.Contains("command", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ThrowsWhenTypeMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new CommandCall(new CommandCallConfig { Command = "WRKACTJOB" }));
            Assert.Contains("type", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_ThrowsOnInvalidType()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new CommandCall(new CommandCallConfig { Command = "WRKACTJOB", Type = "invalid" }));
            Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── CL commands ──────────────────────────────────────────────────────────

        [Fact]
        public void ToXml_ClCommand_DefaultExecIsCmd()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "WRKACTJOB",
                Type    = "cl"
            });
            Assert.Equal("<cmd exec='cmd' error='fast'>WRKACTJOB</cmd>", call.ToXml());
        }

        [Fact]
        public void ToXml_ClCommand_RexxExecWhenCommandContainsQuestionMark()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "RTVJOBA USRLIBL(?) SYSLIBL(?)",
                Type    = "cl"
            });
            Assert.Contains("exec='rexx'", call.ToXml());
        }

        [Fact]
        public void ToXml_ClCommand_ExplicitExecOverridesDefault()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "WRKACTJOB",
                Type    = "cl",
                Options = new ClOptions { Exec = "system" }
            });
            Assert.Contains("exec='system'", call.ToXml());
        }

        [Fact]
        public void ToXml_ClCommand_ErrorOptionApplied()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "WRKACTJOB",
                Type    = "cl",
                Options = new ClOptions { Error = "on" }
            });
            Assert.Contains("error='on'", call.ToXml());
        }

        [Fact]
        public void ToXml_ClCommand_HexBeforeAfterApplied()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "WRKACTJOB",
                Type    = "cl",
                Options = new ClOptions { Hex = "on", Before = "37", After = "819" }
            });
            var xml = call.ToXml();
            Assert.Contains("hex='on'",    xml);
            Assert.Contains("before='37'", xml);
            Assert.Contains("after='819'", xml);
        }

        // ── SH commands ──────────────────────────────────────────────────────────

        [Fact]
        public void ToXml_ShCommand_GeneratesShTag()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "ls -la",
                Type    = "sh"
            });
            var xml = call.ToXml();
            Assert.StartsWith("<sh ", xml);
            Assert.EndsWith("</sh>", xml);
            Assert.Contains("ls -la", xml);
        }

        [Fact]
        public void ToXml_ShCommand_RowsOptionApplied()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "ls",
                Type    = "sh",
                Options = new ShOptions { Rows = "on" }
            });
            Assert.Contains("rows='on'", call.ToXml());
        }

        // ── QSH commands ─────────────────────────────────────────────────────────

        [Fact]
        public void ToXml_QshCommand_GeneratesQshTag()
        {
            var call = new CommandCall(new CommandCallConfig
            {
                Command = "echo hello",
                Type    = "qsh"
            });
            var xml = call.ToXml();
            Assert.StartsWith("<qsh ", xml);
            Assert.EndsWith("</qsh>", xml);
        }
    }
}
