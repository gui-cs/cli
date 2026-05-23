namespace Terminal.Gui.Cli;

/// <summary>The two kinds of CLI commands the library knows about.</summary>
public enum CommandKind
{
    /// <summary>An interactive command that returns a typed value.</summary>
    Input,

    /// <summary>An interactive or headless command that does not return a typed result value.</summary>
    Viewer
}
