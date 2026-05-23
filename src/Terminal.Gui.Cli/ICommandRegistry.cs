namespace Terminal.Gui.Cli;

/// <summary>Manages alias-to-command lookup.</summary>
public interface ICommandRegistry
{
    /// <summary>Registers a command instance.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>PrimaryAlias</c> is not present in <c>Aliases</c>, or any alias is already registered.
    /// </exception>
    void Register (ICliCommand command);

    /// <summary>Resolves an alias case-insensitively.</summary>
    bool TryResolve (string alias, out ICliCommand? command);

    /// <summary>All registered commands in registration order.</summary>
    IReadOnlyCollection<ICliCommand> All { get; }
}
