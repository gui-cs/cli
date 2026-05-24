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

`CommandResult` and `CommandResult<T>` intentionally live together in `CommandResult.cs`. `ICliCommand<TValue>` intentionally lives in `ICliCommandGeneric.cs`; do not use angle brackets in filenames.
