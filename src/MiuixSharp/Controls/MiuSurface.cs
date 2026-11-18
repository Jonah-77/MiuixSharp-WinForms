using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A rounded surface container (Panel replacement) with Miuix surface color and
/// a thin outline. Unlike MiuCard it has no shadow padding, so child controls
/// dock right up to the rounded edge — ideal for wrapping tables and sections.
/// </summary>
public class MiuSurface : Panel
{
    private int _radius = 16;
    private bool _outline = true;

    public MiuSurface()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        MiuThemeHook.Attach(this, () => Invalidate());
        UpdateRegion();
    }

    public int Radius { get => _radius; set { _radius = value; UpdateRegion(); Invalidate(); } }
    public bool ShowOutline { get => _outline; set { _outline = value; Invalidate(); } }

    /// <summary>
    /// Clip the control to its rounded shape. Without this the areas outside the rounded
    /// corners keep whatever was painted before, which shows up as black corner artefacts
    /// on top of a light parent surface.
    /// </summary>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width < 2 || Height < 2) return;
        // ApplyRegion disposes the previous Region; assigning Region directly leaks a GDI
        // region on every resize.
        RoundedPath.ApplyRegion(this, MiuTheme.Dp(_radius));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        if (rect.Width <= 0 || rect.Height <= 0) return;
        using (var path = RoundedPath.Rect(rect, MiuTheme.Dp(_radius)))
        {
            using var b = new SolidBrush(MiuTheme.Surface);
            g.FillPath(b, path);
        }
        // The control clips itself to a rounded Region, so an edge-centred stroke is half
        // clipped (dashed outline at 150%). Stroke one pixel inside the fill instead.
        if (_outline)
        {
            var edge = new Rectangle(1, 1, Math.Max(0, rect.Width - 2), Math.Max(0, rect.Height - 2));
            if (edge.Width > 2 && edge.Height > 2)
                using (var path = RoundedPath.Rect(edge, Math.Max(3, MiuTheme.Dp(_radius) - 1)))
                using (var pen = new Pen(MiuTheme.Outline, 1))
                    g.DrawPath(pen, path);
        }
        base.OnPaint(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { /* transparent */ }
}
