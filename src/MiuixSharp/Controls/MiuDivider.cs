using System.Drawing;
using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Divider orientation, matching Miuix HorizontalDivider / VerticalDivider.</summary>
public enum MiuDividerOrientation { Horizontal, Vertical }

/// <summary>
/// A thin divider using Miuix defaults: 0.75dp thickness and dividerLine color.
/// </summary>
public class MiuDivider : Control
{
    private MiuDividerOrientation _orientation = MiuDividerOrientation.Horizontal;

    public MiuDivider()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    public MiuDividerOrientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
            if (value == MiuDividerOrientation.Horizontal) Height = 2;
            else Width = 2;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(MiuTheme.Colors.DividerLine, 0.75f);
        if (_orientation == MiuDividerOrientation.Horizontal)
            e.Graphics.DrawLine(pen, 0, Height / 2f, Width, Height / 2f);
        else
            e.Graphics.DrawLine(pen, Width / 2f, 0, Width / 2f, Height);
    }
}
