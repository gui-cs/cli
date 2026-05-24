namespace Terminal.Gui.Cli;

/// <summary>POSIX-conventional exit codes.</summary>
public static class ExitCodes
{
    /// <summary>Success.</summary>
    public const int Ok = 0;

    /// <summary>Successful command execution with no result.</summary>
    public const int NoResult = 1;

    /// <summary>Usage error: bad command, bad option, or output-file creation failure.</summary>
    public const int UsageError = 2;

    /// <summary>Validation error, equivalent to sysexits EX_DATAERR.</summary>
    public const int ValidationError = 65;

    /// <summary>I/O error, equivalent to sysexits EX_IOERR.</summary>
    public const int IoError = 74;

    /// <summary>Cancelled, equivalent to 128 + SIGINT.</summary>
    public const int Cancelled = 130;

    /// <summary>Maps a command result to a process exit code.</summary>
    public static int FromResult (CommandResult result)
    {
        return result.Status switch
        {
            CommandStatus.Ok => Ok,
            CommandStatus.NoResult => NoResult,
            CommandStatus.Cancelled => Cancelled,
            CommandStatus.Error => result.ErrorCode switch
            {
                "validation" => ValidationError,
                "io" => IoError,
                _ => UsageError
            },
            _ => UsageError
        };
    }
}
