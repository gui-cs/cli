namespace Terminal.Gui.Cli.Survey;

/// <summary>The structured result of the survey: a person's profile.</summary>
public sealed record SurveyAnswers (
    string Name,
    IReadOnlyList<string> Fruits,
    string? FavoriteFruit,
    string Sport,
    int Age,
    string Password,
    string? Color)
{
    /// <summary>A one-line, human-readable summary used for plain-text (non-JSON) output.</summary>
    public override string ToString ()
    {
        var fruits = Fruits.Count > 0 ? string.Join (", ", Fruits) : "none";
        var color = Color ?? "unspecified";
        return $"{Name}, age {Age} — likes {fruits}; plays {Sport}; favorite color {color}.";
    }
}
