using System.Globalization;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Terminal.Gui.Cli.Survey;

/// <summary>
///     Builds the Spectre.Console renderable for a profile and renders it to a text writer.
///     Spectre is the rendering engine; Terminal.Gui (via the host and, in the TUI, the
///     Terminal.Gui.Interop.Spectre bridge) handles presentation and interaction.
/// </summary>
public static class SpectreProfile
{
    /// <summary>Builds a composed renderable (Panel + Table + BarChart) describing the profile.</summary>
    public static IRenderable Build (SurveyAnswers answers)
    {
        ArgumentNullException.ThrowIfNull (answers);

        Table table = new Table ()
            .Border (TableBorder.Rounded)
            .AddColumn (new TableColumn ("[bold]Field[/]"))
            .AddColumn (new TableColumn ("[bold]Value[/]"));

        var fruits = answers.Fruits.Count > 0 ? string.Join (", ", answers.Fruits) : "none";
        var color = answers.Color is null ? "[grey]unspecified[/]" : Markup.Escape (answers.Color);

        table.AddRow (new Markup ("Name"), new Markup ($"[green]{Markup.Escape (answers.Name)}[/]"));
        table.AddRow (new Markup ("Age"), new Markup (answers.Age.ToString (CultureInfo.InvariantCulture)));
        table.AddRow (new Markup ("Sport"), new Markup (Markup.Escape (answers.Sport)));
        table.AddRow (new Markup ("Fruits"), new Markup (Markup.Escape (fruits)));
        table.AddRow (new Markup ("Color"), new Markup (color));

        Panel panel = new Panel (table)
            .Header ($"[yellow]{Markup.Escape (answers.Name)}[/]", Justify.Center)
            .Border (BoxBorder.Double);

        BarChart chart = new BarChart ()
            .Width (44)
            .Label ("[bold]Metrics[/]")
            .AddItem ("Age", answers.Age, Color.Aqua)
            .AddItem ("Fruits", answers.Fruits.Count, Color.Green);

        return new Rows (panel, new Spectre.Console.Text (string.Empty), chart);
    }

    /// <summary>Renders the profile to <paramref name="writer" /> as ANSI (or plain text when not a terminal).</summary>
    public static void RenderToAnsi (SurveyAnswers answers, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull (writer);

        IAnsiConsole console = AnsiConsole.Create (new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput (writer),
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            Interactive = InteractionSupport.No
        });

        // When stdout is redirected (piped, --cat) Spectre cannot detect a terminal width,
        // so pin a deterministic one to avoid truncating the card.
        if (console.Profile.Width < 40)
        {
            console.Profile.Width = 100;
        }

        console.Write (Build (answers));
    }
}
