using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>A Miuix-style radio button with a filled inner dot when selected.</summary>
public class MiuRadioButton : Control
{
    private bool _checked, _hover;

    public MiuRadioButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Size = new Size(MiuTheme.Dp(20), MiuTheme.Dp(20));
        Cursor = Cursors.Hand;
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value; Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public event EventHandler CheckedChanged;

    protected override void OnClick(EventArgs e) { Checked = true; base.OnClick(e); }
    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var box = new Rectangle(1, 1, Math.Min(Width, Height) - 2, Math.Min(Width, Height) - 2);
        Color border = _checked ? MiuTheme.Primary : (_hover ? MiuTheme.Text : MiuTheme.Colors.OnSurfaceVariantSummary);
        using (var pen = new Pen(border, 2)) g.DrawEllipse(pen, box);

        if (_checked)
        {
            int dot = Math.Max(6, box.Width / 2);
            using var b = new SolidBrush(MiuTheme.Primary);
            g.FillEllipse(b, box.X + (box.Width - dot) / 2, box.Y + (box.Height - dot) / 2, dot, dot);
        }
    }
}
