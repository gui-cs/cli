using Terminal.Gui.App;
using Xunit;

namespace Terminal.Gui.Cli.Tests;

public sealed class MetadataHelpProviderTests
{
    [Fact]
    public void GetRootHelp_GeneratesMarkdownWithHeadings ()
    {
        CommandRegistry registry = new ();
        registry.Register (new StubCommand ("pick", "Pick something."));
        MetadataHelpProvider provider = new ();

        var result = provider.GetRootHelp (registry);

        Assert.NotNull (result);
        Assert.Contains ("## Commands", result);
        Assert.Contains ("## Framework options", result);
        Assert.Contains ("- `pick`", result);
        Assert.Contains ("`--help`", result);
    }

    [Fact]
    public void GetRootHelp_ListsAllRegisteredCommands ()
    {
        CommandRegistry registry = new ();
        registry.Register (new StubCommand ("alpha", "Alpha command."));
        registry.Register (new StubCommand ("beta", "Beta command."));
        MetadataHelpProvider provider = new ();

        var result = provider.GetRootHelp (registry);

        Assert.NotNull (result);
        Assert.Contains ("- `alpha` — Alpha command.", result);
        Assert.Contains ("- `beta` — Beta command.", result);
    }

    private sealed class StubCommand (string alias, string description) : ICliCommand
    {
        public string PrimaryAlias { get; } = alias;

        public IReadOnlyList<string> Aliases => [PrimaryAlias];

        public string Description => description;

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
