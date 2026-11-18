using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A Miuix-style slider with an active track and optional key points,
/// using the official sliderBackground / sliderKeyPoint color roles.
/// </summary>
public class MiuSlider : Control
{
    private int _value, _min = 0, _max = 100;
    private bool _dragging;
    private readonly List<int> _keyPoints = new();

    public MiuSlider()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        // sliderBackground is a translucent role (#0F000000 / #26FFFFFF): it has to be composited
        // over the parent surface, not over an opaque control background.
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        AccessibleRole = AccessibleRole.Slider;
        ApplyMetrics();
        MiuThemeHook.Attach(this, () => { ApplyMetrics(); Invalidate(); });
    }

    private int _autoHeight = -1;

    /// <summary>Re-derives dp metrics in device pixels (DPI change / app-level zoom).</summary>
    private void ApplyMetrics()
    {
        int h = MiuTheme.Dp(28); // SliderDefaults.MinHeight = 28dp
        if (_autoHeight < 0 || Height == _autoHeight) Height = h;
        _autoHeight = h;
    }

    /// <summary>Step for the Left/Right (Up/Down) keys.</summary>
    public int SmallChange { get; set; } = 1;

    /// <summary>Step for PageUp / PageDown.</summary>
    public int LargeChange { get; set; } = 10;

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down
        or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        int next = _value;
        switch (e.KeyCode)
        {
            case Keys.Left: case Keys.Down: next = _value - SmallChange; break;
            case Keys.Right: case Keys.Up: next = _value + SmallChange; break;
            case Keys.PageDown: next = _value - LargeChange; break;
            case Keys.PageUp: next = _value + LargeChange; break;
            case Keys.Home: next = Lo; break;
            case Keys.End: next = Hi; break;
            default: base.OnKeyDown(e); return;
        }
        e.Handled = true;
        e.SuppressKeyPress = true;
        Value = next;
        ValueChangedFinished?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override AccessibleObject CreateAccessibilityInstance() => new MiuSliderAccessibleObject(this);

    private sealed class MiuSliderAccessibleObject : ControlAccessibleObject
    {
        private readonly MiuSlider _owner;
        public MiuSliderAccessibleObject(MiuSlider owner) : base(owner) => _owner = owner;
        public override AccessibleRole Role => AccessibleRole.Slider;
        // Windows exposes the numeric value through the Value property (accValue).
        public override string Value
        {
            get => _owner.Value.ToString();
            set { if (int.TryParse(value, out var v)) _owner.Value = v; }
        }
    }

    // Minimum/Maximum can be set in any order (Minimum = 50 then Maximum = 40, or a caller
    // passing Min > Max). Math.Clamp throws when min > max, so clamp against the sorted pair.
    private int Lo => Math.Min(_min, _max);
    private int Hi => Math.Max(_min, _max);

    public int Value
    {
        get => _value;
        set
        {
            int clamped = Math.Clamp(value, Lo, Hi);
            if (clamped == _value) { Invalidate(); return; }
            _value = clamped;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public int Minimum { get => _min; set { _min = value; _value = Math.Clamp(_value, Lo, Hi); Invalidate(); } }
    public int Maximum { get => _max; set { _max = value; _value = Math.Clamp(_value, Lo, Hi); Invalidate(); } }
    public List<int> KeyPoints => _keyPoints;
    /// <summary>Whether to show key-point indicators (Miuix showKeyPoints, default false).</summary>
    public bool ShowKeyPoints { get; set; } = true;
    /// <summary>Magnetic snap threshold as a fraction (Miuix default 0.02).</summary>
    public float MagnetThreshold { get; set; } = 0.02f;
    public event EventHandler ValueChanged;
    public event EventHandler ValueChangedFinished;

    private int ValueFromX(int x)
    {
        int w = Width - 20;
        if (w <= 0) return Lo;
        double ratio = Math.Clamp((x - 10) / (double)w, 0, 1);
        int raw = Lo + (int)Math.Round(ratio * (Hi - Lo));
        // Magnetic snap to key points (Miuix magnetThreshold).
        if (_keyPoints.Count > 0 && MagnetThreshold > 0 && (Hi - Lo) > 0)
        {
            double span = (Hi - Lo) * MagnetThreshold;
            var nearest = _keyPoints.OrderBy(k => Math.Abs(k - raw)).First();
            if (Math.Abs(nearest - raw) <= span) return nearest;
        }
        return raw;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    { if (e.Button == MouseButtons.Left) { _dragging = true; Value = ValueFromX(e.X); } base.OnMouseDown(e); }
    protected override void OnMouseMove(MouseEventArgs e)
    { if (_dragging) Value = ValueFromX(e.X); base.OnMouseMove(e); }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_dragging) { _dragging = false; ValueChangedFinished?.Invoke(this, EventArgs.Empty); }
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int cy = Height / 2;
        int x0 = MiuTheme.Dp(10), x1 = Width - MiuTheme.Dp(10), w = x1 - x0;
        double ratio = (Hi - Lo) == 0 ? 0 : (_value - Lo) / (double)(Hi - Lo);
        int thumbX = x0 + (int)Math.Round(ratio * w);

        // inactive track
        g.DrawLine(MiuResources.RoundPen(MiuTheme.Colors.SliderBackground, MiuTheme.Dpf(8)), x0, cy, x1, cy);
        // active track
        g.DrawLine(MiuResources.RoundPen(MiuTheme.Primary, MiuTheme.Dpf(8)), x0, cy, thumbX, cy);

        // key points — KeyPointRadius = 3.855dp (official)
        if (ShowKeyPoints)
        {
            float kpr = MiuTheme.Dpf(3.855f);
            foreach (var kp in _keyPoints)
            {
                double r = (Hi - Lo) == 0 ? 0 : (kp - Lo) / (double)(Hi - Lo);
                int kx = x0 + (int)Math.Round(r * w);
                g.FillEllipse(MiuResources.Brush(MiuTheme.Colors.SliderKeyPointForeground), kx - kpr, cy - kpr, kpr * 2, kpr * 2);
            }
        }

        // thumb
        int tr = MiuTheme.Dp(10);
        g.FillEllipse(MiuResources.Brush(MiuTheme.Primary), thumbX - tr, cy - tr, tr * 2, tr * 2);
        g.DrawEllipse(MiuResources.Pen(MiuTheme.OnPrimary, MiuTheme.Dpf(2)), thumbX - tr, cy - tr, tr * 2, tr * 2);

        if (Focused && ShowFocusCues)
            ControlPaint.DrawFocusRectangle(g, new Rectangle(x0, 0, Math.Max(1, x1 - x0), Height - 1));
    }
}
