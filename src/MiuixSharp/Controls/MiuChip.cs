using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Semantic tone for a small status chip.</summary>
public enum MiuChipTone { Neutral, Primary, Success, Warning, Muted }

/// <summary>A small rounded status chip (pill label). Auto-sizes to its text.</summary>
public class MiuChip : Control
{
    private MiuChipTone _tone = MiuChipTone.Neutral;

    public MiuChip()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Font = MiuTheme.FontSmall;
        ForeColor = MiuTheme.Text;
        Height = 26;
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    public MiuChipTone Tone { get => _tone; set { _tone = value; Invalidate(); } }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            using var g = CreateGraphics();
            var size = g.MeasureString(value ?? "", Font);
            Width = Math.Max(44, (int)Math.Ceiling(size.Width) + 22);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        Color back, fore;
        switch (_tone)
        {
            case MiuChipTone.Primary: back = MiuTheme.PrimaryContainer; fore = MiuTheme.OnPrimaryContainer; break;
            case MiuChipTone.Success: back = Color.FromArgb(0xE3, 0xF4, 0xEA); fore = MiuTheme.Success; break;
            case MiuChipTone.Warning: back = MiuTheme.WarningContainer; fore = MiuTheme.Warning; break;
            case MiuChipTone.Muted: back = MiuTheme.SurfaceAlt; fore = MiuTheme.TextSecondary; break;
            default: back = MiuTheme.SurfaceAlt; fore = MiuTheme.Text; break;
        }

        using (var p = RoundedPath.Rect(rect, rect.Height / 2))
        using (var b = new SolidBrush(back)) g.FillPath(b, p);
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap };
        using var tb = new SolidBrush(fore);
        g.DrawString(Text, Font, tb, rect, sf);
    }
}
