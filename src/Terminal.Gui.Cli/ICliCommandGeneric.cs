using Terminal.Gui.App;

namespace Terminal.Gui.Cli;

/// <summary>Typed command that returns a value.</summary>
public interface ICliCommand<TValue> : ICliCommand
{
    /// <summary>Runs the command and returns a typed result.</summary>
    new Task<CommandResult<TValue>> RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken);

    async Task<CommandResult> ICliCommand.RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        CommandResult<TValue> result = await RunAsync (app, initial, options, cancellationToken);
        return new (result.Status, result.Value, result.ErrorCode, result.ErrorMessage);
    }
}
