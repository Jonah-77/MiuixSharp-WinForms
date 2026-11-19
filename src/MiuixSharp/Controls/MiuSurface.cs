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
    }

    public int Radius { get => _radius; set { _radius = value; Invalidate(); } }
    public bool ShowOutline { get => _outline; set { _outline = value; Invalidate(); } }

    // No Region any more: clipping to a rounded Region steps every anti-aliased corner into
    // hard pixels (a frayed edge at 150 %). The backdrop fill below covers the area outside the
    // path instead, so the corners blend with the parent surface.
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using var b = new SolidBrush(MiuBackdrop.ResolveColor(this));
        e.Graphics.FillRectangle(b, ClientRectangle);
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
        // Stroke one pixel inside the fill so the 1 px outline is never half-covered by the
        // anti-aliased edge of the fill path.
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
}
