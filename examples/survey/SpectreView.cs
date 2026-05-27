// Vendored from gui-cs/Terminal.Gui Terminal.Gui.Interop.Spectre (not yet published as a NuGet package).
// Remove once Terminal.Gui.Interop.Spectre ships on NuGet.

using Spectre.Console;
using Spectre.Console.Rendering;
using Terminal.Gui.Drawing;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using Size = System.Drawing.Size;
using TgAttribute = Terminal.Gui.Drawing.Attribute;

namespace Terminal.Gui.Interop.Spectre;

/// <summary>
///     A read-only <see cref="View" /> that renders a Spectre <see cref="IRenderable" />.
/// </summary>
public class SpectreView : View
{
    private static readonly IAnsiConsole _nullConsole = AnsiConsole.Create (new AnsiConsoleSettings
    {
        Out = new AnsiConsoleOutput (TextWriter.Null)
    });

    private bool _autoSize = true;

    private IRenderable? _renderable;

    /// <summary>
    ///     Gets or sets the Spectre renderable to display.
    /// </summary>
    public IRenderable? Renderable
    {
        get => _renderable;
        set
        {
            if (ReferenceEquals (_renderable, value))
            {
                return;
            }

            _renderable = value;
            UpdateContentSizeFromRenderable ();
            SetNeedsDraw ();
        }
    }

    /// <summary>
    ///     Gets or sets whether this view updates content size from the rendered Spectre content.
    /// </summary>
    public bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize == value)
            {
                return;
            }

            _autoSize = value;
            UpdateContentSizeFromRenderable ();
            SetNeedsDraw ();
        }
    }

    /// <inheritdoc />
    protected override void OnViewportChanged (DrawEventArgs e)
    {
        base.OnViewportChanged (e);
        UpdateContentSizeFromRenderable ();
    }

    /// <inheritdoc />
    protected override bool OnDrawingContent (DrawContext? context)
    {
        if (Renderable is null)
        {
            return true;
        }

        var maxWidth = Math.Max (Viewport.Width, 1);
        (IReadOnlyList<Segment> segments, _, _) = RenderSegments (Renderable, maxWidth);

        var row = 0;
        var col = 0;

        foreach (Segment segment in segments)
        {
            if (segment.IsLineBreak)
            {
                row++;
                col = 0;

                continue;
            }

            if (segment.IsControlCode || string.IsNullOrEmpty (segment.Text))
            {
                continue;
            }

            DrawSegment (segment, row, ref col);
        }

        return true;
    }

    private void DrawSegment (Segment segment, int row, ref int col)
    {
        if (row < Viewport.Y || row >= Viewport.Bottom)
        {
            col += segment.Text.GetColumns ();

            return;
        }

        TgAttribute attribute = segment.Style.ToAttribute ();

        foreach (var grapheme in GraphemeHelper.GetGraphemes (segment.Text))
        {
            var graphemeWidth = grapheme.GetColumns ();

            if (graphemeWidth > 0)
            {
                var visible = col + graphemeWidth > Viewport.X && col < Viewport.Right;

                if (visible)
                {
                    SetAttribute (attribute);
                    AddStr (col - Viewport.X, row - Viewport.Y, grapheme);
                }
            }

            col += graphemeWidth;
        }
    }

    private void UpdateContentSizeFromRenderable ()
    {
        if (!AutoSize)
        {
            SetContentSize (null);

            return;
        }

        if (Renderable is null)
        {
            SetContentSize (new Size (0, 0));

            return;
        }

        var maxWidth = Math.Max (Viewport.Width, 1);
        var (_, contentWidth, contentHeight) = RenderSegments (Renderable, maxWidth);
        SetContentSize (new Size (contentWidth, contentHeight));
    }

    private static (IReadOnlyList<Segment> Segments, int ContentWidth, int ContentHeight) RenderSegments (
        IRenderable renderable,
        int maxWidth)
    {
        RenderOptions renderOptions = RenderOptions.Create (_nullConsole);
        List<Segment> segments = [.. renderable.Render (renderOptions, maxWidth)];

        if (segments.Count == 0)
        {
            return (segments, 0, 0);
        }

        var maxLineWidth = 0;
        var lineWidth = 0;
        var lineCount = 1;

        foreach (Segment segment in segments)
        {
            if (segment.IsLineBreak)
            {
                maxLineWidth = Math.Max (maxLineWidth, lineWidth);
                lineWidth = 0;
                lineCount++;

                continue;
            }

            if (segment.IsControlCode || string.IsNullOrEmpty (segment.Text))
            {
                continue;
            }

            lineWidth += segment.Text.GetColumns ();
        }

        maxLineWidth = Math.Max (maxLineWidth, lineWidth);

        return (segments, maxLineWidth, lineCount);
    }
}
