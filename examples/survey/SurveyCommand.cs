using System.Collections.ObjectModel;
using System.Globalization;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Interop.Spectre;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;
using Color = Spectre.Console.Color;

namespace Terminal.Gui.Cli.Survey;

/// <summary>
///     An input command that collects a person's profile via a Wizard and returns it as a typed
///     <see cref="SurveyAnswers" />. With <c>--name</c> (or other options) it runs headless and emits
///     a structured <c>--json</c> envelope; otherwise it launches an interactive Terminal.Gui Wizard.
/// </summary>
public sealed class SurveyCommand : ICliCommand<SurveyAnswers>
{
    /// <summary>Fruit catalog: (display label, value for output, whether the row is selectable).</summary>
    private static readonly (string Label, string Value, bool Selectable)[] Fruits =
    [
        ("Apple", "Apple", true),
        ("Apricot", "Apricot", true),
        ("Banana", "Banana", true),
        ("Berries:", "", false),
        ("    Blackberry", "Blackberry", true),
        ("    Blueberry", "Blueberry", true),
        ("    Raspberry", "Raspberry", true),
        ("    Strawberry", "Strawberry", true),
        ("Mango", "Mango", true),
        ("Orange", "Orange", true),
        ("Pear", "Pear", true)
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

        return new CommandResult<SurveyAnswers> (CommandStatus.Ok, captured, null, null);
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
            Height = Fruits.Length + 6, // tall enough for the largest step (fruits list + label + buttons)
            BorderStyle = LineStyle.Rounded,
            SchemeName = SchemeManager.SchemesToSchemeName (Schemes.Accent)
        };
        wizard.Border.Thickness = new Thickness (0, 1, 0, 0);

        // --- Step 1: Name ---
        WizardStep nameStep = new () { Title = "Name" };
        Label nameLabel = new () { Text = "_Name:" };
        TextField nameField = new ()
        {
            X = Pos.Right (nameLabel) + 1,
            Y = 0,
            Width = Dim.Percent (50)
        };
        nameStep.Add (nameLabel, nameField);
        wizard.AddStep (nameStep);

        // --- Step 2: Favorite Fruits (multi-select) ---
        WizardStep fruitsStep = new () { Title = "Fruits" };
        Label fruitsLabel = new () { Text = "_Favorite fruits (Space to toggle):" };
        var fruitChecked = new bool[Fruits.Length];
        ListView fruitsList = new ()
        {
            X = 0,
            Y = 1,
            Width = Dim.Auto (),
            Height = Dim.Fill ()
        };
        RefreshFruitDisplay (fruitsList, fruitChecked);

        fruitsList.Accepting += (_, args) =>
        {
            if (fruitsList.Value is { } idx and >= 0 && idx < Fruits.Length && Fruits[idx].Selectable)
            {
                fruitChecked[idx] = !fruitChecked[idx];
                RefreshFruitDisplay (fruitsList, fruitChecked);
            }

            args.Handled = true;
        };

        fruitsStep.Add (fruitsLabel, fruitsList);
        wizard.AddStep (fruitsStep);

        // --- Step 3: Conditional single-pick (only if >1 fruit selected) ---
        WizardStep favFruitStep = new () { Title = "Favorite Fruit" };
        Label favFruitLabel = new () { Text = "Ok, but if you could only choose _one:" };
        ListView favFruitList = new ()
        {
            X = 0,
            Y = 1,
            Width = Dim.Auto (),
            Height = Dim.Fill ()
        };
        favFruitStep.Add (favFruitLabel, favFruitList);
        wizard.AddStep (favFruitStep);

        // --- Step 4: Favorite Sport ---
        WizardStep sportStep = new () { Title = "Sport" };
        Label sportLabel = new () { Text = "Favorite _sport:" };
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
            Width = Dim.Percent (50)
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
            var text = (sportTextField.Text).Trim ();

            if (sportSelector.Value is not null &&
                !string.Equals (text, sportSelector.Labels![sportSelector.Value.Value],
                    StringComparison.OrdinalIgnoreCase))
            {
                sportSelector.Value = null;
            }
        };

        sportStep.Add (sportLabel, sportSelector, sportOrLabel, sportTextField);
        wizard.AddStep (sportStep);

        // --- Step 5: Age (validated) ---
        WizardStep ageStep = new () { Title = "Age" };
        Label ageLabel = new () { Text = "_Age (1-120):" };
        TextField ageField = new ()
        {
            X = Pos.Right (ageLabel) + 1,
            Y = 0,
            Width = Dim.Percent (50)
        };
        Label ageError = new ()
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill (),
            Visible = false
        };
        ageStep.Add (ageLabel, ageField, ageError);
        wizard.AddStep (ageStep);

        // --- Step 6: Password ---
        WizardStep passwordStep = new () { Title = "Password" };
        Label passwordLabel = new () { Text = "_Password:" };
        TextField passwordField = new ()
        {
            X = Pos.Right (passwordLabel) + 1,
            Y = 0,
            Width = Dim.Percent (50),
            Secret = true
        };
        passwordStep.Add (passwordLabel, passwordField);
        wizard.AddStep (passwordStep);

        // --- Step 7: Favorite Color ---
        WizardStep colorStep = new () { Title = "Color" };
        Label colorLabel = new () { Text = "Favorite _color:" };
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

        // --- Optional Step 8: Confirmation with Spectre card rendering ---
        WizardStep? confirmStep = null;
        SpectreView? confirmView = null;

        if (confirm)
        {
            confirmStep = new WizardStep { Title = "Confirm" };
            Label confirmLabel = new () { Text = "Review your answers and press Finish to _confirm:" };
            confirmView = new SpectreView
            {
                X = 0,
                Y = 2,
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };
            confirmStep.Add (confirmLabel, confirmView);
            wizard.AddStep (confirmStep);
        }

        // --- Step navigation ---
        wizard.StepChanged += (_, _) =>
        {
            if (wizard.CurrentStep == favFruitStep)
            {
                List<string> selected = GetSelectedFruits (fruitChecked);

                if (selected.Count <= 1)
                {
                    wizard.GoNext ();
                }
                else
                {
                    favFruitList.SetSource (new ObservableCollection<string> (selected));
                }
            }
            else if (wizard.CurrentStep == confirmStep && confirmView is not null)
            {
                SurveyAnswers preview = BuildAnswers (
                    nameField, fruitChecked, favFruitList, sportTextField, ageField, passwordField, colorPicker);

                // Get the background color from the wizard step so the table blends in
                Attribute attr = confirmStep!.GetAttributeForRole (VisualRole.Normal);
                Color? spectreBg = null;

                if (attr is { Background: var tgBg } && tgBg != Drawing.Color.None)
                {
                    spectreBg = new Color (tgBg.R, tgBg.G, tgBg.B);
                }

                confirmView.Renderable = SpectreProfile.Build (preview, spectreBg);
            }
        };

        // Validate age before allowing advancement past the age step
        wizard.MovingNext += (_, args) =>
        {
            if (wizard.CurrentStep != ageStep)
            {
                return;
            }

            var ageText = (ageField.Text).Trim ();

            if (ageText.Length == 0 ||
                !int.TryParse (ageText, NumberStyles.None, CultureInfo.InvariantCulture, out var age) ||
                age < 1 || age > 120)
            {
                ageError.Text = "Please enter a valid age between 1 and 120.";
                ageError.Visible = true;
                args.Cancel = true;
            }
            else
            {
                ageError.Visible = false;
            }
        };

        // Capture results when wizard finishes
        SurveyAnswers? result = null;

        wizard.Accepted += (_, _) =>
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
        var name = (nameField.Text).Trim ();
        List<string> selectedFruits = GetSelectedFruits (fruitChecked);

        var favoriteFruit = selectedFruits.Count == 1
            ? selectedFruits[0]
            : favFruitList.Value is >= 0 && favFruitList.Value < (favFruitList.Source?.Count ?? 0)
                ? favFruitList.Source!.ToList ()[favFruitList.Value.Value]?.ToString ()
                : selectedFruits.Count > 0
                    ? selectedFruits[0]
                    : null;

        var sport = (sportTextField.Text).Trim ();

        if (sport.Length == 0)
        {
            sport = "Unspecified";
        }

        var ageText = (ageField.Text).Trim ();
        int.TryParse (ageText, NumberStyles.None, CultureInfo.InvariantCulture, out var age);

        var password = passwordField.Text ?? string.Empty;
        var color = colorPicker.SelectedColor.ToString ();

        return new SurveyAnswers (name, selectedFruits, favoriteFruit, sport, age, password, color);
    }

    private static List<string> GetSelectedFruits (bool[] fruitChecked)
    {
        List<string> selected = [];

        for (var i = 0; i < fruitChecked.Length; i++)
        {
            if (fruitChecked[i] && Fruits[i].Selectable)
            {
                selected.Add (Fruits[i].Value);
            }
        }

        return selected;
    }

    private static void RefreshFruitDisplay (ListView list, bool[] fruitChecked)
    {
        IEnumerable<string> items = Fruits.Select ((f, i) =>
            f.Selectable
                ? fruitChecked[i] ? $"[x] {f.Label}" : $"[ ] {f.Label}"
                : $"    {f.Label}");
        list.SetSource (new ObservableCollection<string> (items));
    }
}
