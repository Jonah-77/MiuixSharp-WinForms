using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>A Miuix-style toggle switch, self-drawn with a sliding thumb.</summary>
public class MiuSwitch : Control
{
    private bool _on;
    private float _anim; // 0..1 thumb position

    public MiuSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Width = MiuTheme.Dp(46); Height = MiuTheme.Dp(26);
        Cursor = Cursors.Hand;
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    public bool Checked
    {
        get => _on;
        set
        {
            // Re-assigning the same value must not raise CheckedChanged (two-way bindings
            // set Checked from their own handler and would otherwise loop).
            if (_on == value) return;
            _on = value;
            _anim = value ? 1 : 0;
            Invalidate();
            OnCheckedChanged(EventArgs.Empty);
        }
    }

    public event EventHandler CheckedChanged;
    protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

    protected override void OnClick(EventArgs e)
    { Checked = !Checked; base.OnClick(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int h = Height;
        var track = new Rectangle(0, 0, Width - 1, h - 1);
        Color trackColor = _on ? MiuTheme.Primary : RoundedPath.Blend(MiuTheme.SurfaceAlt, MiuTheme.TextSecondary, 0.25);
        using (var p = RoundedPath.Rect(track, h / 2))
        using (var b = new SolidBrush(trackColor)) g.FillPath(b, p);

        int thumbD = h - 6;
        float x = 3 + _anim * (Width - thumbD - 6);
        var thumb = new RectangleF(x, 3, thumbD, thumbD);
        using var tb = new SolidBrush(_on ? MiuTheme.OnPrimary : Color.White);
        g.FillEllipse(tb, thumb);
    }
}
