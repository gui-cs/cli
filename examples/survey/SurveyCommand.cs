using System.Collections.ObjectModel;
using System.Globalization;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Terminal.Gui.Cli.Survey;

/// <summary>
///     An input command that collects a person's profile via a Wizard and returns it as a typed
///     <see cref="SurveyAnswers" />. With <c>--name</c> (or other options) it runs headless and emits
///     a structured <c>--json</c> envelope; otherwise it launches an interactive Terminal.Gui Wizard.
/// </summary>
public sealed class SurveyCommand : ICliCommand<SurveyAnswers>
{
    private static readonly string[] AllFruits =
    [
        "Apple",
        "Apricot",
        "Banana",
        "  Blackberry",
        "  Blueberry",
        "  Raspberry",
        "  Strawberry",
        "Mango",
        "Orange",
        "Pear"
    ];

    private static readonly string[] FruitDisplayLabels =
    [
        "Apple",
        "Apricot",
        "Banana",
        "Berries:",
        "  Blackberry",
        "  Blueberry",
        "  Raspberry",
        "  Strawberry",
        "Mango",
        "Orange",
        "Pear"
    ];

    private static readonly bool[] FruitIsSelectable =
    [
        true, // Apple
        true, // Apricot
        true, // Banana
        false, // Berries: (header)
        true, // Blackberry
        true, // Blueberry
        true, // Raspberry
        true, // Strawberry
        true, // Mango
        true, // Orange
        true // Pear
    ];

    private static readonly string[] FruitValues =
    [
        "Apple",
        "Apricot",
        "Banana",
        "", // Berries header
        "Blackberry",
        "Blueberry",
        "Raspberry",
        "Strawberry",
        "Mango",
        "Orange",
        "Pear"
    ];

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

        SurveyAnswers? captured = await RunWizardAsync (app, cancellationToken);

        if (captured is null)
        {
            return new CommandResult<SurveyAnswers> (CommandStatus.Cancelled, null, null, null);
        }

        // Render the Spectre card to stdout after the wizard completes (like the Prompt example)
        SpectreProfile.RenderToAnsi (captured, Console.Out);
        return new CommandResult<SurveyAnswers> (CommandStatus.NoResult, captured, null, null);
    }

    private static async Task<SurveyAnswers?> RunWizardAsync (
        IApplication app,
        CancellationToken cancellationToken)
    {
        Wizard wizard = new ()
        {
            Title = "Survey - Enter to accept, Esc to quit",
            Width = Dim.Fill (),
            BorderStyle = LineStyle.Rounded
        };
        wizard.Border.Thickness = new Thickness (0, 1, 0, 0);

        // Step 1: Name
        WizardStep nameStep = new () { Title = "What is your name?" };
        TextField nameField = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill ()
        };
        nameStep.Add (nameField);
        wizard.AddStep (nameStep);

        // Step 2: Favorite Fruits (multi-select using marks)
        WizardStep fruitsStep = new () { Title = "What are your favorite fruits?" };
        var fruitChecked = new bool[FruitDisplayLabels.Length];
        ListView fruitsList = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };
        fruitsList.SetSource (new ObservableCollection<string> (
            FruitDisplayLabels.Select ((label, i) =>
                FruitIsSelectable[i] ? $"[ ] {label}" : $"    {label}")));

        fruitsList.Accepting += (_, args) =>
        {
            var idx = fruitsList.Value;

            if (idx is >= 0 && idx < FruitIsSelectable.Length && FruitIsSelectable[idx.Value])
            {
                fruitChecked[idx.Value] = !fruitChecked[idx.Value];
                UpdateFruitDisplay (fruitsList, fruitChecked);
            }

            args.Handled = true;
        };

        fruitsStep.Add (fruitsList);
        wizard.AddStep (fruitsStep);

        // Step 3: Conditional - "Ok, but if you could only choose one"
        WizardStep favFruitStep = new () { Title = "Ok, but if you could only choose one?" };
        ListView favFruitList = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };
        favFruitStep.Add (favFruitList);
        wizard.AddStep (favFruitStep);

        // Step 4: Favorite Sport
        WizardStep sportStep = new () { Title = "What is your favorite sport?" };
        OptionSelector sportSelector = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill (),
            Labels = ["Soccer", "Hockey", "Basketball"],
            Value = null
        };
        TextField sportTextField = new ()
        {
            X = 0,
            Y = Pos.Bottom (sportSelector) + 1,
            Width = Dim.Fill (),
            Title = "Or type your own:"
        };

        sportSelector.ValueChanged += (_, args) =>
        {
            if (args.NewValue is >= 0 && args.NewValue < sportSelector.Labels!.Count)
            {
                sportTextField.Text = sportSelector.Labels[args.NewValue.Value];
            }
        };

        sportTextField.TextChanged += (_, _) =>
        {
            var text = (sportTextField.Text ?? string.Empty).Trim ();
            var matchesOption = false;

            for (var i = 0; i < sportSelector.Labels!.Count; i++)
            {
                if (string.Equals (text, sportSelector.Labels[i], StringComparison.OrdinalIgnoreCase))
                {
                    matchesOption = true;

                    break;
                }
            }

            if (!matchesOption && sportSelector.Value is not null)
            {
                sportSelector.Value = null;
            }
        };

        sportStep.Add (sportSelector, sportTextField);
        wizard.AddStep (sportStep);

        // Step 5: Age
        WizardStep ageStep = new () { Title = "How old are you?" };
        TextField ageField = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill ()
        };
        ageStep.Add (ageField);
        wizard.AddStep (ageStep);

        // Step 6: Password
        WizardStep passwordStep = new () { Title = "Enter a password" };
        TextField passwordField = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill (),
            Secret = true
        };
        passwordStep.Add (passwordField);
        wizard.AddStep (passwordStep);

        // Step 7: Favorite Color
        WizardStep colorStep = new () { Title = "What is your favorite color?" };
        ColorPicker colorPicker = new ()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };
        colorPicker.Style.ShowTextFields = true;
        colorPicker.Style.ShowColorName = true;
        colorPicker.ApplyStyleChanges ();
        colorStep.Add (colorPicker);
        wizard.AddStep (colorStep);

        // Handle step navigation to conditionally skip favFruitStep
        wizard.StepChanged += (_, _) =>
        {
            if (wizard.CurrentStep == favFruitStep)
            {
                List<string> selectedFruits = GetSelectedFruits (fruitChecked);

                if (selectedFruits.Count <= 1)
                {
                    wizard.GoNext ();
                }
                else
                {
                    favFruitList.SetSource (new ObservableCollection<string> (selectedFruits));
                }
            }
        };

        SurveyAnswers? result = null;

        wizard.Accepting += (_, args) =>
        {
            var name = (nameField.Text ?? string.Empty).Trim ();
            List<string> selectedFruits = GetSelectedFruits (fruitChecked);
            var favoriteFruit = selectedFruits.Count == 1
                ? selectedFruits[0]
                : favFruitList.Value is >= 0 && favFruitList.Value < (favFruitList.Source?.Count ?? 0)
                    ? favFruitList.Source!.ToList ()[favFruitList.Value.Value]?.ToString ()
                    : selectedFruits.Count > 0
                        ? selectedFruits[0]
                        : null;

            var sport = (sportTextField.Text ?? string.Empty).Trim ();

            if (sport.Length == 0)
            {
                sport = "Unspecified";
            }

            var ageText = (ageField.Text ?? string.Empty).Trim ();
            int.TryParse (ageText, NumberStyles.None, CultureInfo.InvariantCulture, out var age);

            var password = passwordField.Text ?? string.Empty;
            var color = colorPicker.SelectedColor.ToString ();

            result = new SurveyAnswers (name, selectedFruits, favoriteFruit, sport, age, password, color);
            args.Handled = true;
        };

        await app.RunAsync (wizard, cancellationToken);
        return result;
    }

    private static List<string> GetSelectedFruits (bool[] fruitChecked)
    {
        List<string> selected = [];

        for (var i = 0; i < fruitChecked.Length; i++)
        {
            if (fruitChecked[i] && FruitIsSelectable[i])
            {
                selected.Add (FruitValues[i]);
            }
        }

        return selected;
    }

    private static void UpdateFruitDisplay (ListView list, bool[] fruitChecked)
    {
        IEnumerable<string> items = FruitDisplayLabels.Select ((label, i) =>
            FruitIsSelectable[i]
                ? fruitChecked[i] ? $"[x] {label}" : $"[ ] {label}"
                : $"    {label}");
        list.SetSource (new ObservableCollection<string> (items));
    }
}
