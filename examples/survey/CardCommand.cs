using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Terminal.Gui.Cli.Survey;

/// <summary>
///     A viewer command that renders a profile as a rich Spectre.Console card. With <c>--cat</c> the
///     card is written to stdout by Spectre directly; in the TUI it is shown in a Terminal.Gui window.
/// </summary>
public sealed class CardCommand : IViewerCommand
{
    /// <inheritdoc />
    public string PrimaryAlias => "card";

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; } = ["card"];

    /// <inheritdoc />
    public string Description => "Render a profile as a rich Spectre.Console card.";

    /// <inheritdoc />
    public CommandKind Kind => CommandKind.Viewer;

    /// <inheritdoc />
    public Type ResultType => typeof (void);

    /// <inheritdoc />
    public IReadOnlyList<CommandOptionDescriptor> Options => ProfileInput.Options;

    /// <inheritdoc />
    public bool AcceptsPositionalArgs => true;

    /// <inheritdoc />
    public Task<CommandResult?> RenderCatAsync (
        CommandRunOptions options,
        TextWriter stdout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (stdout);

        if (!TryResolve (options, out SurveyAnswers answers, out var error))
        {
            return Task.FromResult<CommandResult?> (
                new CommandResult (CommandStatus.Error, null, "validation", error));
        }

        SpectreProfile.RenderToAnsi (answers, stdout);
        return Task.FromResult<CommandResult?> (new CommandResult (CommandStatus.Ok, null, null, null));
    }

    /// <inheritdoc />
    public async Task<CommandResult> RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (app);

        if (!TryResolve (options, out SurveyAnswers answers, out var error))
        {
            return new CommandResult (CommandStatus.Error, null, "validation", error);
        }

        Runnable window = new ()
        {
            Title = "Card",
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };

        // Phase A renders the profile as markdown in a Terminal.Gui view. Once the
        // Terminal.Gui.Interop.Spectre package is published this is replaced with a
        // SpectreView whose Renderable is SpectreProfile.Build (answers), so the exact
        // Spectre card shown by --cat also renders inside the TUI.
        Markdown content = new ()
        {
            Width = Dim.Fill (),
            Height = Dim.Fill (1)
        };

        StatusBar statusBar = new (
        [
            new Shortcut (Application.GetDefaultKey (Command.Quit), "Quit", window.RequestStop)
        ]);

        window.Add (content, statusBar);
        window.Initialized += (_, _) => { content.Text = ToMarkdown (answers); };

        await app.RunAsync (window, cancellationToken);
        return new CommandResult (CommandStatus.Ok, null, null, null);
    }

    private static bool TryResolve (CommandRunOptions options, out SurveyAnswers answers, out string? error)
    {
        if (!ProfileInput.TryBuild (options, null, out answers, out error))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace (answers.Name))
        {
            answers = ProfileInput.Sample;
        }

        return true;
    }

    private static string ToMarkdown (SurveyAnswers answers)
    {
        var fruits = answers.Fruits.Count > 0 ? string.Join (", ", answers.Fruits) : "none";
        var color = answers.Color ?? "unspecified";

        StringBuilder builder = new ();
        builder.AppendLine ($"# {Escape (answers.Name)}");
        builder.AppendLine ();
        builder.AppendLine ("| Field | Value |");
        builder.AppendLine ("|-------|-------|");
        builder.AppendLine ($"| Age | {answers.Age} |");
        builder.AppendLine ($"| Sport | {Escape (answers.Sport)} |");
        builder.AppendLine ($"| Fruits | {Escape (fruits)} |");
        builder.AppendLine ($"| Color | {Escape (color)} |");
        return builder.ToString ();
    }

    private static string Escape (string value)
    {
        return value.Replace ("|", "\\|", StringComparison.Ordinal);
    }
}
