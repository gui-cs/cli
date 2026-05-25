using System.Globalization;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Terminal.Gui.Cli.Survey;

/// <summary>
///     An input command that collects a person's profile and returns it as a typed
///     <see cref="SurveyAnswers" />. With <c>--name</c> (or other options) it runs headless and emits
///     a structured <c>--json</c> envelope; otherwise it launches an interactive Terminal.Gui form.
/// </summary>
public sealed class SurveyCommand : ICliCommand<SurveyAnswers>
{
    /// <inheritdoc />
    public string PrimaryAlias => "survey";

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; } = ["survey"];

    /// <inheritdoc />
    public string Description => "Collect a profile and return it as structured data.";

    /// <inheritdoc />
    public CommandKind Kind => CommandKind.Input;

    /// <inheritdoc />
    public Type ResultType => typeof (SurveyAnswers);

    /// <inheritdoc />
    public IReadOnlyList<CommandOptionDescriptor> Options => ProfileInput.Options;

    /// <inheritdoc />
    public bool AcceptsPositionalArgs => true;

    /// <inheritdoc />
    public async Task<CommandResult<SurveyAnswers>> RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull (app);

        if (!ProfileInput.TryBuild (options, initial, out SurveyAnswers answers, out var error))
        {
            return new CommandResult<SurveyAnswers> (CommandStatus.Error, null, "validation", error);
        }

        if (!string.IsNullOrWhiteSpace (answers.Name))
        {
            return new CommandResult<SurveyAnswers> (CommandStatus.Ok, answers, null, null);
        }

        if (Console.IsInputRedirected)
        {
            return new CommandResult<SurveyAnswers> (
                CommandStatus.Error,
                null,
                "validation",
                "A name is required. Pass --name in non-interactive mode.");
        }

        SurveyAnswers? captured = await RunFormAsync (app, answers, cancellationToken);

        return captured is null
            ? new CommandResult<SurveyAnswers> (CommandStatus.Cancelled, null, null, null)
            : new CommandResult<SurveyAnswers> (CommandStatus.Ok, captured, null, null);
    }

    private static async Task<SurveyAnswers?> RunFormAsync (
        IApplication app,
        SurveyAnswers defaults,
        CancellationToken cancellationToken)
    {
        Runnable window = new ()
        {
            Title = "Survey",
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };

        TextField nameField = Field (0, defaults.Name);
        TextField fruitsField = Field (2, string.Join (", ", defaults.Fruits));
        TextField sportField = Field (4, defaults.Sport == "Unspecified" ? string.Empty : defaults.Sport);
        TextField ageField = Field (6,
            defaults.Age > 0 ? defaults.Age.ToString (CultureInfo.InvariantCulture) : string.Empty);
        TextField colorField = Field (8, defaults.Color ?? string.Empty);

        Label errorLabel = new ()
        {
            X = 0,
            Y = 10,
            Width = Dim.Fill (),
            Text = string.Empty
        };

        window.Add (
            Caption (0, "Name:"),
            nameField,
            Caption (2, "Fruits:"),
            fruitsField,
            Caption (4, "Sport:"),
            sportField,
            Caption (6, "Age:"),
            ageField,
            Caption (8, "Color:"),
            colorField,
            errorLabel);

        SurveyAnswers? captured = null;

        void Submit ()
        {
            var name = (nameField.Text ?? string.Empty).Trim ();

            if (string.IsNullOrWhiteSpace (name))
            {
                errorLabel.Text = "Name is required.";
                return;
            }

            var ageText = (ageField.Text ?? string.Empty).Trim ();
            var age = 0;

            if (ageText.Length > 0 &&
                (!int.TryParse (ageText, NumberStyles.None, CultureInfo.InvariantCulture, out age) || age < 1 ||
                 age > 120))
            {
                errorLabel.Text = "Age must be a whole number between 1 and 120.";
                return;
            }

            var fruits = (fruitsField.Text ?? string.Empty)
                .Split (',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var sport = (sportField.Text ?? string.Empty).Trim ();
            var color = (colorField.Text ?? string.Empty).Trim ();

            captured = new SurveyAnswers (
                name,
                fruits,
                sport.Length > 0 ? sport : "Unspecified",
                age,
                color.Length > 0 ? color : null);

            window.RequestStop ();
        }

        StatusBar statusBar = new (
        [
            new Shortcut (Key.F2, "Submit", Submit),
            new Shortcut (Application.GetDefaultKey (Command.Quit), "Quit", window.RequestStop)
        ]);

        window.Add (statusBar);

        await app.RunAsync (window, cancellationToken);
        return captured;

        static TextField Field (int y, string text)
        {
            return new TextField
            {
                X = 10,
                Y = y,
                Width = Dim.Fill (),
                Text = text
            };
        }

        static Label Caption (int y, string text)
        {
            return new Label
            {
                X = 0,
                Y = y,
                Text = text
            };
        }
    }
}
