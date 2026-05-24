namespace Terminal.Gui.Cli;

/// <summary>Pluggable help rendering.</summary>
public interface IHelpProvider
{
    /// <summary>Renders root-level help. Return null to use generated fallback text.</summary>
    string? GetRootHelp (ICommandRegistry registry);

    /// <summary>Renders per-command help. Return null to use generated fallback text.</summary>
    string? GetCommandHelp (ICliCommand command);
}
