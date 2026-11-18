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
        Height = MiuTheme.Dp(28); // SliderDefaults.MinHeight = 28dp
        Cursor = Cursors.Hand;
        MiuTheme.Changed += (_, __) => Invalidate();
    }

    public int Value
    {
        get => _value;
        set { _value = Math.Clamp(value, _min, _max); Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty); }
    }
    public int Minimum { get => _min; set { _min = value; _value = Math.Clamp(_value, _min, _max); Invalidate(); } }
    public int Maximum { get => _max; set { _max = value; _value = Math.Clamp(_value, _min, _max); Invalidate(); } }
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
        if (w <= 0) return _min;
        double ratio = Math.Clamp((x - 10) / (double)w, 0, 1);
        int raw = _min + (int)Math.Round(ratio * (_max - _min));
        // Magnetic snap to key points (Miuix magnetThreshold).
        if (_keyPoints.Count > 0 && MagnetThreshold > 0 && (_max - _min) > 0)
        {
            double span = (_max - _min) * MagnetThreshold;
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
        double ratio = (_max - _min) == 0 ? 0 : (_value - _min) / (double)(_max - _min);
        int thumbX = x0 + (int)Math.Round(ratio * w);

        // inactive track
        using (var pen = new Pen(MiuTheme.Colors.SliderBackground, MiuTheme.Dpf(8)))
        { pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; g.DrawLine(pen, x0, cy, x1, cy); }
        // active track
        using (var pen = new Pen(MiuTheme.Primary, MiuTheme.Dpf(8)))
        { pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; g.DrawLine(pen, x0, cy, thumbX, cy); }

        // key points — KeyPointRadius = 3.855dp (official)
        if (ShowKeyPoints)
        {
            float kpr = MiuTheme.Dpf(3.855f);
            foreach (var kp in _keyPoints)
            {
                double r = (_max - _min) == 0 ? 0 : (kp - _min) / (double)(_max - _min);
                int kx = x0 + (int)Math.Round(r * w);
                using var b = new SolidBrush(MiuTheme.Colors.SliderKeyPointForeground);
                g.FillEllipse(b, kx - kpr, cy - kpr, kpr * 2, kpr * 2);
            }
        }

        // thumb
        int tr = MiuTheme.Dp(10);
        using (var b = new SolidBrush(MiuTheme.Primary)) g.FillEllipse(b, thumbX - tr, cy - tr, tr * 2, tr * 2);
        using (var pen = new Pen(MiuTheme.OnPrimary, MiuTheme.Dpf(2))) g.DrawEllipse(pen, thumbX - tr, cy - tr, tr * 2, tr * 2);
    }
}
