# Terminal.Gui.Cli

![Terminal.Gui.Cli Example App](docs/images/hero.gif)

A .NET library that lets [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui) applications expose Views as scriptable CLI commands with typed JSON output, POSIX exit codes, and AI-agent discoverability.

Ships as a single NuGet package: **[`Terminal.Gui.Cli`](https://www.nuget.org/packages/Terminal.Gui.Cli)**.

## What it does

`Terminal.Gui.Cli` provides a hosting layer (`CliHost`) that wires up:

- **CLI parsing** — positional command dispatch, typed options, `--initial` pre-fill for input commands.
- **Structured output** — `--json` emits a versioned `JsonEnvelope`; `--cat` renders viewer content headlessly.
- **AI-agent discoverability** — `--opencli` emits machine-readable metadata; `--agent-guide` serves embedded Markdown guidance.
- **Built-in help** — `--help` renders command/option metadata via pluggable `IHelpProvider`.
- **Exit codes** — deterministic POSIX exit codes from `CommandResult` status.

## Command model

| Kind | Interface | Description |
|------|-----------|-------------|
| **Input** | `ICliCommand<T>` | Launches a Terminal.Gui UI, returns a typed result. |
| **Viewer** | `IViewerCommand` | Displays content; supports `--cat` for headless rendering. |

Commands register explicitly (no reflection scanning) and resolve by case-insensitive alias.

## Quickstart

```csharp
using Terminal.Gui.Cli;

CliHost host = new (options =>
{
    options.ApplicationName = "my-app";
    options.Version = "1.0.0";
});

host.Registry.Register (new MyCommand ());

return await host.RunAsync (args);
```

```sh
# Interactive (launches Terminal.Gui)
my-app greet --initial "World"

# JSON envelope
my-app greet --initial "World" --json

# Agent discovery
my-app --opencli
my-app --agent-guide

# Headless viewer
my-app info --cat
```

## Framework options

All commands inherit these options from the host:

| Option | Description |
|--------|-------------|
| `--help` / `-h` | Show help |
| `--version` | Show version |
| `--opencli` | Emit OpenCLI metadata JSON |
| `--agent-guide` | Emit embedded agent guide Markdown |
| `--json` | Wrap output in JSON envelope |
| `--initial <value>` | Pre-fill input value |
| `--timeout <duration>` | Cancel after duration (e.g., `30s`, `5m`) |
| `--output <path>` / `-o` | Write output to file |
| `--cat` | Headless render (viewer commands only) |

## Repository layout

```
specs/        Constitution and library spec
src/          Terminal.Gui.Cli library
tests/        Unit, integration, and smoke tests
examples/     Example console app
scripts/      Tooling and recording scripts
docs/         Images and documentation assets
```

## Build

Requires .NET 10 SDK. Solution file: `Terminal.Gui.Cli.slnx`.

```sh
dotnet restore Terminal.Gui.Cli.slnx
dotnet build   Terminal.Gui.Cli.slnx

# Tests
dotnet run --project tests/Terminal.Gui.Cli.Tests
dotnet run --project tests/Terminal.Gui.Cli.IntegrationTests
dotnet run --project tests/Terminal.Gui.Cli.SmokeTests

# Example app
dotnet run --project examples/Terminal.Gui.Cli.ExampleApp -- greet --initial "World" --json
```

## Status

**Alpha** — `0.1.0-develop` pre-release stream on the `develop` branch.

## License

MIT; see [`LICENSE`](LICENSE).
