using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A rounded, self-drawn scroll host.
///
/// WinForms' <c>AutoScroll</c> produces the square system scrollbar with arrow buttons —
/// the one element that still looks like Windows 95 in an otherwise rounded layout. This
/// control scrolls its content with the mouse wheel and draws a thin rounded indicator
/// instead, in the same visual language as the rest of MiuixSharp.
///
/// Usage: put one child in it, call <see cref="SetContent"/>, and call
/// <see cref="RefreshLayout"/> after adding or removing rows.
/// </summary>
public class MiuScrollArea : Panel
{
    private const int GutterWidth = 12;   // dp
    private const int ThumbWidth = 6;     // dp

    private Control _content;
    private int _offset;
    private bool _dragging;
    private int _dragAnchorY;
    private int _dragAnchorOffset;

    public MiuScrollArea()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        AutoScroll = false;
        // The indicator is drawn from ThumbColor, which resolves the live theme role.
        MiuThemeHook.Attach(this, () => Invalidate());
    }

    /// <summary>The scrollable child. Only one is supported.</summary>
    public Control Content => _content;

    /// <summary>Colour of the thin scroll indicator. Follows the theme unless set explicitly.</summary>
    public Color ThumbColor
    {
        get => _thumbColor ?? MiuTheme.Colors.OnSurfaceVariantActions;
        set { _thumbColor = value; Invalidate(); }
    }
    private Color? _thumbColor;

    /// <summary>Attach the scrollable content.</summary>
    public void SetContent(Control content)
    {
        if (_content != null) Controls.Remove(_content);
        _content = content;
        if (content != null)
        {
            content.Dock = DockStyle.None;
            if (content is ScrollableControl sc) sc.AutoScroll = false;
            Controls.Add(content);
        }
        HookWheel(content);
        RefreshLayout();
    }

    /// <summary>Re-measure the content and update the indicator. Call after rows change.</summary>
    public void RefreshLayout()
    {
        if (_content == null) return;

        bool scrollable = ContentHeight() > ClientSize.Height;
        int gutter = scrollable ? GutterWidth : 0;

        _content.Width = Math.Max(60, ClientSize.Width - gutter);
        _content.PerformLayout();
        _content.Height = ContentHeight();

        int max = Math.Max(0, _content.Height - ClientSize.Height);
        if (_offset > max) _offset = max;
        _content.Top = -_offset;

        HookWheel(_content);
        Invalidate();
    }

    /// <summary>Total height the content needs.</summary>
    private int ContentHeight()
    {
        if (_content == null) return 0;
        int h = _content.Padding.Top;
        foreach (Control c in _content.Controls)
            h = Math.Max(h, c.Bottom);
        return h + _content.Padding.Bottom;
    }

    /// <summary>Scroll the content by a number of pixels (negative scrolls up).</summary>
    public void ScrollBy(int pixels)
    {
        if (_content == null) return;
        int max = Math.Max(0, _content.Height - ClientSize.Height);
        _offset = Math.Max(0, Math.Min(max, _offset + pixels));
        _content.Top = -_offset;
        Invalidate();
    }

    /// <summary>Scroll to the very bottom of the content.</summary>
    public void ScrollToBottom()
    {
        if (_content == null) return;
        ScrollBy(int.MaxValue / 4);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RefreshLayout();
    }

    /// <summary>
    /// The mouse wheel over a child control goes to that child, not to us. Attach a handler
    /// to every descendant so the whole area scrolls as one.
    /// </summary>
    private void HookWheel(Control root)
    {
        if (root == null) return;
        root.MouseWheel -= OnContentWheel;
        root.MouseWheel += OnContentWheel;
        foreach (Control c in root.Controls) HookWheel(c);
    }

    private void OnContentWheel(object sender, MouseEventArgs e)
    {
        if (_content == null) return;
        int max = Math.Max(0, _content.Height - ClientSize.Height);
        if (max <= 0) return;
        _offset = Math.Max(0, Math.Min(max, _offset - (e.Delta / 120) * 54));
        _content.Top = -_offset;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        var thumb = ThumbRect();
        if (!thumb.IsEmpty && thumb.Contains(e.Location))
        {
            _dragging = true;
            _dragAnchorY = e.Y;
            _dragAnchorOffset = _offset;
            Capture = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging)
        {
            int max = Math.Max(0, (_content?.Height ?? 0) - ClientSize.Height);
            int trackH = Math.Max(1, ClientSize.Height - 4);
            var thumb = ThumbRect();
            int thumbH = Math.Max(1, thumb.Height);
            int travel = Math.Max(1, trackH - thumbH);
            int delta = e.Y - _dragAnchorY;
            _offset = Math.Max(0, Math.Min(max, _dragAnchorOffset + (int)(delta * (double)max / travel)));
            if (_content != null) _content.Top = -_offset;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_dragging) { _dragging = false; Capture = false; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (_content == null) return;
        int max = Math.Max(0, _content.Height - ClientSize.Height);
        if (max <= 0) return;

        var thumb = ThumbRect();
        if (thumb.IsEmpty) return;

        // Thin rounded indicator, no arrow buttons.
        using (var path = RoundedPath.Rect(thumb, thumb.Width / 2))
        using (var b = new SolidBrush(ThumbColor))
            g.FillPath(b, path);
    }

    /// <summary>Geometry of the scroll indicator.</summary>
    private Rectangle ThumbRect()
    {
        if (_content == null) return Rectangle.Empty;
        int max = Math.Max(0, _content.Height - ClientSize.Height);
        if (max <= 0) return Rectangle.Empty;

        int trackTop = 2;
        int trackH = Math.Max(1, ClientSize.Height - 4);
        int thumbH = Math.Max(20, (int)(trackH * (double)ClientSize.Height / _content.Height));
        thumbH = Math.Min(thumbH, trackH);
        int thumbY = trackTop + (int)((trackH - thumbH) * (_offset / (double)max));
        return new Rectangle(ClientSize.Width - GutterWidth + (GutterWidth - ThumbWidth) / 2, thumbY, ThumbWidth, thumbH);
    }
}
