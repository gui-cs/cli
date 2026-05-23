namespace Terminal.Gui.Cli;

/// <summary>Parsed options bag passed to commands.</summary>
public sealed class CommandRunOptions
{
    /// <summary>Pre-fill value for the View.</summary>
    public string? Initial { get; init; }

    /// <summary>Title override for TUI chrome. --prompt/-p is an alias for --title/-t.</summary>
    public string? Title { get; init; }

    /// <summary>Whether to emit the JSON envelope instead of plain text.</summary>
    public bool JsonOutput { get; init; }

    /// <summary>Cancel after this duration.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Force fullscreen. Input commands otherwise default to inline.</summary>
    public bool Fullscreen { get; init; }

    /// <summary>Render supported viewer content to stdout instead of launching the TUI.</summary>
    public bool Cat { get; init; }

    /// <summary>Write successful command output to this file instead of stdout.</summary>
    public string? OutputPath { get; init; }

    /// <summary>Constrain inline height.</summary>
    public int? Rows { get; init; }

    /// <summary>Positional arguments after the alias.</summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>Per-command option values keyed by long option name without dashes.</summary>
    public IReadOnlyDictionary<string, string> CommandOptions { get; init; }
        = new Dictionary<string, string> ();

    /// <summary>Consumer-registered global option values keyed by long option name without dashes.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Extensions { get; init; }
        = new Dictionary<string, IReadOnlyList<string>> ();

    /// <summary>Gets the last value for a single-value consumer extension, parsed by <paramref name="parser" />.</summary>
    public T? GetExtension<T> (string key, Func<string, T> parser, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull (parser);

        if (!Extensions.TryGetValue (key, out IReadOnlyList<string>? values) || values.Count == 0)
        {
            return defaultValue;
        }

        return parser (values[^1]);
    }

    /// <summary>Gets all values for a repeatable consumer extension.</summary>
    public IReadOnlyList<string> GetExtensionList (string key)
    {
        return Extensions.TryGetValue (key, out IReadOnlyList<string>? values) ? values : [];
    }

    /// <summary>Returns true when a consumer extension flag or value is present.</summary>
    public bool HasExtension (string key)
    {
        return Extensions.ContainsKey (key);
    }
}
