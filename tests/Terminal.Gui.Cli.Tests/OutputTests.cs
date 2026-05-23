using System.Text.Json;
using Xunit;

namespace Terminal.Gui.Cli.Tests;

public sealed class OutputTests
{
    [Fact]
    public void JsonEnvelope_ToJson_UsesCamelCaseAndOmitsNulls ()
    {
        string json = JsonEnvelope.Ok ("value").ToJson ();

        using JsonDocument document = JsonDocument.Parse (json);
        Assert.Equal (1, document.RootElement.GetProperty ("schemaVersion").GetInt32 ());
        Assert.Equal ("ok", document.RootElement.GetProperty ("status").GetString ());
        Assert.Equal ("value", document.RootElement.GetProperty ("value").GetString ());
        Assert.False (document.RootElement.TryGetProperty ("code", out _));
    }

    [Fact]
    public void ResultWriter_WritesErrorsToStderrInPlainText ()
    {
        using var stdout = new StringWriter ();
        using var stderr = new StringWriter ();

        bool success = ResultWriter.Write (new CommandResult (CommandStatus.Error, null, "validation", "bad"), false, stdout, stderr);

        Assert.True (success);
        Assert.Equal (string.Empty, stdout.ToString ());
        Assert.Contains ("bad", stderr.ToString ());
    }

    [Fact]
    public void TerminalEscapeSanitizer_RemovesOscAndPreservesRenderedSgr ()
    {
        Assert.Equal ("title", TerminalEscapeSanitizer.Sanitize ("\u001b]0;bad\u0007title"));
        Assert.Equal ("\u001b[1mstrong\u001b[0m", TerminalEscapeSanitizer.SanitizeRenderedOutput ("\u001b[1mstrong\u001b[0m"));
    }
}
