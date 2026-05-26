using System.Text.Json;
using Terminal.Gui.Cli.Survey;
using Xunit;

namespace Terminal.Gui.Cli.IntegrationTests;

/// <summary>
///     Exercises the survey example end-to-end through the real configured host: headless input,
///     structured JSON (via the host's ResultJsonResolver), Spectre --cat rendering, and the card TUI.
/// </summary>
public sealed class SurveyExampleTests
{
    private static readonly string[] FullProfileArgs =
    [
        "survey", "--name", "Ada", "--age", "36", "--sport", "Fencing", "--fruits", "Apple,Cherry", "--color", "Teal"
    ];

    [Fact]
    public async Task Survey_Headless_ReturnsSummary ()
    {
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (FullProfileArgs, TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        Assert.Contains ("Ada", stdout.ToString ());
        Assert.Contains ("Fencing", stdout.ToString ());
        Assert.Equal (string.Empty, stderr.ToString ());
    }

    [Fact]
    public async Task Survey_Json_EmitsStructuredObject ()
    {
        string[] args = [.. FullProfileArgs, "--json"];
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (args, TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);

        using JsonDocument document = JsonDocument.Parse (stdout.ToString ());
        JsonElement value = document.RootElement.GetProperty ("value");
        Assert.Equal (JsonValueKind.Object, value.ValueKind);
        Assert.Equal ("Ada", value.GetProperty ("name").GetString ());
        Assert.Equal (36, value.GetProperty ("age").GetInt32 ());
        Assert.Equal ("Teal", value.GetProperty ("color").GetString ());
        Assert.Equal (2, value.GetProperty ("fruits").GetArrayLength ());
    }

    [Fact]
    public async Task Survey_Json_OmitsNullColor ()
    {
        string[] args = ["survey", "--name", "Bob", "--age", "20", "--json"];
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (args, TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);

        using JsonDocument document = JsonDocument.Parse (stdout.ToString ());
        JsonElement value = document.RootElement.GetProperty ("value");
        Assert.False (value.TryGetProperty ("color", out _));
    }

    [Fact]
    public async Task Survey_InvalidAge_ReturnsValidationError ()
    {
        string[] args = ["survey", "--name", "Ada", "--age", "999"];
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (args, TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.ValidationError, exitCode);
        Assert.Contains ("Invalid age", stderr.ToString ());
    }

    [Fact]
    public async Task Card_Cat_RendersSpectreCard ()
    {
        string[] args =
        [
            "card", "--name", "Ada", "--age", "36", "--sport", "Fencing", "--fruits", "Apple,Cherry", "--color",
            "Teal", "--cat"
        ];
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (args, TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        var output = stdout.ToString ();
        Assert.Contains ("Ada", output);
        Assert.Contains ("Field", output);
        Assert.Contains ("Metrics", output);
    }

    [Fact]
    public async Task Card_Cat_DefaultRendersSample ()
    {
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (["card", "--cat"], TestContext.Current.CancellationToken, stdout,
                stderr);

        Assert.Equal (ExitCodes.Ok, exitCode);
        Assert.Contains ("Ada Lovelace", stdout.ToString ());
    }

    [Fact]
    public async Task Card_Cat_InvalidAge_SurfacesErrorToStderr ()
    {
        string[] args = ["card", "--name", "Ada", "--age", "999", "--cat"];
        using StringWriter stdout = new ();
        using StringWriter stderr = new ();

        var exitCode = await SurveyApp.CreateHost ()
            .RunAsync (args, TestContext.Current.CancellationToken, stdout, stderr);

        Assert.Equal (ExitCodes.ValidationError, exitCode);
        Assert.Contains ("Invalid age", stderr.ToString ());
        Assert.Equal (string.Empty, stdout.ToString ());
    }
}
