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
    /// <summary>Builds a composed renderable (Panel + Table) describing the profile.</summary>
    public static IRenderable Build (SurveyAnswers answers)
    {
        ArgumentNullException.ThrowIfNull (answers);

        Table table = new Table ()
            .Border (TableBorder.Rounded)
            .AddColumn (new TableColumn ("[bold]Question[/]"))
            .AddColumn (new TableColumn ("[bold]Answer[/]"));

        table.AddRow (new Markup ("Name"), new Markup ($"[green]{Markup.Escape (answers.Name)}[/]"));

        var favFruit = answers.FavoriteFruit ?? "none";
        table.AddRow (new Markup ("Favorite fruit"), new Markup (Markup.Escape (favFruit)));
        table.AddRow (new Markup ("Favorite sport"), new Markup (Markup.Escape (answers.Sport)));
        table.AddRow (new Markup ("Age"), new Markup (answers.Age.ToString (CultureInfo.InvariantCulture)));

        var password = answers.Password.Length > 0 ? new string ('*', answers.Password.Length) : "[grey]none[/]";
        table.AddRow (new Markup ("Password"), new Markup (password));

        var color = answers.Color is null ? "[grey]unspecified[/]" : Markup.Escape (answers.Color);
        table.AddRow (new Markup ("Favorite color"), new Markup (color));

        Panel panel = new Panel (table)
            .Header ("[yellow]Results[/]", Justify.Center)
            .Border (BoxBorder.Double);

        return panel;
    }

    /// <summary>Builds the results table without the outer panel border (for embedded use).</summary>
    public static IRenderable BuildTable (SurveyAnswers answers)
    {
        ArgumentNullException.ThrowIfNull (answers);

        Table table = new Table ()
            .Border (TableBorder.Rounded)
            .AddColumn (new TableColumn ("[bold]Question[/]"))
            .AddColumn (new TableColumn ("[bold]Answer[/]"));

        table.AddRow (new Markup ("Name"), new Markup ($"[green]{Markup.Escape (answers.Name)}[/]"));

        var favFruit = answers.FavoriteFruit ?? "none";
        table.AddRow (new Markup ("Favorite fruit"), new Markup (Markup.Escape (favFruit)));
        table.AddRow (new Markup ("Favorite sport"), new Markup (Markup.Escape (answers.Sport)));
        table.AddRow (new Markup ("Age"), new Markup (answers.Age.ToString (CultureInfo.InvariantCulture)));

        var password = answers.Password.Length > 0 ? new string ('*', answers.Password.Length) : "[grey]none[/]";
        table.AddRow (new Markup ("Password"), new Markup (password));

        var color = answers.Color is null ? "[grey]unspecified[/]" : Markup.Escape (answers.Color);
        table.AddRow (new Markup ("Favorite color"), new Markup (color));

        return table;
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

        if (console.Profile.Width < 40)
        {
            console.Profile.Width = 100;
        }

        console.Write (Build (answers));
    }
}
