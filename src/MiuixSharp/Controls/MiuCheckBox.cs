using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Checkbox tri-state, matching Compose ToggleableState.</summary>
public enum MiuToggleState { Off, On, Indeterminate }

/// <summary>A Miuix-style rounded-square checkbox with check mark / dash (tri-state).</summary>
public class MiuCheckBox : Control
{
    private bool _hover;
    private MiuToggleState _state = MiuToggleState.Off;

    public MiuCheckBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Size = new Size(MiuTheme.Dp(20), MiuTheme.Dp(20));
        Cursor = Cursors.Hand;
        MiuTheme.Changed += (_, __) => Invalidate();
    }

    public bool Checked
    {
        get => _state == MiuToggleState.On;
        set => State = value ? MiuToggleState.On : MiuToggleState.Off;
    }

    public MiuToggleState State
    {
        get => _state;
        set { if (_state == value) return; _state = value; Invalidate(); CheckedChanged?.Invoke(this, EventArgs.Empty); }
    }
    public event EventHandler CheckedChanged;

    protected override void OnClick(EventArgs e)
    { State = State == MiuToggleState.On ? MiuToggleState.Off : MiuToggleState.On; base.OnClick(e); }
    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int d = Math.Min(Width, Height) - 1;
        var box = new Rectangle(0, 0, d, d);

        bool on = _state != MiuToggleState.Off; // indeterminate shares checked colors
        Color fill, border;
        if (!Enabled) { fill = MiuTheme.Colors.DisabledSecondary; border = MiuTheme.Colors.DisabledOnSecondary; }
        else if (on) { fill = MiuTheme.Primary; border = MiuTheme.Primary; }
        else { fill = _hover ? MiuTheme.SurfaceAlt : Color.Transparent; border = MiuTheme.Colors.OnSurfaceVariantSummary; }

        using (var p = RoundedPath.Rect(box, MiuTheme.Dp(5)))
        {
            using (var b = new SolidBrush(fill)) g.FillPath(b, p);
            using (var pen = new Pen(border, 2)) g.DrawPath(pen, p);
        }

        if (on)
        {
            using var pen = new Pen(Enabled ? MiuTheme.OnPrimary : MiuTheme.Colors.DisabledOnPrimary, 2.5f);
            if (_state == MiuToggleState.Indeterminate)
            {
                // horizontal dash
                g.DrawLine(pen, d * 0.25f, d * 0.5f, d * 0.75f, d * 0.5f);
            }
            else
            {
                var pts = new[]
                {
                    new PointF(d * 0.22f, d * 0.52f),
                    new PointF(d * 0.43f, d * 0.72f),
                    new PointF(d * 0.80f, d * 0.28f),
                };
                g.DrawLines(pen, pts);
            }
        }
    }
}
