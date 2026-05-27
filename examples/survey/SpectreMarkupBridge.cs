// Vendored from gui-cs/Terminal.Gui Terminal.Gui.Interop.Spectre (not yet published as a NuGet package).
// Remove once Terminal.Gui.Interop.Spectre ships on NuGet.

using Spectre.Console;
using Terminal.Gui.Drawing;
using TgAttribute = Terminal.Gui.Drawing.Attribute;
using TgColor = Terminal.Gui.Drawing.Color;
using SpectreColor = Spectre.Console.Color;

namespace Terminal.Gui.Interop.Spectre;

/// <summary>
///     Converts between Spectre.Console styling and Terminal.Gui drawing attributes.
/// </summary>
public static class SpectreMarkupBridge
{
    /// <summary>
    ///     Converts a Spectre <see cref="Style" /> to a Terminal.Gui <see cref="TgAttribute" />.
    /// </summary>
    public static TgAttribute ToAttribute (this Style style)
    {
        TgColor foreground = SpectreColorToTg (style.Foreground);
        TgColor background = SpectreColorToTg (style.Background);
        TextStyle textStyle = DecorationToTextStyle (style.Decoration);

        return new TgAttribute (foreground, background, textStyle);
    }

    private static TgColor SpectreColorToTg (SpectreColor? color)
    {
        if (color is null)
        {
            return TgColor.None;
        }

        SpectreColor value = color.Value;

        if (value == SpectreColor.Default)
        {
            return TgColor.None;
        }

        return new TgColor (value.R, value.G, value.B);
    }

    private static TextStyle DecorationToTextStyle (Decoration? decoration)
    {
        if (decoration is null)
        {
            return TextStyle.None;
        }

        Decoration value = decoration.Value;
        TextStyle style = TextStyle.None;

        if ((value & Decoration.Bold) != 0)
        {
            style |= TextStyle.Bold;
        }

        if ((value & Decoration.Dim) != 0)
        {
            style |= TextStyle.Faint;
        }

        if ((value & Decoration.Italic) != 0)
        {
            style |= TextStyle.Italic;
        }

        if ((value & Decoration.Underline) != 0)
        {
            style |= TextStyle.Underline;
        }

        if ((value & Decoration.Invert) != 0)
        {
            style |= TextStyle.Reverse;
        }

        if ((value & (Decoration.SlowBlink | Decoration.RapidBlink)) != 0)
        {
            style |= TextStyle.Blink;
        }

        if ((value & Decoration.Strikethrough) != 0)
        {
            style |= TextStyle.Strikethrough;
        }

        return style;
    }
}
