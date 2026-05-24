using Terminal.Gui.Views;

namespace Terminal.Gui.Cli;

/// <summary>Markdown-to-ANSI helper for help and viewer output.</summary>
public static class MarkdownRenderer
{
    /// <summary>Renders markdown as ANSI to <paramref name="output" /> and sanitizes rendered output.</summary>
    public static void RenderToAnsi (string markdown, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull (markdown);
        ArgumentNullException.ThrowIfNull (output);

        var rendered = new Markdown ().RenderToAnsi (markdown,
            Math.Max (1, Console.IsOutputRedirected ? 80 : Console.WindowWidth));
        output.Write (TerminalEscapeSanitizer.SanitizeRenderedOutput (rendered));
    }
}
