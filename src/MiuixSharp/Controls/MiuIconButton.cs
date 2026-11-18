using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A 40dp fully-round icon button (Miuix IconButton default size), with a
/// text glyph as the icon for dependency-free usage.
/// </summary>
public class MiuIconButton : Control
{
    private bool _hover, _pressed;
    private MiuButtonStyle _style = MiuButtonStyle.Text;

    public MiuIconButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        // Miuix IconButtonDefaults.background is Color.Unspecified (i.e. nothing is painted):
        // the button sits on the parent surface. WinForms would otherwise give it the opaque
        // system control colour, which turns into a white block on a dark theme.
        BackColor = Color.Transparent;
        Size = new Size(MiuTheme.Dp(40), MiuTheme.Dp(40)); // IconButtonDefaults Size = 40dp
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI Symbol", 14F);
        MiuTheme.Changed += (_, __) => Invalidate();
    }

    public MiuButtonStyle Style { get => _style; set { _style = value; Invalidate(); } }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        Color back = Color.Transparent, fore = MiuTheme.Text;
        if (_style == MiuButtonStyle.Filled) { back = MiuTheme.Primary; fore = MiuTheme.OnPrimary; }
        if (_pressed) back = back != Color.Transparent ? RoundedPath.Blend(back, Color.Gray, 0.2) : MiuTheme.PrimaryContainer;
        else if (_hover) back = back != Color.Transparent ? RoundedPath.Blend(back, Color.White, 0.12) : MiuTheme.SurfaceAlt;

        using (var p = RoundedPath.Rect(rect, Math.Min(Width, Height) / 2))
        {
            if (back != Color.Transparent) using (var b = new SolidBrush(back)) g.FillPath(b, p);
        }
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var tb = new SolidBrush(fore);
        g.DrawString(Text, Font, tb, rect, sf);
    }
}
