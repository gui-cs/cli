using Xunit;
using Terminal.Gui.App;

namespace Terminal.Gui.Cli.Tests;

public sealed class CliHostTests
{
    [Fact]
    public async Task RunAsync_OpenCli_WritesRegisteredBuiltIns ()
    {
        var host = new CliHost (options =>
        {
            options.ApplicationName = "sample";
            options.Version = "1.2.3";
        });
        using var stdout = new StringWriter ();
        using var stderr = new StringWriter ();

        int exitCode = await host.RunAsync (["--opencli"], TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        Assert.Contains ("\"name\":\"sample\"", stdout.ToString ());
        Assert.Contains ("\"alias\":\"help\"", stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task RunAsync_AgentGuideCat_WritesLiteralWithoutStartingTui ()
    {
        var host = new CliHost (options =>
        {
            options.AgentGuide = "# Guide";
            options.AgentGuideIsResource = false;
        });
        using var stdout = new StringWriter ();
        using var stderr = new StringWriter ();

        int exitCode = await host.RunAsync (["agent-guide", "--cat"], TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        Assert.Equal ("# Guide", stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task RunAsync_CommandCancellation_ReturnsCancelledExitCode ()
    {
        var host = new CliHost ();
        host.Registry.Register (new CancellingCatCommand ());
        using var stdout = new StringWriter ();
        using var stderr = new StringWriter ();

        int exitCode = await host.RunAsync (["cancel", "--cat"], TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Cancelled, exitCode);
        Assert.Equal (string.Empty, stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    private sealed class CancellingCatCommand : IViewerCommand
    {
        public string PrimaryAlias => "cancel";

        public IReadOnlyList<string> Aliases { get; } = ["cancel"];

        public string Description => "Cancels.";

        public CommandKind Kind => CommandKind.Viewer;

        public Type ResultType => typeof (void);

        public IReadOnlyList<CommandOptionDescriptor> Options { get; } = [];

        public Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options, CancellationToken cancellationToken)
        {
            throw new OperationCanceledException (cancellationToken);
        }

        public Task<CommandResult?> RenderCatAsync (CommandRunOptions options, TextWriter stdout, CancellationToken cancellationToken)
        {
            throw new OperationCanceledException (cancellationToken);
        }
    }
}
