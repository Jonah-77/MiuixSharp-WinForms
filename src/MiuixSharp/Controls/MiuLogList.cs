using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// Self-drawn read-only log/history list. WinForms' multiline <c>TextBox</c> can only offer
/// the square system scrollbar, which breaks a rounded layout; this control draws its own
/// thin rounded scroll indicator and scrolls with the mouse wheel.
/// </summary>
public class MiuLogList : Control
{
    private readonly List<string> _lines = new();
    private int _scroll;
    private int _radius = 10;
    private int _lineHeight = 20;

    public MiuLogList()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Font = MiuTheme.Font;
        UpdateRegion();
        // Every painted colour comes from the live theme; without this the list keeps the
        // old palette after a Light/Dark switch.
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    /// <summary>Corner radius in pixels.</summary>
    public int Radius
    {
        get => _radius;
        set { _radius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
    }

    /// <summary>Height of one entry in pixels.</summary>
    public int LineHeight
    {
        get => _lineHeight;
        set { _lineHeight = Math.Max(12, value); Invalidate(); }
    }

    /// <summary>Fill of the list body. Leave unset to follow the current theme role.</summary>
    public Color SurfaceColor
    {
        get => _surfaceSet ? _surface : MiuTheme.Colors.SecondaryContainer;
        set { _surface = value; _surfaceSet = true; Invalidate(); }
    }

    private Color _surface;
    private bool _surfaceSet;

    /// <summary>Number of entries currently scrolled out of view.</summary>
    public int ScrollOffset => _scroll;

    public int VisibleLineCount =>
        Math.Max(1, (Height - MiuTheme.Dp(6)) / Math.Max(1, MiuTheme.Dp(_lineHeight)));

    public int MaxScroll => Math.Max(0, _lines.Count - VisibleLineCount);

    /// <summary>Replace the content.</summary>
    public void SetLines(IEnumerable<string> lines)
    {
        _lines.Clear();
        if (lines != null) _lines.AddRange(lines);
        _scroll = 0;
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
        _scroll = Math.Min(_scroll, MaxScroll);
        Invalidate();
    }

    private void UpdateRegion()
    {
        if (Width < 2 || Height < 2) return;
        RoundedPath.ApplyRegion(this, MiuTheme.Dp(_radius));
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (_lines.Count == 0) return;
        int step = Math.Max(1, e.Delta / 120) * 3;
        _scroll = Math.Max(0, Math.Min(MaxScroll, _scroll + (e.Delta < 0 ? step : -step)));
        Invalidate();
        base.OnMouseWheel(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        if (r.Width < 6 || r.Height < 6) return;

        using (var path = RoundedPath.Rect(r, MiuTheme.Dp(_radius)))
        using (var b = new SolidBrush(SurfaceColor))
            g.FillPath(b, path);

        int visible = VisibleLineCount;
        int lh = MiuTheme.Dp(_lineHeight);
        int y = MiuTheme.Dp(3);
        for (int i = _scroll; i < _lines.Count && i < _scroll + visible; i++)
        {
            var text = _lines[i] ?? "";
            // Reserve room for the scroll indicator on the right.
            var textRect = new Rectangle(MiuTheme.Dp(10), y, Math.Max(4, Width - MiuTheme.Dp(20)), lh);
            TextRenderer.DrawText(g, text, Font, textRect, MiuTheme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            y += lh;
        }

        if (_lines.Count > visible)
        {
            // Thin rounded indicator on the right edge, in the same language as the rest.
            int barW = MiuTheme.Dp(4);
            int trackX = Width - MiuTheme.Dp(7);
            int trackH = Math.Max(MiuTheme.Dp(6), Height - MiuTheme.Dp(8));
            using (var track = RoundedPath.Rect(new Rectangle(trackX, MiuTheme.Dp(4), barW, trackH), barW / 2))
            using (var tb = new SolidBrush(MiuTheme.Colors.DividerLine))
                g.FillPath(tb, track);

            double ratio = (double)visible / _lines.Count;
            int knobH = Math.Max(MiuTheme.Dp(18), (int)(trackH * ratio));
            int knobY = MiuTheme.Dp(4) + (int)((trackH - knobH) * (_scroll / (double)Math.Max(1, MaxScroll)));
            using (var knob = RoundedPath.Rect(new Rectangle(trackX, knobY, barW, knobH), barW / 2))
            using (var kb = new SolidBrush(MiuTheme.Colors.OnSurfaceVariantActions))
                g.FillPath(kb, knob);
        }
    }
}
