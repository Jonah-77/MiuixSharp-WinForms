using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Linear or circular progress indicator styled with Miuix primary colors.</summary>
public class MiuProgressIndicator : Control
{
    private bool _circular;
    private int _progress = -1; // -1 = indeterminate
    private readonly System.Windows.Forms.Timer _timer;
    private float _spin;

    public MiuProgressIndicator()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        // Only the track and the arc are painted — the rest is the parent surface.
        BackColor = Color.Transparent;
        Size = new Size(MiuTheme.Dp(40), MiuTheme.Dp(40));
        _timer = new System.Windows.Forms.Timer { Interval = 30 };
        _timer.Tick += (_, __) => { _spin = (_spin + 12) % 360; Invalidate(); };
        MiuThemeHook.Attach(this, () => Invalidate());
        Disposed += (_, __) => _timer.Dispose();
    }

    /// <summary>Circular spinner (true) or linear bar (false).</summary>
    public bool Circular
    {
        get => _circular;
        set { _circular = value; Size = value ? new Size(MiuTheme.Dp(40), MiuTheme.Dp(40)) : new Size(MiuTheme.Dp(200), MiuTheme.Dp(6)); if (_progress < 0) _timer.Start(); else _timer.Stop(); Invalidate(); }
    }

    /// <summary>0..100, or -1 for indeterminate animation.</summary>
    public int Progress
    {
        get => _progress;
        set { _progress = value < 0 ? -1 : Math.Clamp(value, 0, 100); if (_progress < 0) _timer.Start(); else _timer.Stop(); Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (_circular) DrawCircular(g); else DrawLinear(g);
    }

    private void DrawCircular(Graphics g)
    {
        int pad = MiuTheme.Dp(2);
        int d = Math.Min(Width, Height) - pad * 2;
        var box = new Rectangle(pad, pad, d, d);
        float w = MiuTheme.Dpf(4);
        using var track = new Pen(MiuTheme.Colors.SliderBackground, w);
        g.DrawEllipse(track, box);
        using var pen = new Pen(MiuTheme.Primary, w) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        if (_progress < 0)
            g.DrawArc(pen, box, _spin, 110);
        else
            g.DrawArc(pen, box, -90, 360f * _progress / 100);
    }

    private void DrawLinear(Graphics g)
    {
        int cy = Height / 2;
        using var bg = new Pen(MiuTheme.Colors.SliderBackground, Height) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(bg, 0, cy, Width, cy);
        using var fg = new Pen(MiuTheme.Primary, Height) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        if (_progress < 0)
        {
            int seg = Math.Max(20, Width / 3);
            int x = (int)((_spin / 360f) * (Width + seg)) - seg;
            g.DrawLine(fg, Math.Max(0, x), cy, Math.Min(Width, x + seg), cy);
        }
        else
            g.DrawLine(fg, 0, cy, Width * _progress / 100, cy);
    }
}
