using Terminal.Gui.App;
using Xunit;

namespace Terminal.Gui.Cli.Tests;

public sealed class CliHostTests
{
    [Fact]
    public async Task RunAsync_OpenCli_WritesRegisteredBuiltIns ()
    {
        CliHost host = new (options =>
        {
            options.ApplicationName = "sample";
            options.Version = "1.2.3";
        });
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await host.RunAsync (["--opencli"], TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        Assert.Contains ("\"name\":\"sample\"", stdout.ToString ());
        Assert.Contains ("\"alias\":\"help\"", stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task RunAsync_AgentGuideCat_WritesLiteralWithoutStartingTui ()
    {
        CliHost host = new (options =>
        {
            options.AgentGuide = "# Guide";
            options.AgentGuideIsResource = false;
        });
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await host.RunAsync (["agent-guide", "--cat"], TestContext.Current.CancellationToken, stdout,
            stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        Assert.Equal ("# Guide", stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task RunAsync_CommandCancellation_ReturnsCancelledExitCode ()
    {
        CliHost host = new ();
        host.Registry.Register (new CancellingCatCommand ());
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();
        using CancellationTokenSource cancellation = new ();
        cancellation.Cancel ();

        var exitCode = await host.RunAsync (["cancel", "--cat"], cancellation.Token, stdout, stderr);

        Assert.Equal (ExitCodes.Cancelled, exitCode);
        Assert.Equal (string.Empty, stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task RunAsync_HelpFlag_RendersMarkdownAsAnsi ()
    {
        CliHost host = new (options =>
        {
            options.ApplicationName = "test-app";
            options.Version = "1.0.0";
        });
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await host.RunAsync (["--help"], TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        var output = stdout.ToString ();
        // MarkdownRenderer.RenderToAnsi produces ANSI escape sequences
        Assert.Contains ("\x1b[", output);
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task RunAsync_HelpCat_RendersMarkdownAsAnsi ()
    {
        CliHost host = new (options =>
        {
            options.ApplicationName = "test-app";
            options.Version = "1.0.0";
        });
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await host.RunAsync (["help", "--cat"], TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        var output = stdout.ToString ();
        Assert.Contains ("\x1b[", output);
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

        public Task<CommandResult> RunAsync (IApplication app, string? initial, CommandRunOptions options,
            CancellationToken cancellationToken)
        {
            throw new OperationCanceledException (cancellationToken);
        }

        public Task<CommandResult?> RenderCatAsync (CommandRunOptions options, TextWriter stdout,
            CancellationToken cancellationToken)
        {
            throw new OperationCanceledException (cancellationToken);
        }
    }
}
