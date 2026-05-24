using Terminal.Gui.App;

namespace Terminal.Gui.Cli;

/// <summary>Non-interactive viewer command that prints the consumer's agent guide.</summary>
public sealed class AgentGuideCommand : IViewerCommand
{
    private readonly string _markdown;

    /// <summary>Creates an agent guide command from resolved markdown content.</summary>
    public AgentGuideCommand (string markdown)
    {
        _markdown = markdown ?? throw new ArgumentNullException (nameof (markdown));
    }

    /// <inheritdoc />
    public string PrimaryAlias => "agent-guide";

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; } = ["agent-guide"];

    /// <inheritdoc />
    public string Description => "Show the agent guide.";

    /// <inheritdoc />
    public CommandKind Kind => CommandKind.Viewer;

    /// <inheritdoc />
    public Type ResultType => typeof (string);

    /// <inheritdoc />
    public IReadOnlyList<CommandOptionDescriptor> Options { get; } = [];

    /// <inheritdoc />
    public Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        return Task.FromResult (new CommandResult (CommandStatus.Ok, _markdown, null, null));
    }

    /// <inheritdoc />
    public Task<CommandResult?> RenderCatAsync (CommandRunOptions options, TextWriter stdout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (stdout);
        stdout.Write (_markdown);
        return Task.FromResult<CommandResult?> (new CommandResult (CommandStatus.Ok, null, null, null));
    }
}
