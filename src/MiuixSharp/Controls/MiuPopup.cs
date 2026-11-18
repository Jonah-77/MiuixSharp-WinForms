using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>Strips the native border and background from a ToolStripDropDown so it can host a rounded card.</summary>
internal sealed class MiuPopupRenderer : ToolStripRenderer
{
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        => e.Graphics.Clear(MiuTheme.Surface);

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
}

/// <summary>
/// A borderless dropdown that hosts exactly one control. Used by the dropdown, date and
/// history popups; closing on outside click is inherited from ToolStripDropDown.
/// </summary>
internal sealed class MiuPopup : ToolStripDropDown
{
    private readonly ToolStripControlHost _host;

    public MiuPopup(Control content)
    {
        AutoSize = false;
        AutoClose = true;
        Padding = Padding.Empty;
        Margin = Padding.Empty;
        DropShadowEnabled = true;
        BackColor = MiuTheme.Surface;
        Renderer = new MiuPopupRenderer();
        _host = new ToolStripControlHost(content)
        {
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            AutoSize = false,
        };
        Items.Add(_host);
        content.Location = Point.Empty;
    }

    /// <summary>Size the dropdown to the content, clamping its height to <paramref name="maxHeight"/>.</summary>
    public void Fit(Size size, int maxHeight)
    {
        int h = Math.Min(size.Height, maxHeight);
        _host.Size = new Size(size.Width, h);
        Size = new Size(size.Width, h);
    }

    /// <summary>Show under (or above) the anchor control, aligned to its left edge.</summary>
    public void ShowNear(Control anchor, int gap = 2)
    {
        var area = Screen.FromControl(anchor).WorkingArea;
        var topLeft = anchor.PointToScreen(Point.Empty);
        int x = Math.Min(topLeft.X, area.Right - Width - 4);
        x = Math.Max(area.Left + 4, x);
        int y = topLeft.Y + anchor.Height + gap;
        if (y + Height > area.Bottom) y = Math.Max(area.Top + 4, topLeft.Y - Height - gap);
        Show(new Point(x, y));
    }
}

/// <summary>
/// A rounded card that hosts a list of rows drawn by <see cref="MiuListPanel"/> or the calendar.
/// Paints its own surface so the popup reads as a Miuix card rather than a WinForms menu.
/// </summary>
internal class MiuPopupCard : Control
{
    private int _radius = 12;

    public MiuPopupCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = MiuTheme.Surface;
    }

    public int Radius
    {
        get => _radius;
        set { _radius = Math.Max(0, value); Invalidate(); }
    }

    /// <summary>One line of text clipped to <paramref name="bounds"/>, vertically centred.</summary>
    protected static void DrawText(Graphics g, string text, Font font, Color color, Rectangle bounds)
    {
        if (string.IsNullOrEmpty(text) || bounds.Width <= 2) return;
        TextRenderer.DrawText(g, text, font, bounds, color,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        if (rect.Width < 4 || rect.Height < 4) return;
        using var path = RoundedPath.Rect(rect, MiuTheme.Dp(_radius));
        using (var b = new SolidBrush(MiuTheme.Surface)) g.FillPath(b, path);
        using (var pen = new Pen(MiuTheme.Outline, 1)) g.DrawPath(pen, path);
    }
}

/// <summary>
/// Owner-drawn single-column list used by the dropdown and history popups.
/// Rows get a rounded hover pill instead of the native list highlight, and an optional
/// trailing ✕ column deletes an entry without leaving the popup.
/// </summary>
internal sealed class MiuListPanel : MiuPopupCard
{
    private readonly List<string> _items = new();
    private int _hover = -1;
    private bool _hoverDelete;
    private int _selected = -1;
    private int _scroll;
    private static Font _bold;

    private static Font BoldFont => _bold ??= new Font(MiuTheme.Font, FontStyle.Bold);

    public int ItemHeight { get; set; } = 32;
    public int TopPad { get; set; } = 6;
    public bool ShowDelete { get; set; }

    public event Action<int> Picked;
    public event Action<int> RequestDelete;

    public IReadOnlyList<string> Items => _items;

    public int SelectedIndex
    {
        get => _selected;
        set { _selected = value; Invalidate(); }
    }

    public int ContentHeight => TopPad * 2 + _items.Count * ItemHeight;

    public void SetItems(IEnumerable<string> items)
    {
        _items.Clear();
        if (items != null) _items.AddRange(items);
        _scroll = 0;
        _hover = -1;
        Height = ContentHeight;
        Invalidate();
    }

    private int IndexAt(int y)
    {
        int local = y + _scroll - TopPad;
        if (local < 0) return -1;
        int idx = local / ItemHeight;
        return idx >= 0 && idx < _items.Count ? idx : -1;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int idx = IndexAt(e.Y);
        bool onDelete = ShowDelete && idx >= 0 && e.X >= Width - 34;
        if (idx != _hover || onDelete != _hoverDelete)
        {
            _hover = idx;
            _hoverDelete = onDelete;
            Invalidate();
        }
        Cursor = onDelete ? Cursors.Hand : Cursors.Default;
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        if (_hover != -1 || _hoverDelete) { _hover = -1; _hoverDelete = false; Invalidate(); }
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        int idx = IndexAt(e.Y);
        if (idx >= 0 && e.Button == MouseButtons.Left)
        {
            if (ShowDelete && e.X >= Width - 34) RequestDelete?.Invoke(idx);
            else Picked?.Invoke(idx);
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        int max = Math.Max(0, ContentHeight - Height);
        int next = Math.Clamp(_scroll - e.Delta / 3, 0, max);
        if (next != _scroll) { _scroll = next; Invalidate(); }
        base.OnMouseWheel(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        for (int i = 0; i < _items.Count; i++)
        {
            int y = TopPad + i * ItemHeight - _scroll;
            if (y + ItemHeight < 0 || y > Height) continue;
            var row = new Rectangle(6, y + 2, Width - 12, ItemHeight - 4);
            if (row.Width < 8) continue;

            bool selected = i == _selected;
            bool hot = i == _hover;
            if (selected || hot)
            {
                using var pill = RoundedPath.Rect(row, MiuTheme.Dp(8));
                using var b = new SolidBrush(selected ? MiuTheme.Colors.TertiaryContainer : MiuTheme.Colors.SurfaceContainerHigh);
                g.FillPath(b, pill);
            }

            int textRight = ShowDelete ? 34 : 8;
            var textRect = new Rectangle(row.X + 10, row.Y, row.Width - 10 - textRight, row.Height);
            TextRenderer.DrawText(g, _items[i], selected ? BoldFont : MiuTheme.Font, textRect,
                selected ? MiuTheme.Primary : MiuTheme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

            if (ShowDelete)
            {
                var delRect = new Rectangle(row.Right - 30, row.Y, 26, row.Height);
                if (_hoverDelete && i == _hover)
                    MiuGlyph.HoverDisc(g, delRect, MiuTheme.Colors.SurfaceContainerHighest);
                MiuGlyph.Cross(g, delRect.X + delRect.Width / 2f, delRect.Y + delRect.Height / 2f, 8f,
                    _hoverDelete && i == _hover ? MiuTheme.TextSecondary : MiuTheme.Colors.OnSurfaceVariantActions);
            }
        }
    }
}
