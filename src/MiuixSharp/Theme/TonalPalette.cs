using System.Drawing;

namespace MiuixSharp.Theme;

/// <summary>
/// Generates a Material/Miuix-style tonal palette from a single seed color —
/// the "Monet" idea. A tone is a lightness level (10 darkest .. 95 lightest);
/// the style controls chroma and hue behaviour, mirroring Miuix ThemePaletteStyle.
/// </summary>
public sealed class TonalPalette
{
    private readonly double _hue;
    private readonly double _baseChroma;
    private readonly MiuPaletteStyle _style;

    public TonalPalette(Color seed, MiuPaletteStyle style = MiuPaletteStyle.TonalSpot)
    {
        ToHsl(seed, out var h, out var s, out _);
        _hue = h;
        _style = style;
        _baseChroma = style switch
        {
            MiuPaletteStyle.Neutral => 0.12,
            MiuPaletteStyle.Monochrome => 0.0,
            MiuPaletteStyle.Vibrant => Math.Clamp(Math.Max(0.9, s * 1.35), 0, 1),
            MiuPaletteStyle.Expressive => Math.Clamp(Math.Max(0.85, s * 1.15), 0, 1),
            _ => Math.Clamp(Math.Max(0.4, s), 0, 1) // TonalSpot
        };
    }

    /// <summary>Return the color at a given tone level (0 black .. 100 white).</summary>
    public Color Tone(int level)
    {
        level = Math.Clamp(level, 0, 100);
        double t = level / 100.0;
        double satShape = 1.0 - Math.Abs(2.0 * t - 1.0) * 0.55;
        double s = Math.Clamp(_baseChroma * satShape, 0, 1);
        // Vibrant/Expressive shift hue slightly toward the seed for a livelier palette.
        double h = _hue;
        if (_style == MiuPaletteStyle.Vibrant) h = (_hue + 12) % 360;
        else if (_style == MiuPaletteStyle.Expressive) h = (_hue + 6) % 360;
        return FromHsl(h, s, t);
    }

    public Color T10 => Tone(10);
    public Color T20 => Tone(20);
    public Color T40 => Tone(40);
    public Color T50 => Tone(50);
    public Color T80 => Tone(80);
    public Color T90 => Tone(90);
    public Color T95 => Tone(95);

    private static void ToHsl(Color c, out double h, out double s, out double l)
    {
        double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double d = max - min;
        l = (max + min) / 2.0;
        if (d < 1e-6) { h = 0; s = 0; return; }
        s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);
        if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
        else if (max == g) h = (b - r) / d + 2;
        else h = (r - g) / d + 4;
        h *= 60.0;
    }

    private static Color FromHsl(double h, double s, double l)
    {
        h = ((h % 360) + 360) % 360 / 360.0;
        double r, g, b;
        if (s < 1e-6) { r = g = b = l; }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = Hue2Rgb(p, q, h + 1.0 / 3);
            g = Hue2Rgb(p, q, h);
            b = Hue2Rgb(p, q, h - 1.0 / 3);
        }
        return Color.FromArgb(
            (int)Math.Round(r * 255),
            (int)Math.Round(g * 255),
            (int)Math.Round(b * 255));
    }

    private static double Hue2Rgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2) return q;
        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
        return p;
    }
}
