namespace Terminal.Gui.Cli;

/// <summary>Outcome status of a command run.</summary>
public enum CommandStatus
{
    /// <summary>The command completed successfully.</summary>
    Ok,

    /// <summary>The user or caller cancelled the command.</summary>
    Cancelled,

    /// <summary>The command failed.</summary>
    Error,

    /// <summary>The command completed but produced no result.</summary>
    NoResult
}
