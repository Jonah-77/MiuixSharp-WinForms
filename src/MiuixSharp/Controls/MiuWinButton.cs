using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A rounded button that DERIVES FROM System.Windows.Forms.Button, so it drops
/// into existing WinForms code (FlatAppearance, DialogResult, Dock, etc.) with
/// zero changes while painting a Miuix-style rounded surface.
/// This is the compatibility bridge used to theme legacy WinForms apps.
/// </summary>
[System.Drawing.ToolboxBitmap(typeof(MiuWinButton), "MiuixSharp.Icons.MiuWinButton.bmp")]
[System.ComponentModel.Description("可主题化的原生 Button，用于替换旧窗体里的系统按钮")]
[System.ComponentModel.DefaultEvent("Click")]
public class MiuWinButton : Button
{
    private bool _hover, _pressed;
    private int _radius = 8;
    private MiuButtonStyle _miuStyle = MiuButtonStyle.Outlined;
    private Color? _forcedBack, _forcedFore, _forcedBorder;

    public MiuWinButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = MiuTheme.Font;
        Cursor = Cursors.Hand;
        Padding = new Padding(6, 0, 6, 0);
        MiuThemeHook.Attach(this, () => Invalidate());
        // Clip to rounded region so no native square focus frame / corners leak through.
        HandleCreated += (_, __) => ApplyRegion();
        Resize += (_, __) => ApplyRegion();
    }

    private void ApplyRegion()
    {
        if (Width <= 0 || Height <= 0) return;
        using var path = RoundedPath.Rect(new Rectangle(0, 0, Width - 1, Height - 1), MiuTheme.Dp(_radius));
        Region?.Dispose();
        Region = new Region(path);
    }

    /// <summary>Corner radius in pixels. Default 8.</summary>
    [DefaultValue(8)]
    public int Radius { get => _radius; set { _radius = value; ApplyRegion(); Invalidate(); } }

    public MiuButtonStyle MiuStyle
    {
        get => _miuStyle;
        set { _miuStyle = value; _forcedBack = _forcedFore = _forcedBorder = null; Invalidate(); }
    }

    /// <summary>Override colors (used by Theme factories that pre-compute colors).</summary>
    public void SetColors(Color back, Color fore, Color border)
    {
        _forcedBack = back; _forcedFore = fore; _forcedBorder = border; Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        // Deliberately skip base.OnPaint to avoid the native square button frame.
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        if (rect.Width <= 0 || rect.Height <= 0) return;

        Color back, fore, border;
        if (_forcedBack.HasValue) { back = _forcedBack.Value; fore = _forcedFore ?? MiuTheme.Text; border = _forcedBorder ?? Color.Transparent; }
        else
        {
            switch (_miuStyle)
            {
                case MiuButtonStyle.Filled: back = MiuTheme.Primary; fore = MiuTheme.OnPrimary; border = Color.Transparent; break;
                case MiuButtonStyle.Tonal: back = MiuTheme.PrimaryContainer; fore = MiuTheme.OnPrimaryContainer; border = Color.Transparent; break;
                case MiuButtonStyle.Warning: back = MiuTheme.Surface; fore = MiuTheme.Warning; border = MiuTheme.Warning; break;
                case MiuButtonStyle.Text: back = Color.Transparent; fore = MiuTheme.Primary; border = Color.Transparent; break;
                default: back = MiuTheme.Surface; fore = MiuTheme.Text; border = MiuTheme.Outline; break;
            }
        }

        if (_pressed) back = RoundedPath.Blend(back == Color.Transparent ? MiuTheme.Surface : back, MiuTheme.Primary, 0.12);
        else if (_hover) back = RoundedPath.Blend(back == Color.Transparent ? MiuTheme.Surface : back, MiuTheme.Primary, 0.06);

        // A stroke centred on the same path the Region clips is half-clipped: at 150% that
        // renders as a dashed border. Fill with the full path, stroke one pixel inside it.
        using (var path = RoundedPath.Rect(rect, MiuTheme.Dp(_radius)))
        {
            if (back != Color.Transparent) using (var b = new SolidBrush(back)) g.FillPath(b, path);
        }
        var edge = new Rectangle(1, 1, Math.Max(0, rect.Width - 2), Math.Max(0, rect.Height - 2));
        if (border != Color.Transparent && edge.Width > 2 && edge.Height > 2)
            using (var path = RoundedPath.Rect(edge, Math.Max(3, MiuTheme.Dp(_radius) - 1)))
            using (var pen = new Pen(border, 1))
                g.DrawPath(pen, path);
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap, Trimming = StringTrimming.EllipsisCharacter };
        using var tb = new SolidBrush(Enabled ? fore : MiuTheme.Colors.DisabledOnSurface);
        g.DrawString(Text, Font, tb, rect, sf);

        // base.OnPaint is skipped, so the native focus frame never appears — draw our own.
        if (Focused && ShowFocusCues)
            ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(rect, -MiuTheme.Dp(4), -MiuTheme.Dp(4)));
    }
}
