using System.Drawing;
using System.Drawing.Drawing2D;

namespace MiuixSharp.Drawing;

/// <summary>
/// Small vector glyphs used by the input controls (chevron, calendar, plus/minus, cross).
/// They are drawn as strokes instead of text glyphs so they stay crisp at 125%/150%
/// scaling and never depend on a font that may be missing.
/// </summary>
public static class MiuGlyph
{
    /// <summary>A chevron pointing down (or up when <paramref name="up"/> is true).</summary>
    public static void Chevron(Graphics g, float cx, float cy, float width, Color color, bool up = false, float thickness = 1.6f)
    {
        float w = Math.Max(4f, width);
        float h = w * 0.55f;
        PointF[] pts = up
            ? new[] { new PointF(cx - w / 2, cy + h / 2), new PointF(cx, cy - h / 2), new PointF(cx + w / 2, cy + h / 2) }
            : new[] { new PointF(cx - w / 2, cy - h / 2), new PointF(cx, cy + h / 2), new PointF(cx + w / 2, cy - h / 2) };
        using var pen = new Pen(color, thickness)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round,
        };
        g.DrawLines(pen, pts);
    }

    /// <summary>A chevron pointing left or right.</summary>
    public static void Arrow(Graphics g, float cx, float cy, float size, Color color, bool right, float thickness = 1.6f)
    {
        float w = Math.Max(4f, size) * 0.55f;
        float h = Math.Max(4f, size);
        PointF[] pts = right
            ? new[] { new PointF(cx - w / 2, cy - h / 2), new PointF(cx + w / 2, cy), new PointF(cx - w / 2, cy + h / 2) }
            : new[] { new PointF(cx + w / 2, cy - h / 2), new PointF(cx - w / 2, cy), new PointF(cx + w / 2, cy + h / 2) };
        using var pen = new Pen(color, thickness)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round,
        };
        g.DrawLines(pen, pts);
    }

    /// <summary>A minimal calendar outline: rounded frame, header rule and three date dots.</summary>
    public static void Calendar(Graphics g, float cx, float cy, float size, Color color, float thickness = 1.4f)
    {
        float w = Math.Max(8f, size);
        float h = w * 0.92f;
        float x = cx - w / 2;
        float y = cy - h / 2;
        using var pen = new Pen(color, thickness) { LineJoin = LineJoin.Round };

        var box = Rectangle.Round(new RectangleF(x, y, w, h));
        using (var path = RoundedPath.Rect(box, 3)) g.DrawPath(pen, path);

        float ruleY = y + h * 0.33f;
        g.DrawLine(pen, x + 0.5f, ruleY, x + w - 0.5f, ruleY);

        using var dot = new SolidBrush(color);
        float r = Math.Max(0.8f, w * 0.055f);
        float dotY = y + h * 0.66f;
        for (int i = 0; i < 3; i++)
        {
            float dotX = x + w * (0.27f + i * 0.23f);
            g.FillEllipse(dot, dotX - r, dotY - r, r * 2, r * 2);
        }
    }

    public static void Plus(Graphics g, float cx, float cy, float size, Color color, float thickness = 1.6f)
    {
        float half = Math.Max(2f, size) / 2;
        using var pen = new Pen(color, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, cx - half, cy, cx + half, cy);
        g.DrawLine(pen, cx, cy - half, cx, cy + half);
    }

    public static void Minus(Graphics g, float cx, float cy, float size, Color color, float thickness = 1.6f)
    {
        float half = Math.Max(2f, size) / 2;
        using var pen = new Pen(color, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, cx - half, cy, cx + half, cy);
    }

    public static void Cross(Graphics g, float cx, float cy, float size, Color color, float thickness = 1.5f)
    {
        float half = Math.Max(2f, size) / 2;
        using var pen = new Pen(color, thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(pen, cx - half, cy - half, cx + half, cy + half);
        g.DrawLine(pen, cx + half, cy - half, cx - half, cy + half);
    }

    /// <summary>Filled circular backdrop used for hover states of in-field affordances.</summary>
    public static void HoverDisc(Graphics g, Rectangle bounds, Color color)
    {
        using var b = new SolidBrush(color);
        g.FillEllipse(b, bounds);
    }
}
