namespace Terminal.Gui.Cli;

/// <summary>Default case-insensitive, duplicate-rejecting command registry.</summary>
public sealed class CommandRegistry : ICommandRegistry
{
    private readonly List<ICliCommand> _commands = [];
    private readonly Dictionary<string, ICliCommand> _commandsByAlias = new (StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IReadOnlyCollection<ICliCommand> All => _commands;

    /// <inheritdoc />
    public void Register (ICliCommand command)
    {
        ArgumentNullException.ThrowIfNull (command);

        if (!command.Aliases.Contains (command.PrimaryAlias, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException ("PrimaryAlias must be present in Aliases.");
        }

        foreach (var alias in command.Aliases)
        {
            if (string.IsNullOrWhiteSpace (alias))
            {
                throw new InvalidOperationException ("Command aliases must not be empty.");
            }

            if (_commandsByAlias.ContainsKey (alias))
            {
                throw new InvalidOperationException ($"Alias '{alias}' is already registered.");
            }
        }

        _commands.Add (command);

        foreach (var alias in command.Aliases)
        {
            _commandsByAlias.Add (alias, command);
        }
    }

    /// <inheritdoc />
    public bool TryResolve (string alias, out ICliCommand? command)
    {
        return _commandsByAlias.TryGetValue (alias, out command);
    }
}
