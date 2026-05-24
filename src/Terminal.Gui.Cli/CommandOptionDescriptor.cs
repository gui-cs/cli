namespace Terminal.Gui.Cli;

/// <summary>Metadata descriptor for a per-command option.</summary>
public sealed record CommandOptionDescriptor (
    string Name,
    string? ShortName,
    Type ValueType,
    string Description,
    bool Required,
    string? DefaultValue);
