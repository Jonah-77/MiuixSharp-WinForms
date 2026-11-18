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
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        AccessibleRole = AccessibleRole.CheckButton;
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    /// <summary>Flips the state exactly as a click would (keyboard / automation).</summary>
    public void PerformToggle()
    {
        State = State == MiuToggleState.On ? MiuToggleState.Off : MiuToggleState.On;
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

    protected override AccessibleObject CreateAccessibilityInstance() => new MiuCheckAccessibleObject(this);

    private sealed class MiuCheckAccessibleObject : ControlAccessibleObject
    {
        private readonly MiuCheckBox _owner;
        public MiuCheckAccessibleObject(MiuCheckBox owner) : base(owner) => _owner = owner;
        public override AccessibleRole Role => AccessibleRole.CheckButton;
        public override string DefaultAction => "切换";
        public override void DoDefaultAction() => _owner.PerformToggle();
        public override AccessibleStates State
        {
            get
            {
                var s = base.State;
                if (_owner.State == MiuToggleState.On) s |= AccessibleStates.Checked;
                if (_owner.State == MiuToggleState.Indeterminate) s |= AccessibleStates.Mixed;
                return s;
            }
        }
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

        if (Focused && ShowFocusCues)
            ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(box, MiuTheme.Dp(2), MiuTheme.Dp(2)));
    }
}
