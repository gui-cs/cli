using Terminal.Gui.App;
using Terminal.Gui.Cli;

namespace Terminal.Gui.Cli.ExampleApp;

/// <summary>An input command that prompts for a name and returns a greeting.</summary>
public sealed class GreetCommand : ICliCommand<string>
{
    /// <inheritdoc />
    public string PrimaryAlias => "greet";

    /// <inheritdoc />
    public IReadOnlyList<string> Aliases { get; } = ["greet"];

    /// <inheritdoc />
    public string Description => "Prompt for a name and return a greeting.";

    /// <inheritdoc />
    public CommandKind Kind => CommandKind.Input;

    /// <inheritdoc />
    public Type ResultType => typeof (string);

    /// <inheritdoc />
    public IReadOnlyList<CommandOptionDescriptor> Options { get; } =
    [
        new CommandOptionDescriptor ("formal", "f", typeof (bool), "Use a formal greeting style.", false, null)
    ];

    /// <inheritdoc />
    public Task<CommandResult<string>> RunAsync (
        IApplication app,
        string? initial,
        CommandRunOptions options,
        CancellationToken cancellationToken)
    {
        var name = initial ?? "World";
        var formal = options.CommandOptions.TryGetValue ("formal", out var formalValue)
                     && formalValue.Equals ("true", StringComparison.OrdinalIgnoreCase);

        var greeting = formal
            ? $"Good day, {name}. It is a pleasure to meet you."
            : $"Hello, {name}!";

        return Task.FromResult (new CommandResult<string> (CommandStatus.Ok, greeting, null, null));
    }
}
