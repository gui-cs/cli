using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Terminal.Gui.Cli;

/// <summary>Interactive TUI markdown help viewer, with --cat support for ANSI stdout.</summary>
public sealed class HelpCommand : IViewerCommand
{
    private readonly IHelpProvider _helpProvider;
    private readonly ICommandRegistry _registry;

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
    public async Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        var markdown = ResolveHelp (options);
        var title = options.Title ?? "Help";

        Runnable window = new ()
        {
            Title = title,
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };

        Markdown markdownView = new ()
        {
            Width = Dim.Fill (),
            Height = Dim.Fill (1)
        };

        markdownView.LinkClicked += (_, e) =>
        {
            if (e.Url is not null && e.Url.StartsWith ("help:", StringComparison.OrdinalIgnoreCase))
            {
                var topic = e.Url["help:".Length..];
                var topicMarkdown = ResolveHelpTopic (topic);

                if (topicMarkdown is not null)
                {
                    markdownView.Text = topicMarkdown;
                    window.Title = $"Help - {topic}";
                }

                e.Handled = true;
            }
        };

        StatusBar statusBar = new (
        [
            new Shortcut (Application.GetDefaultKey (Command.Quit), "Quit", window.RequestStop)
        ]);

        window.Add (markdownView, statusBar);

        window.Initialized += (_, _) => { markdownView.Text = markdown; };

        await app.RunAsync (window, cancellationToken);

        return new CommandResult (CommandStatus.Ok, null, null, null);
    }

    /// <inheritdoc />
    public Task<CommandResult?> RenderCatAsync (CommandRunOptions options, TextWriter stdout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (stdout);
        MarkdownRenderer.RenderToAnsi (ResolveHelp (options), stdout);
        return Task.FromResult<CommandResult?> (new CommandResult (CommandStatus.Ok, null, null, null));
    }

    private string ResolveHelp (CommandRunOptions options)
    {
        if (options.Arguments.Count > 0 && _registry.TryResolve (options.Arguments[0], out ICliCommand? command) &&
            command is not null)
        {
            return _helpProvider.GetCommandHelp (command) ??
                   new MetadataHelpProvider ().GetCommandHelp (command) ?? string.Empty;
        }

        return _helpProvider.GetRootHelp (_registry) ??
               new MetadataHelpProvider ().GetRootHelp (_registry) ?? string.Empty;
    }

    private string? ResolveHelpTopic (string topic)
    {
        if (topic.Equals ("help", StringComparison.OrdinalIgnoreCase))
        {
            return _helpProvider.GetRootHelp (_registry) ??
                   new MetadataHelpProvider ().GetRootHelp (_registry);
        }

        if (_registry.TryResolve (topic, out ICliCommand? command) && command is not null)
        {
            return _helpProvider.GetCommandHelp (command) ??
                   new MetadataHelpProvider ().GetCommandHelp (command);
        }

        return null;
    }
}
