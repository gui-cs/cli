using Terminal.Gui.App;

namespace Terminal.Gui.Cli;

/// <summary>A CLI command backed by Terminal.Gui. Implemented by consumer apps and built-ins.</summary>
public interface ICliCommand
{
    /// <summary>The canonical alias shown in help and OpenCLI output.</summary>
    string PrimaryAlias { get; }

    /// <summary>All aliases that resolve to this command. Must include <see cref="PrimaryAlias" />.</summary>
    IReadOnlyList<string> Aliases { get; }

    /// <summary>Human-readable one-line command description.</summary>
    string Description { get; }

    /// <summary>The command kind.</summary>
    CommandKind Kind { get; }

    /// <summary>The CLR type of the value written to the JSON envelope, or <see cref="void" />.</summary>
    Type ResultType { get; }

    /// <summary>Per-command options accepted by this command.</summary>
    IReadOnlyList<CommandOptionDescriptor> Options { get; }

    /// <summary>Whether this command consumes positional arguments.</summary>
    bool AcceptsPositionalArgs => false;

    /// <summary>
    /// Validates the --initial value before Terminal.Gui starts. The default permits any value;
    /// commands override this method when they need command-specific validation.
    /// </summary>
    bool TryValidateInitial (string initial, CommandRunOptions options) => true;

    /// <summary>Runs the command after the host has initialized Terminal.Gui.</summary>
    Task<CommandResult> RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken);
}
