using Terminal.Gui.App;
using Xunit;

namespace Terminal.Gui.Cli.Tests;

public sealed class ArgParserTests
{
    [Fact]
    public void Parse_SeparatesFrameworkGlobalsCommandOptionsAndPositionals ()
    {
        var parser = new ArgParser ([new GlobalOptionDescriptor ("profile", "P", "Profile", false, true)]);
        var command = new TestCommand (acceptsPositionalArgs: true);

        ArgParser.ParseResult result = parser.Parse (["--profile", "dev", "pick", "--json", "--name", "value", "arg"], command);

        Assert.True (result.Success, result.Error);
        Assert.Equal ("pick", result.Alias);
        Assert.NotNull (result.Options);
        Assert.True (result.Options.JsonOutput);
        Assert.Equal ("value", result.Options.CommandOptions["name"]);
        Assert.Equal (["dev"], result.Options.GetExtensionList ("profile"));
        Assert.Equal (["arg"], result.Options.Arguments);
    }

    [Theory]
    [InlineData ("150ms", 150)]
    [InlineData ("2s", 2000)]
    public void TryParseTimeout_AcceptsSupportedSuffixes (string input, int milliseconds)
    {
        Assert.True (ArgParser.TryParseTimeout (input, out TimeSpan timeout));
        Assert.Equal (milliseconds, (int)timeout.TotalMilliseconds);
    }

    [Fact]
    public void Parse_RejectsMissingRequiredCommandOption ()
    {
        var parser = new ArgParser ([]);

        ArgParser.ParseResult result = parser.Parse (["pick"], new TestCommand (acceptsPositionalArgs: false));

        Assert.False (result.Success);
        Assert.Contains ("--name", result.Error);
    }

    private sealed class TestCommand (bool acceptsPositionalArgs) : ICliCommand
    {
        public string PrimaryAlias => "pick";

        public IReadOnlyList<string> Aliases { get; } = ["pick"];

        public string Description => "Test command.";

        public CommandKind Kind => CommandKind.Input;

        public Type ResultType => typeof (string);

        public IReadOnlyList<CommandOptionDescriptor> Options { get; } = [new ("name", "n", typeof (string), "Name", true, null)];

        public bool AcceptsPositionalArgs { get; } = acceptsPositionalArgs;

        public Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options, CancellationToken cancellationToken)
        {
            return Task.FromResult (new CommandResult (CommandStatus.Ok, "ok", null, null));
        }
    }
}
