using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
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

        var confirm = options.CommandOptions.ContainsKey ("confirm");
        SurveyAnswers? captured = await RunWizardAsync (app, confirm, cancellationToken);

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
        bool confirm,
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
        WizardStep nameStep = new () { Title = "Name" };
        Label nameLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "_Name:"
        };
        TextField nameField = new ()
        {
            X = Pos.Right (nameLabel) + 1,
            Y = 0,
            Width = Dim.Fill ()
        };
        nameField.Accepting += (_, args) => args.Handled = true;
        nameStep.Add (nameLabel, nameField);
        wizard.AddStep (nameStep);

        // Step 2: Favorite Fruits (multi-select using marks)
        WizardStep fruitsStep = new () { Title = "Fruits" };
        Label fruitsLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "_Favorite fruits (Space to toggle):"
        };
        var fruitChecked = new bool[FruitDisplayLabels.Length];
        ListView fruitsList = new ()
        {
            X = 0,
            Y = 1,
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

        fruitsStep.Add (fruitsLabel, fruitsList);
        wizard.AddStep (fruitsStep);

        // Step 3: Conditional - "Ok, but if you could only choose one"
        WizardStep favFruitStep = new () { Title = "Favorite Fruit" };
        Label favFruitLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "Ok, but if you could only choose _one:"
        };
        ListView favFruitList = new ()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };
        favFruitStep.Add (favFruitLabel, favFruitList);
        wizard.AddStep (favFruitStep);

        // Step 4: Favorite Sport
        WizardStep sportStep = new () { Title = "Sport" };
        Label sportLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "Favorite _sport:"
        };
        OptionSelector sportSelector = new ()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill (),
            Labels = ["Soccer", "Hockey", "Basketball"],
            Value = null
        };
        Label sportOrLabel = new ()
        {
            X = 0,
            Y = Pos.Bottom (sportSelector) + 1,
            Text = "Or _type your own:"
        };
        TextField sportTextField = new ()
        {
            X = Pos.Right (sportOrLabel) + 1,
            Y = Pos.Bottom (sportSelector) + 1,
            Width = Dim.Fill ()
        };
        sportTextField.Accepting += (_, args) => args.Handled = true;

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

        sportStep.Add (sportLabel, sportSelector, sportOrLabel, sportTextField);
        wizard.AddStep (sportStep);

        // Step 5: Age
        WizardStep ageStep = new () { Title = "Age" };
        Label ageLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "_Age (1-120):"
        };
        TextField ageField = new ()
        {
            X = Pos.Right (ageLabel) + 1,
            Y = 0,
            Width = Dim.Fill ()
        };
        ageField.Accepting += (_, args) => args.Handled = true;
        ageStep.Add (ageLabel, ageField);
        wizard.AddStep (ageStep);

        // Step 6: Password
        WizardStep passwordStep = new () { Title = "Password" };
        Label passwordLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "_Password:"
        };
        TextField passwordField = new ()
        {
            X = Pos.Right (passwordLabel) + 1,
            Y = 0,
            Width = Dim.Fill (),
            Secret = true
        };
        passwordField.Accepting += (_, args) => args.Handled = true;
        passwordStep.Add (passwordLabel, passwordField);
        wizard.AddStep (passwordStep);

        // Step 7: Favorite Color
        WizardStep colorStep = new () { Title = "Color" };
        Label colorLabel = new ()
        {
            X = 0,
            Y = 0,
            Text = "Favorite _color:"
        };
        ColorPicker colorPicker = new ()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill (),
            Height = Dim.Fill ()
        };
        colorPicker.Style.ShowTextFields = true;
        colorPicker.Style.ShowColorName = true;
        colorPicker.ApplyStyleChanges ();
        colorStep.Add (colorLabel, colorPicker);
        wizard.AddStep (colorStep);

        // Optional Step 8: Confirmation (only if --confirm is set)
        WizardStep? confirmStep = null;
        Label? confirmContentLabel = null;

        if (confirm)
        {
            confirmStep = new WizardStep { Title = "Confirm" };
            Label confirmLabel = new ()
            {
                X = 0,
                Y = 0,
                Text = "Review your answers and press Finish to _confirm:"
            };
            confirmContentLabel = new Label
            {
                X = 0,
                Y = 2,
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };
            confirmStep.Add (confirmLabel, confirmContentLabel);
            wizard.AddStep (confirmStep);
        }

        // Handle step navigation to conditionally skip favFruitStep and populate confirm
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
            else if (confirm && wizard.CurrentStep == confirmStep && confirmContentLabel is not null)
            {
                // Build preview text for confirmation
                SurveyAnswers preview = BuildAnswers (
                    nameField, fruitChecked, favFruitList, sportTextField, ageField, passwordField, colorPicker);
                confirmContentLabel.Text = FormatPreview (preview);
            }
        };

        SurveyAnswers? result = null;

        wizard.Accepting += (_, _) =>
        {
            result = BuildAnswers (
                nameField, fruitChecked, favFruitList, sportTextField, ageField, passwordField, colorPicker);
        };

        await app.RunAsync (wizard, cancellationToken);
        return result;
    }

    private static SurveyAnswers BuildAnswers (
        TextField nameField,
        bool[] fruitChecked,
        ListView favFruitList,
        TextField sportTextField,
        TextField ageField,
        TextField passwordField,
        ColorPicker colorPicker)
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

        return new SurveyAnswers (name, selectedFruits, favoriteFruit, sport, age, password, color);
    }

    private static string FormatPreview (SurveyAnswers answers)
    {
        StringBuilder sb = new ();
        sb.AppendLine ($"Name:     {answers.Name}");
        sb.AppendLine ($"Fruits:   {string.Join (", ", answers.Fruits)}");

        if (answers.FavoriteFruit is not null)
        {
            sb.AppendLine ($"Favorite: {answers.FavoriteFruit}");
        }

        sb.AppendLine ($"Sport:    {answers.Sport}");
        sb.AppendLine ($"Age:      {answers.Age}");
        sb.AppendLine ($"Password: {new string ('*', answers.Password.Length)}");
        sb.AppendLine ($"Color:    {answers.Color}");

        return sb.ToString ();
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
