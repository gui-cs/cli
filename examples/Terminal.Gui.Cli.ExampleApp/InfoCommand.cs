using Terminal.Gui.App;

namespace Terminal.Gui.Cli.ExampleApp;

/// <summary>A viewer command that displays application information.</summary>
public sealed class InfoCommand : IViewerCommand
{
    private const string InfoText = """
                                    Example App v1.0.0
                                    A demonstration of the Terminal.Gui.Cli library.

                                    This app shows how to:
                                    - Register input and viewer commands
                                    - Use --help, --json, --opencli, and agent-guide
                                    - Embed an agent-guide.md resource
                                    - Support --cat for headless content rendering
                                    """;

    /// <inheritdoc />
    public string PrimaryAlias => "info";

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; } = ["info"];

    /// <inheritdoc />
    public string Description => "Display application information.";

    /// <inheritdoc />
    public CommandKind Kind => CommandKind.Viewer;

    /// <inheritdoc />
    public Type ResultType => typeof (string);

    /// <inheritdoc />
    public IReadOnlyList<CommandOptionDescriptor> Options { get; } = [];

    /// <inheritdoc />
    public Task<CommandResult> RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        return Task.FromResult (new CommandResult (CommandStatus.Ok, InfoText, null, null));
    }

    /// <inheritdoc />
    public Task<CommandResult?> RenderCatAsync (
        CommandRunOptions options,
        TextWriter stdout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (stdout);
        stdout.Write (InfoText);
        return Task.FromResult<CommandResult?> (new CommandResult (CommandStatus.Ok, null, null, null));
    }
}
