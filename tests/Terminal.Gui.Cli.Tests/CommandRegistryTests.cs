using Terminal.Gui.App;
using Xunit;

namespace Terminal.Gui.Cli.Tests;

public sealed class CommandRegistryTests
{
    [Fact]
    public void Register_ResolvesAliasesCaseInsensitively ()
    {
        CommandRegistry registry = new ();
        TestCommand command = new ("pick", ["pick", "select"]);

        registry.Register (command);

        Assert.True (registry.TryResolve ("SELECT", out ICliCommand? resolved));
        Assert.Same (command, resolved);
    }

    [Fact]
    public void Register_RejectsDuplicateAliasesCaseInsensitively ()
    {
        CommandRegistry registry = new ();
        registry.Register (new TestCommand ("pick", ["pick"]));

        Assert.Throws<InvalidOperationException> (() =>
            registry.Register (new TestCommand ("other", ["PICK", "other"])));
    }

    private sealed class TestCommand (string primaryAlias, IReadOnlyList<string> aliases) : ICliCommand
    {
        public string PrimaryAlias { get; } = primaryAlias;

        public IReadOnlyList<string> Aliases { get; } = aliases;

        public string Description => "Test command.";

        public CommandKind Kind => CommandKind.Input;

        public Type ResultType => typeof (string);

        public IReadOnlyList<CommandOptionDescriptor> Options { get; } = [];

        public Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options,
            CancellationToken cancellationToken)
        {
            return Task.FromResult (new CommandResult (CommandStatus.Ok, "ok", null, null));
        }
    }
}
