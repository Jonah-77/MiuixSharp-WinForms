using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Button visual style, mirroring Miuix / Material 3 button variants.</summary>
public enum MiuButtonStyle { Filled, Tonal, Outlined, Text, Warning }

/// <summary>A rounded, self-drawn button with hover / pressed feedback.</summary>
public class MiuButton : Control
{
    private MiuButtonStyle _style = MiuButtonStyle.Filled;
    private bool _hover, _pressed;
    private int _radius = MiuTheme.RadiusMd;

    public MiuButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Font = MiuTheme.Font;
        ForeColor = MiuTheme.Text;
        MiuThemeHook.Attach(this, () => { UpdateColors(); ApplyMetrics(); Invalidate(); });
        ApplyMetrics();
        UpdateColors();

    }

    private int _autoHeight = -1;

    /// <summary>Re-derives dp metrics in device pixels (DPI change / app-level zoom).</summary>
    private void ApplyMetrics()
    {
        int h = MiuTheme.Dp(MiuTheme.ButtonMinHeight); // 40dp per Miuix ButtonDefaults
        if (_autoHeight < 0 || Height == _autoHeight) Height = h;
        _autoHeight = h;
        Padding = new Padding(MiuTheme.Dp(16), 0, MiuTheme.Dp(16), 0);
    }

    public MiuButtonStyle Style
    {
        get => _style;
        set { _style = value; UpdateColors(); Invalidate(); }
    }

    public int Radius
    {
        get => _radius;
        set { _radius = value; Invalidate(); }
    }

    private Color _baseBack, _baseFore, _hoverBack, _pressBack, _border;

    private void UpdateColors()
    {
        switch (_style)
        {
            case MiuButtonStyle.Filled:
                _baseBack = MiuTheme.Primary; _baseFore = MiuTheme.OnPrimary;
                _hoverBack = RoundedPath.Blend(MiuTheme.Primary, Color.White, 0.12);
                _pressBack = RoundedPath.Blend(MiuTheme.Primary, Color.Black, 0.12);
                _border = Color.Transparent; break;
            case MiuButtonStyle.Tonal:
                _baseBack = MiuTheme.PrimaryContainer; _baseFore = MiuTheme.OnPrimaryContainer;
                _hoverBack = RoundedPath.Blend(MiuTheme.PrimaryContainer, MiuTheme.Primary, 0.10);
                _pressBack = RoundedPath.Blend(MiuTheme.PrimaryContainer, MiuTheme.Primary, 0.20);
                _border = Color.Transparent; break;
            case MiuButtonStyle.Outlined:
                _baseBack = MiuTheme.Surface; _baseFore = MiuTheme.Primary;
                _hoverBack = MiuTheme.PrimaryContainer; _pressBack = RoundedPath.Blend(MiuTheme.PrimaryContainer, MiuTheme.Primary, 0.15);
                _border = MiuTheme.Outline; break;
            case MiuButtonStyle.Text:
                _baseBack = Color.Transparent; _baseFore = MiuTheme.Primary;
                _hoverBack = MiuTheme.PrimaryContainer; _pressBack = RoundedPath.Blend(MiuTheme.PrimaryContainer, MiuTheme.Primary, 0.15);
                _border = Color.Transparent; break;
            case MiuButtonStyle.Warning:
                _baseBack = MiuTheme.Surface; _baseFore = MiuTheme.Warning;
                _hoverBack = MiuTheme.WarningContainer; _pressBack = RoundedPath.Blend(MiuTheme.WarningContainer, MiuTheme.Warning, 0.18);
                _border = MiuTheme.Warning; break;
        }
        ForeColor = _baseFore;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || keyData == Keys.Enter || base.IsInputKey(keyData);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        Color back = _pressed ? _pressBack : (_hover ? _hoverBack : _baseBack);
        using (var path = RoundedPath.Rect(rect, MiuTheme.Dp(_radius)))
        {
            if (back != Color.Transparent)
                using (var br = new SolidBrush(back)) g.FillPath(br, path);
            if (_border != Color.Transparent)
                using (var pen = new Pen(_border, MiuTheme.Dpf(1))) g.DrawPath(pen, path);
        }

        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        var textRect = Rectangle.Inflate(rect, -Padding.Left / 2, -2);
        using var tb = new SolidBrush(Enabled ? _baseFore : MiuTheme.TextSecondary);
        g.DrawString(Text, Font, tb, textRect, sf);
    }

    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
}
