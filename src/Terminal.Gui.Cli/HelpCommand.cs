using Terminal.Gui.App;

namespace Terminal.Gui.Cli;

/// <summary>Interactive TUI markdown help viewer, with --cat support for ANSI stdout.</summary>
public sealed class HelpCommand : IViewerCommand
{
    private readonly ICommandRegistry _registry;
    private readonly IHelpProvider _helpProvider;

    /// <summary>Creates a help command that lazily reads command metadata from <paramref name="registry" />.</summary>
    public HelpCommand (ICommandRegistry registry, IHelpProvider helpProvider)
    {
        _registry = registry ?? throw new ArgumentNullException (nameof (registry));
        _helpProvider = helpProvider ?? throw new ArgumentNullException (nameof (helpProvider));
    }

    /// <inheritdoc />
    public string PrimaryAlias => "help";

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; } = ["help"];

    /// <inheritdoc />
    public string Description => "Show command help.";

    /// <inheritdoc />
    public CommandKind Kind => CommandKind.Viewer;

    /// <inheritdoc />
    public Type ResultType => typeof (void);

    /// <inheritdoc />
    public IReadOnlyList<CommandOptionDescriptor> Options { get; } = [];

    /// <inheritdoc />
    public bool AcceptsPositionalArgs => true;

    /// <inheritdoc />
    public Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options, CancellationToken cancellationToken)
    {
        string markdown = ResolveHelp (options);
        return Task.FromResult (new CommandResult (CommandStatus.Ok, markdown, null, null));
    }

    /// <inheritdoc />
    public Task<CommandResult?> RenderCatAsync (CommandRunOptions options, TextWriter stdout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (stdout);
        MarkdownRenderer.RenderToAnsi (ResolveHelp (options), stdout);
        return Task.FromResult<CommandResult?> (new CommandResult (CommandStatus.Ok, null, null, null));
    }

    private string ResolveHelp (CommandRunOptions options)
    {
        if (options.Arguments.Count > 0 && _registry.TryResolve (options.Arguments[0], out ICliCommand? command) && command is not null)
        {
            return _helpProvider.GetCommandHelp (command) ?? new MetadataHelpProvider ().GetCommandHelp (command) ?? string.Empty;
        }

        return _helpProvider.GetRootHelp (_registry) ?? new MetadataHelpProvider ().GetRootHelp (_registry) ?? string.Empty;
    }
}
