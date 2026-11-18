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
        AccessibleRole = AccessibleRole.CheckButton;
        ApplyMetrics();
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        Cursor = Cursors.Hand;
        MiuThemeHook.Attach(this, () => { ApplyMetrics(); Invalidate(); });
    }

    private int _autoW = -1, _autoH = -1;

    /// <summary>Re-derives dp metrics in device pixels (DPI change / app-level zoom).</summary>
    private void ApplyMetrics()
    {
        int w = MiuTheme.Dp(46), h = MiuTheme.Dp(26);
        if (_autoW < 0 || (Width == _autoW && Height == _autoH)) Size = new Size(w, h);
        _autoW = w; _autoH = h;
    }

    /// <summary>Flips the switch exactly as a click would (keyboard / automation).</summary>
    public void PerformToggle()
    {
        Checked = !Checked;
        OnClick(EventArgs.Empty);
    }

    protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            PerformToggle();
            return;
        }
        base.OnKeyDown(e);
    }

    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override AccessibleObject CreateAccessibilityInstance() => new MiuSwitchAccessibleObject(this);

    private sealed class MiuSwitchAccessibleObject : ControlAccessibleObject
    {
        private readonly MiuSwitch _owner;
        public MiuSwitchAccessibleObject(MiuSwitch owner) : base(owner) => _owner = owner;
        public override AccessibleRole Role => AccessibleRole.CheckButton;
        public override string DefaultAction => "切换";
        public override void DoDefaultAction() => _owner.PerformToggle();
        public override AccessibleStates State =>
            base.State | (_owner.Checked ? AccessibleStates.Checked : AccessibleStates.None);
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
            g.FillPath(MiuResources.Brush(trackColor), p);

        int thumbD = h - 6;
        float x = 3 + _anim * (Width - thumbD - 6);
        var thumb = new RectangleF(x, 3, thumbD, thumbD);
        g.FillEllipse(MiuResources.Brush(_on ? MiuTheme.OnPrimary : Color.White), thumb);

        if (Focused && ShowFocusCues)
            ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(track, MiuTheme.Dp(2), MiuTheme.Dp(2)));
    }
}
