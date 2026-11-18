using System.Drawing;

namespace MiuixSharp.Theme;

/// <summary>
/// A faithful C# port of Miuix textStyles (docs/guide/textstyles.md).
/// sp≈pt at 96dpi; WinForms uses point sizes, so the sp numbers map near 1:1.
/// </summary>
public static class MiuTextStyles
{
    private const string Face = "Microsoft YaHei";

    private static Font F(float size, FontStyle style = FontStyle.Regular) => new(Face, size, style);

    public static Font Main => F(17);           // 17sp Normal
    public static Font Paragraph => F(17);      // 17sp Normal, lineHeight 1.2em
    public static Font Body1 => F(16);
    public static Font Body2 => F(14);
    public static Font Button => F(17);
    public static Font Footnote1 => F(13);
    public static Font Footnote2 => F(11);
    public static Font Headline1 => F(17);
    public static Font Headline2 => F(16);
    public static Font Subtitle => F(14, FontStyle.Bold);
    public static Font Title1 => F(32);
    public static Font Title2 => F(24);
    public static Font Title3 => F(20);
    public static Font Title4 => F(18);
}
