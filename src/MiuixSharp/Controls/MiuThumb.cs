using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// Rounded image container. The picture is clipped into the rounded shape instead of
/// being painted over a square grey box, which is how native <c>PictureBox</c> behaves.
/// Used for sample thumbnails and for the photo slot inside an edit form.
/// </summary>
public class MiuThumb : Control
{
    private Image _image;
    private string _placeholder = string.Empty;
    private bool _dropActive;
    private int _radius = 10;

    public MiuThumb()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        UpdateRegion();
    }

    /// <summary>Corner radius in pixels.</summary>
    public int Radius
    {
        get => _radius;
        set { _radius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
    }

    /// <summary>Text shown in the middle when no image is set (e.g. the sample number).</summary>
    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value ?? string.Empty; Invalidate(); }
    }

    /// <summary>The picture shown inside the rounded frame. Setting null falls back to the placeholder.</summary>
    public Image Image
    {
        get => _image;
        set { _image = value; Invalidate(); }
    }

    /// <summary>Highlight state used while a file is dragged over the control.</summary>
    public bool DropActive
    {
        get => _dropActive;
        set { _dropActive = value; Invalidate(); }
    }

    /// <summary>Fill behind the picture (Miuix surfaceContainerHigh).</summary>
    public Color SurfaceColor { get; set; } = MiuTheme.Colors.SurfaceContainerHigh;

    /// <summary>Font used for <see cref="Placeholder"/>. Lets callers draw a large "+" in an empty slot.</summary>
    public Font PlaceholderFont { get; set; } = MiuTheme.Font;

    /// <summary>Colour used for <see cref="Placeholder"/>.</summary>
    public Color PlaceholderColor { get; set; } = MiuTheme.TextSecondary;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width < 2 || Height < 2) return;
        using var path = RoundedPath.Rect(new Rectangle(0, 0, Width, Height), MiuTheme.Dp(_radius));
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        if (r.Width < 4 || r.Height < 4) return;

        using (var path = RoundedPath.Rect(r, MiuTheme.Dp(_radius)))
        {
            using (var b = new SolidBrush(SurfaceColor)) g.FillPath(b, path);

            if (_image != null)
            {
                // The picture is clipped to the rounded shape before it is drawn, so the
                // photo never breaks out of the frame.
                var state = g.Save();
                g.SetClip(path);
                float scale = Math.Min((float)Width / _image.Width, (float)Height / _image.Height);
                int w = Math.Max(1, (int)(_image.Width * scale));
                int h = Math.Max(1, (int)(_image.Height * scale));
                g.DrawImage(_image, new Rectangle((Width - w) / 2, (Height - h) / 2, w, h));
                g.Restore(state);
            }
            else if (!string.IsNullOrEmpty(_placeholder))
            {
                TextRenderer.DrawText(g, _placeholder, PlaceholderFont ?? MiuTheme.Font, r, PlaceholderColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            if (_dropActive)
            {
                using var pen = new Pen(MiuTheme.Primary, 2f);
                var inner = new Rectangle(1, 1, Width - 3, Height - 3);
                g.DrawPath(pen, RoundedPath.Rect(inner, Math.Max(0, MiuTheme.Dp(_radius) - 1)));
            }
        }
    }
}
