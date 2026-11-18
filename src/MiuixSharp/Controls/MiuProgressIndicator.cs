using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Linear or circular progress indicator styled with Miuix primary colors.</summary>
[System.Drawing.ToolboxBitmap(typeof(MiuProgressIndicator), "MiuixSharp.Icons.MiuProgressIndicator.bmp")]
[System.ComponentModel.Description("进度指示器，线性或圆形，可确定或不确定")]
public class MiuProgressIndicator : Control
{
    private bool _circular;
    private int _progress = -1; // -1 = indeterminate
    private readonly System.Windows.Forms.Timer _timer;
    private float _spin;
    private int _autoW = -1, _autoH = -1;

    public MiuProgressIndicator()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        // Only the track and the arc are painted — the rest is the parent surface.
        BackColor = Color.Transparent;
        AccessibleRole = AccessibleRole.ProgressBar;
        ApplyMetrics();

        _timer = new System.Windows.Forms.Timer { Interval = 30 };
        _timer.Tick += (_, __) => { _spin = (_spin + 12) % 360; Invalidate(); };

        MiuThemeHook.Attach(this, () => { ApplyMetrics(); Invalidate(); });
        Disposed += (_, __) => _timer.Dispose();
        UpdateAnimation();
    }

    /// <summary>Re-derives dp metrics in device pixels (DPI change / app-level zoom).</summary>
    private void ApplyMetrics()
    {
        var def = _circular
            ? new Size(MiuTheme.Dp(40), MiuTheme.Dp(40))
            : new Size(MiuTheme.Dp(200), MiuTheme.Dp(6));
        // Only sizes that still look "auto" (they match the previous default) are refreshed,
        // so a caller who sized the control explicitly keeps that size.
        if (_autoW < 0 || (Width == _autoW && Height == _autoH)) Size = def;
        _autoW = def.Width; _autoH = def.Height;
    }

    /// <summary>Circular spinner (true) or linear bar (false).</summary>
    public bool Circular
    {
        get => _circular;
        set { _circular = value; ApplyMetrics(); UpdateAnimation(); Invalidate(); }
    }

    /// <summary>0..100, or -1 for indeterminate animation.</summary>
    public int Progress
    {
        get => _progress;
        set { _progress = value < 0 ? -1 : Math.Clamp(value, 0, 100); UpdateAnimation(); Invalidate(); }
    }

    // A spinner that keeps ticking while hidden (or inside the designer) burns a timer for
    // nothing — and the designer would animate every canvas one at once.
    private void UpdateAnimation()
    {
        bool run = _progress < 0 && !DesignMode && Visible && IsHandleCreated;
        if (run) { if (!_timer.Enabled) _timer.Start(); }
        else if (_timer.Enabled) _timer.Stop();
    }

    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); UpdateAnimation(); }
    protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); UpdateAnimation(); }

    protected override AccessibleObject CreateAccessibilityInstance() => new MiuProgressAccessibleObject(this);

    private sealed class MiuProgressAccessibleObject : ControlAccessibleObject
    {
        private readonly MiuProgressIndicator _owner;
        public MiuProgressAccessibleObject(MiuProgressIndicator owner) : base(owner) => _owner = owner;
        public override AccessibleRole Role => AccessibleRole.ProgressBar;
        public override string Value => _owner.Progress < 0 ? "" : _owner.Progress + "%";
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
        if (d <= 0) return;
        var box = new Rectangle(pad, pad, d, d);
        float w = MiuTheme.Dpf(4);
        g.DrawEllipse(MiuResources.Pen(MiuTheme.Colors.SliderBackground, w), box);
        var pen = MiuResources.RoundPen(MiuTheme.Primary, w);
        if (_progress < 0)
            g.DrawArc(pen, box, _spin, 110);
        else
            g.DrawArc(pen, box, -90, 360f * _progress / 100);
    }

    private void DrawLinear(Graphics g)
    {
        int cy = Height / 2;
        float w = Math.Max(1f, Height);
        g.DrawLine(MiuResources.RoundPen(MiuTheme.Colors.SliderBackground, w), 0, cy, Width, cy);
        var fg = MiuResources.RoundPen(MiuTheme.Primary, w);
        if (_progress < 0)
        {
            int seg = Math.Max(20, Width / 3);
            int x = (int)((_spin / 360f) * (Width + seg)) - seg;
            g.DrawLine(fg, Math.Max(0, x), cy, Math.Min(Width, x + seg), cy);
        }
        else
            g.DrawLine(fg, 0, cy, Math.Max(0, Width * _progress / 100), cy);
    }
}
