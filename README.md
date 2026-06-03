# IToolkit for .NET

A .NET port of the [Node.js itoolkit](https://github.com/IBM/nodejs-itoolkit) — an XMLSERVICE wrapper for IBM i (AS/400).

Compatible with:
- **.NET Framework 4.8**
- **.NET 10+**

---

## Project Structure

```
├── IToolkit.sln
├── src/
│   └── IToolkit/               ← library
│       ├── Connection.cs
│       ├── ConnectionOptions.cs
│       ├── CommandCall.cs
│       ├── ProgramCall.cs
│       ├── ParameterConfig.cs
│       └── Transports/
│           ├── ITransport.cs
│           ├── TransportFactory.cs
│           ├── TransportOptions.cs
│           ├── HttpTransport.cs   ← REST/CGI transport
│           ├── SshTransport.cs    ← SSH transport
│           └── OdbcTransport.cs   ← ODBC transport
└── tests/
    └── IToolkit.Tests/         ← xUnit unit tests
```

---

## Getting Started

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

---

## Transports

| Name   | Description                                              | Required fields                        |
|--------|----------------------------------------------------------|----------------------------------------|
| `rest` | HTTP POST to the XMLSERVICE CGI endpoint (`xmlcgi.pgm`) | `Url`, `Username`, `Password`          |
| `ssh`  | SSH to IBM i, runs `xmlservice-cli` on stdin/stdout      | `Host`, `Username`, `Password` or `PrivateKey` |
| `odbc` | ODBC stored-procedure call (`iPLUGR512K`)                | IBM i Access ODBC Driver + `Host`/`Dsn`, `Username`, `Password` |

> **Note – ODBC on .NET 10+**: the `System.Data.Odbc` NuGet package (`8.0.0`) is
> included automatically via the project file. IBM i Access Client Solutions must
> be installed on the machine running the application.

---

## Usage

### CL Command (SSH transport)

```csharp
using IToolkit;
using IToolkit.Transports;
using System.Xml.Linq;

var conn = new Connection(new ConnectionOptions
{
    Transport = "ssh",
    TransportOptions = new TransportOptions
    {
        Host     = "myibmi",
        Username = "myuser",
        Password = "mypassword"
    }
});

conn.Add(new CommandCall(new CommandCallConfig
{
    Type    = "cl",
    Command = "RTVJOBA USRLIBL(?) SYSLIBL(?)"
}));

string xmlOutput = await conn.RunAsync();

// Parse with System.Xml.Linq
var doc = XDocument.Parse(xmlOutput);
Console.WriteLine(doc);
```

### Service Program Function Call (REST transport)

```csharp
using IToolkit;
using IToolkit.Transports;

var conn = new Connection(new ConnectionOptions
{
    Transport = "rest",
    TransportOptions = new TransportOptions
    {
        Url      = "http://myibmi:80/cgi-bin/xmlcgi.pgm",
        Username = "myuser",
        Password = "mypassword"
    }
});

// Call cos() from QSYS/QC2UTIL2
var program = new ProgramCall("QC2UTIL2", new ProgramCallOptions
{
    Lib  = "QSYS",
    Func = "cos"
});

program.AddParam(new ParameterConfig  { Type = "8f", Value = "0", By = "val" });
program.AddReturn(new ParameterConfig { Type = "8f", Value = "" });

conn.Add(program);
conn.Debug(true);   // print XML to stdout

string xmlOutput = await conn.RunAsync();

var doc = System.Xml.Linq.XDocument.Parse(xmlOutput);
var returnValue = doc.Descendants("data").Last().Value;
Console.WriteLine($"cos(0) = {returnValue}");  // 1
```

### Service Program Function Call (SSH transport)

This example calls a custom service program `MYLIB/MATHSRV` that exposes an
`ADD` function accepting two 4-byte integers and returning their sum.

```csharp
using IToolkit;
using IToolkit.Transports;
using System.Xml.Linq;

var conn = new Connection(new ConnectionOptions
{
    Transport = "ssh",
    TransportOptions = new TransportOptions
    {
        Host     = "myibmi",
        Username = "myuser",
        Password = "mypassword"
    }
});

// Call ADD(*int 4, *int 4) returns *int 4  from MYLIB/MATHSRV
var program = new ProgramCall("MATHSRV", new ProgramCallOptions
{
    Lib  = "MYLIB",
    Func = "ADD"
});

// Input parameters (passed by reference by default)
program.AddParam(new ParameterConfig { Type = "4i0", Value = "10" });
program.AddParam(new ParameterConfig { Type = "4i0", Value = "32" });

// Return value
program.AddReturn(new ParameterConfig { Type = "4i0", Value = "" });

conn.Add(program);

string xmlOutput = await conn.RunAsync();

// Parse the result — the return value is the last <data> element
var doc = XDocument.Parse(xmlOutput);
string result = doc.Descendants("data").Last().Value;
Console.WriteLine($"ADD(10, 32) = {result}");  // 42
```

**Common XMLSERVICE data types:**

| Type    | RPG equivalent | Description              |
|---------|----------------|--------------------------|
| `4i0`   | `10I 0`        | 4-byte signed integer    |
| `8i0`   | `20I 0`        | 8-byte signed integer    |
| `4u0`   | `10U 0`        | 4-byte unsigned integer  |
| `8f`    | `8F`           | 8-byte float (double)    |
| `32a`   | `32A`          | 32-char fixed string     |
| `varying2` | `varchar`   | Variable-length string   |
| `ds`    | `DS`           | Data structure           |

### ODBC Transport

```csharp
var conn = new Connection(new ConnectionOptions
{
    Transport = "odbc",
    TransportOptions = new TransportOptions
    {
        Host     = "myibmi",
        Username = "myuser",
        Password = "mypassword",
        XsLib    = "QXMLSERV"      // default
    }
});
```

Or with a DSN:

```csharp
TransportOptions = new TransportOptions { Dsn = "MY_DSN" }
```

### Custom / Testable Transport

```csharp
public class MyTransport : IToolkit.Transports.ITransport
{
    public Task<string> CallAsync(TransportOptions opts, string xml)
    {
        // your implementation
        return Task.FromResult("<myscript/>");
    }
}

var conn = new Connection(new MyTransport(), new TransportOptions());
```

---

## API Reference

### `Connection`

| Member | Description |
|--------|-------------|
| `Connection(ConnectionOptions)` | Create from named-transport options |
| `Connection(ITransport, TransportOptions, bool)` | Create with custom transport (useful for testing) |
| `void Add(CommandCall)` | Queue a command call |
| `void Add(ProgramCall)` | Queue a program call |
| `void Add(string xml)` | Queue raw XML |
| `Task<string> RunAsync()` | Send queued commands and return XML response |
| `string Run()` | Synchronous wrapper around `RunAsync` |
| `bool Debug(bool? flag)` | Get or set verbose mode |
| `TransportOptions GetTransportOptions()` | Return the transport options in use |

### `CommandCall`

```
new CommandCall(CommandCallConfig)
```

`CommandCallConfig`:

| Property | Type | Description |
|----------|------|-------------|
| `Command` | `string` | The command to run |
| `Type` | `string` | `"cl"`, `"sh"`, or `"qsh"` |
| `Options` | `ClOptions` or `ShOptions` | Optional per-type settings |

### `ProgramCall`

```
new ProgramCall(string program, ProgramCallOptions?)
```

| Method | Description |
|--------|-------------|
| `AddParam(ParameterConfig)` | Add an input/output parameter |
| `AddReturn(ParameterConfig)` | Specify the return-value type |
| `string ToXml()` | Get the generated XML |

### `ParameterConfig`

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `string` | XMLSERVICE data type or `"ds"` |
| `Value` | `string` | Data value |
| `Io` | `string?` | `"in"`, `"out"`, `"both"`, `"omit"` |
| `By` | `string?` | `"ref"` or `"val"` |
| `Fields` | `List<ParameterConfig>?` | Child elements for `"ds"` |

---

## License

MIT — see [LICENSE](LICENSE).

> Inspired by the [Node.js itoolkit](https://github.com/IBM/nodejs-itoolkit) project by IBM.
