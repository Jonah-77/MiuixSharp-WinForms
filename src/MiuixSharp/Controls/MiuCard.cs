using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A rounded card surface with a soft, restrained drop shadow and optional
/// accent bar. The signature "floating card" of the Miuix design language.
/// </summary>
public class MiuCard : UserControl
{
    private int _radius = MiuTheme.RadiusLg; // 16dp — CardDefaults.CornerRadius
    private int _shadow = 0;                 // Miuix cards use surface layering, not drop shadow
    private bool _accent;
    private int _pad = -1; // room for the shadow around content, in device px (10dp)

    public MiuCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ApplyMetrics();
        MiuThemeHook.Attach(this, () => { ApplyMetrics(); Invalidate(); });
    }

    /// <summary>Re-derives dp metrics in device pixels (DPI change / app-level zoom).</summary>
    private void ApplyMetrics()
    {
        int pad = MiuTheme.Dp(10);
        // A caller-provided padding wins; only the auto padding follows the scale.
        if (_pad < 0 || Padding.All == _pad) Padding = new Padding(pad);
        _pad = pad;
    }

    /// <summary>Corner radius in pixels.</summary>
    public int Radius { get => _radius; set { _radius = value; Invalidate(); } }

    /// <summary>Shadow softness/spread in pixels. 0 disables the shadow.</summary>
    public int Shadow { get => _shadow; set { _shadow = value; Invalidate(); } }

    /// <summary>Show a thin primary-colour accent bar on the left edge.</summary>
    public bool Accent { get => _accent; set { _accent = value; Invalidate(); } }

    /// <summary>The inner rectangle where child controls should live.</summary>
    public Rectangle ContentBounds => new(_pad, _pad, Width - _pad * 2, Height - _pad * 2);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var cardRect = new Rectangle(_pad, _pad, Width - _pad * 2 - 1, Height - _pad * 2 - 1);
        if (cardRect.Width <= 0 || cardRect.Height <= 0) return;

        // Soft shadow: a few offset translucent rounded fills beneath the card.
        // A shadow is a light-theme affordance: in dark mode the card floats by surface layering.
        // Resolve System mode through IsDark instead of comparing Mode == Light.
        if (_shadow > 0 && !MiuTheme.IsDark)
        {
            for (int i = _shadow; i >= 1; i--)
            {
                int alpha = (int)(10 * (1 - (double)i / _shadow));
                if (alpha <= 0) continue;
                var sr = new Rectangle(cardRect.X, cardRect.Y + i, cardRect.Width, cardRect.Height);
                using var sp = RoundedPath.Rect(sr, MiuTheme.Dp(_radius));
                using var sb = new SolidBrush(Color.FromArgb(alpha, 0x1B, 0x22, 0x2D));
                g.FillPath(sb, sp);
            }
        }

        using (var path = RoundedPath.Rect(cardRect, MiuTheme.Dp(_radius)))
        {
            using var br = new SolidBrush(MiuTheme.Surface);
            g.FillPath(br, path);
            using var pen = new Pen(MiuTheme.Outline, 1);
            g.DrawPath(pen, path);
        }

        if (_accent)
        {
            var bar = new Rectangle(cardRect.X, cardRect.Y + _radius, 4, cardRect.Height - _radius * 2);
            using var p = RoundedPath.Rect(bar, MiuTheme.Dp(2));
            using var b = new SolidBrush(MiuTheme.Primary);
            g.FillPath(b, p);
        }
    }

    protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); Invalidate(); }
}
