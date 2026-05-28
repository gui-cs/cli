# Terminal.Gui.Cli Library Specification

This repository implements the `Terminal.Gui.Cli` package API described by issue "Terminal.Gui.Cli Library Specification".

Public API additions must keep the following contracts aligned with implementation:

- Command model: `CommandKind`, `CommandStatus`, `CommandOptionDescriptor`, `CommandResult`, and `CommandResult<T>`.
- Command interfaces: `ICliCommand`, `ICliCommand<TValue>`, and `IViewerCommand`.
- Registry: `ICommandRegistry` and `CommandRegistry` with case-insensitive alias resolution and duplicate rejection.
- Host and parser: `CliHost`, `CliHostOptions`, `CommandRunOptions`, `GlobalOptionDescriptor`, and `ArgParser`.
- Help and built-ins: `IHelpProvider`, `MetadataHelpProvider`, `EmbeddedMarkdownHelpProvider`, `HelpCommand`, and `AgentGuideCommand`.
- Output and metadata: `JsonEnvelope`, `ResultWriter`, `OpenCliWriter`, `ExitCodes`, `TypeNames`, `TerminalEscapeSanitizer`, and `MarkdownRenderer`.
- Input helper: `InputCommandRunner`.

## Result value JSON serialization

The `--json` envelope serializes `CommandResult.Value` through the source-generated
`CliJsonContext` (constitution C4). That built-in context only resolves the library's own
value types, so consumer commands that return custom result types must supply a
source-generated resolver:

- `CliHostOptions.ResultJsonResolver` (`IJsonTypeInfoResolver?`) — a consumer
  `JsonSerializerContext` (or any resolver) registered on the host.
- `JsonEnvelope.ToJson(IJsonTypeInfoResolver?)` and the optional `resultJsonResolver`
  parameter on `ResultWriter.Write` thread that resolver through serialization.

The resolver is combined with `CliJsonContext` via `JsonTypeInfoResolver.Combine`, keeping
the path reflection-free and AOT-compatible. When `ResultJsonResolver` is null, envelope
values remain restricted to the built-in value types.

`CommandResult` and `CommandResult<T>` intentionally live together in `CommandResult.cs`. `ICliCommand<TValue>` intentionally lives in `ICliCommandGeneric.cs`; do not use angle brackets in filenames.
