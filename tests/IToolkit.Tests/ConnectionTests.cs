// Copyright (c) 2026 caco0516
// Inspired by the Node.js itoolkit project by IBM (https://github.com/IBM/nodejs-itoolkit)
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using IToolkit.Transports;
using Moq;
using Xunit;

namespace IToolkit.Tests
{
    public class ConnectionTests
    {
        private static readonly TransportOptions DefaultOptions = new TransportOptions();

        // ── Constructor ──────────────────────────────────────────────────────────

        [Fact]
        public void Constructor_ThrowsWhenOptionsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Connection(null!));
        }

        [Fact]
        public void Constructor_ThrowsOnUnknownTransport()
        {
            Assert.Throws<ArgumentException>(() => new Connection(new ConnectionOptions
            {
                Transport = "unknown"
            }));
        }

        // ── Add ──────────────────────────────────────────────────────────────────

        [Fact]
        public void Add_CommandCall_DoesNotThrow()
        {
            var conn = BuildConnection(out _);
            conn.Add(new CommandCall(new CommandCallConfig { Command = "WRKACTJOB", Type = "cl" }));
        }

        [Fact]
        public void Add_ProgramCall_DoesNotThrow()
        {
            var conn = BuildConnection(out _);
            conn.Add(new ProgramCall("MYPGM"));
        }

        [Fact]
        public void Add_RawXmlString_DoesNotThrow()
        {
            var conn = BuildConnection(out _);
            conn.Add("<cmd exec='cmd' error='fast'>WRKACTJOB</cmd>");
        }

        [Fact]
        public void Add_NullOrEmptyXmlString_IsIgnored()
        {
            var mock = new Mock<ITransport>();
            var conn = new Connection(mock.Object, DefaultOptions);

            // Adding empty/null should not throw, and the list stays empty
            conn.Add((string)null!);
            conn.Add(string.Empty);

            // RunAsync should throw because nothing was added
            Assert.Throws<InvalidOperationException>(() => conn.Run());
        }

        // ── Run ──────────────────────────────────────────────────────────────────

        [Fact]
        public void Run_ThrowsWhenCommandListEmpty()
        {
            var conn = BuildConnection(out _);
            Assert.Throws<InvalidOperationException>(() => conn.Run());
        }

        [Fact]
        public async Task RunAsync_SendsWrappedXmlToTransport()
        {
            var mock = new Mock<ITransport>();
            mock.Setup(t => t.CallAsync(It.IsAny<TransportOptions>(), It.IsAny<string>()))
                .ReturnsAsync("<myscript><cmd error='fast'>WRKACTJOB</cmd></myscript>");

            var conn = new Connection(mock.Object, DefaultOptions);
            conn.Add(new CommandCall(new CommandCallConfig { Command = "WRKACTJOB", Type = "cl" }));

            var result = await conn.RunAsync();

            Assert.NotNull(result);
            // Transport should have been called with an XML document starting correctly.
            mock.Verify(t => t.CallAsync(
                It.IsAny<TransportOptions>(),
                It.Is<string>(s => s.StartsWith("<?xml version='1.0'?><myscript>"))),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_ClearsCommandListAfterRun()
        {
            var mock = new Mock<ITransport>();
            mock.Setup(t => t.CallAsync(It.IsAny<TransportOptions>(), It.IsAny<string>()))
                .ReturnsAsync("<myscript/>");

            var conn = new Connection(mock.Object, DefaultOptions);
            conn.Add("<cmd exec='cmd' error='fast'>WRKACTJOB</cmd>");
            await conn.RunAsync();

            // After run, list is cleared – a second run without adds must throw.
            Assert.Throws<InvalidOperationException>(() => conn.Run());
        }

        [Fact]
        public async Task RunAsync_MultipleCommandsJoinedWithComma()
        {
            string? capturedXml = null;
            var mock = new Mock<ITransport>();
            mock.Setup(t => t.CallAsync(It.IsAny<TransportOptions>(), It.IsAny<string>()))
                .Callback<TransportOptions, string>((_, xml) => capturedXml = xml)
                .ReturnsAsync("<myscript/>");

            var conn = new Connection(mock.Object, DefaultOptions);
            conn.Add(new CommandCall(new CommandCallConfig { Command = "CMD1", Type = "cl" }));
            conn.Add(new CommandCall(new CommandCallConfig { Command = "CMD2", Type = "cl" }));
            await conn.RunAsync();

            // The two XML elements are comma-joined (mirrors JS Array.join() default behaviour).
            Assert.NotNull(capturedXml);
            Assert.Contains(",", capturedXml!);
        }

        // ── Debug / GetTransportOptions ──────────────────────────────────────────

        [Fact]
        public void Debug_TogglesVerboseAndReturnsCurrentValue()
        {
            var conn = BuildConnection(out _);
            Assert.False(conn.Debug());
            Assert.True(conn.Debug(true));
            Assert.False(conn.Debug(false));
        }

        [Fact]
        public void GetTransportOptions_ReturnsOptions()
        {
            var opts = new TransportOptions { Username = "testuser" };
            var conn = new Connection(new Mock<ITransport>().Object, opts);
            Assert.Same(opts, conn.GetTransportOptions());
        }

        // ── Helper ───────────────────────────────────────────────────────────────

        private static Connection BuildConnection(out Mock<ITransport> mock)
        {
            mock = new Mock<ITransport>();
            mock.Setup(t => t.CallAsync(It.IsAny<TransportOptions>(), It.IsAny<string>()))
                .ReturnsAsync("<myscript/>");
            return new Connection(mock.Object, DefaultOptions);
        }
    }
}
