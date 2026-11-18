using System.Drawing;
using System.Drawing.Drawing2D;

namespace MiuixSharp.Drawing;

/// <summary>Shared geometry helpers for rounded ("squircle-like") control shapes.</summary>
public static class RoundedPath
{
    /// <summary>A simple symmetric rounded rectangle.</summary>
    public static GraphicsPath Rect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = Math.Min(r.Width, r.Height);
        radius = Math.Clamp(radius, 0, d / 2);
        if (radius <= 0) { path.AddRectangle(r); return path; }

        int x = r.X, y = r.Y, w = r.Width, h = r.Height;
        path.AddArc(x, y, radius * 2, radius * 2, 180, 90);           // top-left
        path.AddArc(x + w - radius * 2, y, radius * 2, radius * 2, 270, 90); // top-right
        path.AddArc(x + w - radius * 2, y + h - radius * 2, radius * 2, radius * 2, 0, 90); // bottom-right
        path.AddArc(x, y + h - radius * 2, radius * 2, radius * 2, 90, 90);  // bottom-left
        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// Per-corner rounded rectangle. Each entry is a radius in pixels; 0 = square corner.
    /// Order: top-left, top-right, bottom-right, bottom-left.
    /// </summary>
    public static GraphicsPath Rect(Rectangle r, int tl, int tr, int br, int bl)
    {
        var path = new GraphicsPath();
        int d = Math.Min(r.Width, r.Height);
        tl = Math.Clamp(tl, 0, d / 2);
        tr = Math.Clamp(tr, 0, d / 2);
        br = Math.Clamp(br, 0, d / 2);
        bl = Math.Clamp(bl, 0, d / 2);
        int x = r.X, y = r.Y, w = r.Width, h = r.Height;

        if (tl > 0) path.AddArc(x, y, tl * 2, tl * 2, 180, 90);
        else path.AddLine(x, y, x, y);
        if (tr > 0) path.AddArc(x + w - tr * 2, y, tr * 2, tr * 2, 270, 90);
        else path.AddLine(x + w, y, x + w, y);
        if (br > 0) path.AddArc(x + w - br * 2, y + h - br * 2, br * 2, br * 2, 0, 90);
        else path.AddLine(x + w, y + h, x + w, y + h);
        if (bl > 0) path.AddArc(x, y + h - bl * 2, bl * 2, bl * 2, 90, 90);
        else path.AddLine(x, y + h, x, y + h);
        path.CloseFigure();
        return path;
    }

    /// <summary>Apply a rounded region to a control (clips its corners).</summary>
    public static void ApplyRegion(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0) return;
        using var p = Rect(new Rectangle(0, 0, control.Width, control.Height), radius);
        control.Region?.Dispose();
        control.Region = new Region(p);
    }

    /// <summary>Blend two colours; t=0 returns a, t=1 returns b.</summary>
    public static Color Blend(Color a, Color b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return Color.FromArgb(
            (int)Math.Round(a.R + (b.R - a.R) * t),
            (int)Math.Round(a.G + (b.G - a.G) * t),
            (int)Math.Round(a.B + (b.B - a.B) * t));
    }
}
