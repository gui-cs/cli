namespace Terminal.Gui.Cli;

/// <summary>
/// Viewer command. Viewers can be interactive TUI commands or headless content commands,
/// but they are invoked through the viewer path and default to fullscreen when a TUI is used.
/// </summary>
public interface IViewerCommand : ICliCommand
{
    /// <summary>
    /// Renders content to stdout without launching the TUI. Called when --cat is set.
    /// Return null to indicate --cat is not supported and normal TUI dispatch should continue.
    /// </summary>
    Task<CommandResult?> RenderCatAsync (
        CommandRunOptions options,
        TextWriter stdout,
        CancellationToken cancellationToken) => Task.FromResult<CommandResult?> (null);
}
