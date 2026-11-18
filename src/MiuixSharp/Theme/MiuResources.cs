using System.Drawing;
using System.Drawing.Drawing2D;

namespace MiuixSharp.Theme;

/// <summary>
/// Shared GDI+ pen/brush pool.
///
/// Creating a <see cref="Pen"/> or <see cref="SolidBrush"/> is a comparatively expensive
/// GDI+ call, and the self-painted controls create several per paint. On an animating
/// control (progress spinner, slider drag) that is tens of allocations per second, all of
/// them short-lived. This pool hands out process-wide instances keyed by colour (and width
/// for pens) and is flushed whenever the theme or the scale changes, so a colour role can
/// never be served stale.
///
/// The returned objects are <b>shared</b>: never dispose or mutate them.
/// </summary>
public static class MiuResources
{
    private static readonly Dictionary<int, SolidBrush> Brushes = new();
    private static readonly Dictionary<(int Argb, int Width), Pen> Pens = new();
    private static readonly Dictionary<(int Argb, int Width), Pen> RoundPens = new();
    private static readonly object Gate = new();

    // A palette has 51 roles and a handful of widths; anything beyond this means a caller is
    // generating colours dynamically (gradients, user pickers) and the pool would just grow.
    private const int Limit = 256;

    /// <summary>Shared solid brush for <paramref name="color"/>. Do not dispose.</summary>
    public static SolidBrush Brush(Color color)
    {
        int key = color.ToArgb();
        lock (Gate)
        {
            if (Brushes.TryGetValue(key, out var b)) return b;
            if (Brushes.Count >= Limit) ClearCore();
            b = new SolidBrush(color);
            Brushes[key] = b;
            return b;
        }
    }

    /// <summary>Shared pen for <paramref name="color"/> at <paramref name="width"/> px. Do not dispose.</summary>
    public static Pen Pen(Color color, float width = 1f)
    {
        // Quantise the width: sub-pixel float noise would otherwise create a pen per paint.
        int w = Math.Max(1, (int)Math.Round(width * 4)); // quarter-pixel steps
        int key = color.ToArgb();
        lock (Gate)
        {
            if (Pens.TryGetValue((key, w), out var p)) return p;
            if (Pens.Count >= Limit) ClearCore();
            p = new Pen(color, w / 4f);
            Pens[(key, w)] = p;
            return p;
        }
    }

    /// <summary>Shared pen with round caps (track / slider strokes). Do not dispose.</summary>
    public static Pen RoundPen(Color color, float width)
    {
        int w = Math.Max(1, (int)Math.Round(width * 4));
        int key = color.ToArgb();
        lock (Gate)
        {
            if (RoundPens.TryGetValue((key, w), out var p)) return p;
            if (RoundPens.Count >= Limit) ClearCore();
            p = new Pen(color, w / 4f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            RoundPens[(key, w)] = p;
            return p;
        }
    }

    /// <summary>Drops every pooled object. Called on theme/scale changes and on demand.</summary>
    public static void Clear()
    {
        lock (Gate) ClearCore();
    }

    private static void ClearCore()
    {
        foreach (var b in Brushes.Values) b.Dispose();
        Brushes.Clear();
        foreach (var p in Pens.Values) p.Dispose();
        Pens.Clear();
        foreach (var p in RoundPens.Values) p.Dispose();
        RoundPens.Clear();
    }
}

/// <summary>Small cache for rounded-rect paths, which are rebuilt on every paint today.</summary>
public static class MiuPathCache
{
    private static readonly Dictionary<(int W, int H, int R, int Mode), GraphicsPath> Cache = new();
    private const int Limit = 64;

    /// <summary>
    /// Shared rounded-rect path (uniform radius). Do not dispose; the path is reused until
    /// <see cref="Clear"/> or the size changes.
    /// </summary>
    public static GraphicsPath Rounded(Rectangle r, int radius)
    {
        var key = (r.Width, r.Height, radius, 0);
        if (Cache.TryGetValue(key, out var p) && p != null) return p;
        if (Cache.Count >= Limit) Clear();
        p = MiuixSharp.Drawing.RoundedPath.Rect(r, radius);
        Cache[key] = p;
        return p;
    }

    /// <summary>Drops every cached path (size or radius changed too often, or theme flush).</summary>
    public static void Clear()
    {
        foreach (var p in Cache.Values) p.Dispose();
        Cache.Clear();
    }
}
