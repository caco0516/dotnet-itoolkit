// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Xunit;

namespace IToolkit.Tests
{
    public class ProgramCallTests
    {
        // ── Constructor ──────────────────────────────────────────────────────────

        [Fact]
        public void Constructor_ThrowsWhenProgramEmpty()
        {
            Assert.Throws<ArgumentException>(() => new ProgramCall(""));
        }

        [Fact]
        public void Constructor_DefaultErrorIsFast()
        {
            var pgm = new ProgramCall("MYPGM");
            Assert.Contains("error='fast'", pgm.ToXml());
        }

        [Fact]
        public void Constructor_LibAndFuncIncludedWhenSet()
        {
            var pgm = new ProgramCall("MYSRVPGM", new ProgramCallOptions
            {
                Lib  = "MYLIB",
                Func = "MYEXPORT"
            });
            var xml = pgm.ToXml();
            Assert.Contains("lib='MYLIB'",       xml);
            Assert.Contains("func='MYEXPORT'",   xml);
        }

        // ── AddParam ─────────────────────────────────────────────────────────────

        [Fact]
        public void AddParam_ThrowsWhenTypeMissing()
        {
            var pgm = new ProgramCall("MYPGM");
            Assert.Throws<ArgumentException>(() => pgm.AddParam(new ParameterConfig { Value = "1" }));
        }

        [Fact]
        public void AddParam_SimpleIntegerParam()
        {
            var pgm = new ProgramCall("MYPGM");
            pgm.AddParam(new ParameterConfig { Type = "10i0", Value = "42", Io = "both" });
            var xml = pgm.ToXml();
            Assert.Contains("<parm io='both'>", xml);
            Assert.Contains("<data type='10i0'>42</data>", xml);
        }

        [Fact]
        public void AddParam_PassByValue()
        {
            var pgm = new ProgramCall("MYPGM");
            pgm.AddParam(new ParameterConfig { Type = "8f", Value = "0", By = "val" });
            Assert.Contains("by='val'", pgm.ToXml());
        }

        [Fact]
        public void AddParam_DataStructure()
        {
            var pgm = new ProgramCall("MYPGM");
            pgm.AddParam(new ParameterConfig
            {
                Type   = "ds",
                Fields = new List<ParameterConfig>
                {
                    new ParameterConfig { Type = "10i0", Value = "1" },
                    new ParameterConfig { Type = "32a",  Value = "hello" }
                }
            });
            var xml = pgm.ToXml();
            Assert.Contains("<ds>",              xml);
            Assert.Contains("</ds>",             xml);
            Assert.Contains("<data type='10i0'>1</data>",      xml);
            Assert.Contains("<data type='32a'>hello</data>",   xml);
        }

        [Fact]
        public void AddParam_DataStructureWithOptions()
        {
            var pgm = new ProgramCall("MYPGM");
            pgm.AddParam(new ParameterConfig
            {
                Type = "ds",
                Dim  = "2",
                Dou  = "myLabel",
                Len  = "myLen",
                Fields = new List<ParameterConfig>
                {
                    new ParameterConfig { Type = "10a", Value = "" }
                }
            });
            var xml = pgm.ToXml();
            Assert.Contains("dim='2'",        xml);
            Assert.Contains("dou='myLabel'",  xml);
            Assert.Contains("len='myLen'",    xml);
        }

        [Fact]
        public void AddParam_DataOptions_VaryingHexTrim()
        {
            var pgm = new ProgramCall("MYPGM");
            pgm.AddParam(new ParameterConfig
            {
                Type    = "1024a",
                Value   = "text",
                Varying = "on",
                Hex     = "off",
                Trim    = "on"
            });
            var xml = pgm.ToXml();
            Assert.Contains("varying='on'", xml);
            Assert.Contains("hex='off'",    xml);
            Assert.Contains("trim='on'",    xml);
        }

        // ── AddReturn ─────────────────────────────────────────────────────────────

        [Fact]
        public void AddReturn_ThrowsWhenTypeMissing()
        {
            var pgm = new ProgramCall("MYPGM");
            Assert.Throws<ArgumentException>(() => pgm.AddReturn(new ParameterConfig { Value = "0" }));
        }

        [Fact]
        public void AddReturn_GeneratesReturnElement()
        {
            var pgm = new ProgramCall("QC2UTIL2", new ProgramCallOptions
            {
                Lib  = "QSYS",
                Func = "cos"
            });
            pgm.AddParam(new ParameterConfig  { Type = "8f", Value = "0", By = "val" });
            pgm.AddReturn(new ParameterConfig { Type = "8f", Value = "" });
            var xml = pgm.ToXml();
            Assert.Contains("<return>", xml);
            Assert.Contains("</return>", xml);
            Assert.Contains("</pgm>", xml);
        }

        // ── ToXml ────────────────────────────────────────────────────────────────

        [Fact]
        public void ToXml_ClosesWithPgmTag()
        {
            var pgm = new ProgramCall("MYPGM");
            Assert.EndsWith("</pgm>", pgm.ToXml());
        }
    }
}
