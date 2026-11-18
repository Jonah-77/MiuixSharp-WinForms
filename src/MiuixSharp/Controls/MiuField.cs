using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// Shared shell for every single-line input: text field, dropdown, number stepper and
/// date field. Miuix renders all of them as the same 16dp-rounded tonal container with an
/// optional trailing affordance, while WinForms' native editors are square and border-drawn.
/// This class owns the surface (rounded fill, focus ring, hover) and each derived control
/// only supplies the editor plus its trailing glyph.
/// </summary>
public abstract class MiuField : Control
{
    private bool _focused;
    private bool _hover;
    private int _radius = 10;
    private int _insideMargin = 14;

    protected MiuField()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.Selectable, true);
        BackColor = Color.Transparent;
        Font = MiuTheme.Font;
        Height = MiuTheme.Dp(34);
        TabStop = true;
        Cursor = Cursors.Hand;
        MiuThemeHook.Attach(this, () => { Font = MiuTheme.Font; Invalidate(); });
    }

    /// <summary>Corner radius in pixels. Miuix uses 16dp on tall fields; compact desktop rows use 10.</summary>
    public int Radius
    {
        get => _radius;
        set { _radius = Math.Max(0, value); Invalidate(); }
    }

    /// <summary>Horizontal inner padding — mirrors TextFieldDefaults.InsideMargin.</summary>
    public int InsideMargin
    {
        get => _insideMargin;
        set { _insideMargin = Math.Max(0, value); PerformLayout(); Invalidate(); }
    }

    /// <summary>Tonal fill of the container (Miuix TextFieldDefaults.backgroundColor = secondaryContainer).</summary>
    protected virtual Color FillColor =>
        Enabled ? MiuTheme.Colors.SecondaryContainer : MiuTheme.Colors.DisabledSecondary;

    /// <summary>Text colour of the editor (Miuix labelColor = onSecondaryContainer is too low-contrast for data entry).</summary>
    protected Color ValueColor => Enabled ? MiuTheme.Text : MiuTheme.Colors.DisabledOnSurface;

    /// <summary>Muted colour used for placeholders and trailing glyphs.</summary>
    protected Color MutedColor => MiuTheme.TextSecondary;

    protected bool IsFocused => _focused;
    protected bool IsHovered => _hover;

    /// <summary>Called by hosted editors so the shell paints the focus ring for them.</summary>
    protected void SetFocusState(bool focused)
    {
        if (_focused == focused) return;
        _focused = focused;
        Invalidate();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        SetFocusState(true);
        base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e)
    {
        SetFocusState(false);
        base.OnLostFocus(e);
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && CanFocus && !Focused) Focus();
        base.OnMouseDown(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (Width < 4 || Height < 4) return;
        var shell = new Rectangle(0, 0, Width - 1, Height - 1);

        using (var path = RoundedPath.Rect(shell, MiuTheme.Dp(_radius)))
        using (var brush = new SolidBrush(FillColor))
            g.FillPath(brush, path);

        var innerRect = new Rectangle(1, 1, Width - 3, Height - 3);
        if (innerRect.Width > 2 && innerRect.Height > 2)
        {
            Color border = !Enabled ? MiuTheme.Colors.DisabledOnSurface
                         : _focused ? MiuTheme.Primary
                         : _hover ? RoundedPath.Blend(MiuTheme.Outline, MiuTheme.Primary, 0.35)
                         : MiuTheme.Outline;
            using var borderPath = RoundedPath.Rect(innerRect, Math.Max(0, MiuTheme.Dp(_radius) - 1));
            using var pen = new Pen(border, _focused ? 1.6f : 1f);
            g.DrawPath(pen, borderPath);
        }

        PaintContent(g, shell);
    }

    /// <summary>Draw the value text and trailing affordances on top of the surface.</summary>
    protected virtual void PaintContent(Graphics g, Rectangle shell) { }

    /// <summary>Convenience: draw one line of text clipped to <paramref name="bounds"/>, vertically centred.</summary>
    protected static void DrawText(Graphics g, string text, Font font, Color color, Rectangle bounds, bool right = false)
    {
        if (string.IsNullOrEmpty(text) || bounds.Width <= 2) return;
        var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis
                  | (right ? TextFormatFlags.Right : TextFormatFlags.Left)
                  | TextFormatFlags.NoPadding;
        TextRenderer.DrawText(g, text, font, bounds, color, flags);
    }
}
