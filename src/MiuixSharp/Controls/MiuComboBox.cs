using System.Drawing;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A Miuix-style dropdown. Miuix has no native WinForms equivalent: a ComboBox paints its
/// own square frame and system arrow, so this control draws the rounded shell itself and
/// hosts an owner-drawn popup list instead.
/// </summary>
public class MiuComboBox : MiuField
{
    private readonly List<string> _items = new();
    private int _selectedIndex = -1;
    private string _placeholder = "请选择";
    private MiuPopup _popup;
    private MiuListPanel _list;

    public MiuComboBox()
    {
        Cursor = Cursors.Hand;
    }

    public MiuComboBox(string[] items, string selected = null) : this()
    {
        if (items != null) _items.AddRange(items);
        SelectedItem = selected;
    }

    /// <summary>Backing item collection; mutating it externally is allowed but requires a repaint.</summary>
    public List<string> Items => _items;

    public void SetItems(IEnumerable<string> items)
    {
        _items.Clear();
        if (items != null) _items.AddRange(items);
        if (_selectedIndex >= _items.Count) _selectedIndex = -1;
        Invalidate();
    }

    /// <summary>Placeholder shown when nothing is selected.</summary>
    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value ?? string.Empty; Invalidate(); }
    }

    public string SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
        set => SelectedIndex = value == null ? -1 : _items.IndexOf(value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            int v = _items.Count == 0 ? -1 : Math.Clamp(value, -1, _items.Count - 1);
            if (_selectedIndex == v) return;
            _selectedIndex = v;
            Invalidate();
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler SelectedIndexChanged;

    private void ShowList()
    {
        if (_items.Count == 0) return;

        if (_popup == null)
        {
            _list = new MiuListPanel { ItemHeight = 32 };
            _list.Picked += idx => { SelectedIndex = idx; CloseList(); };
            _popup = new MiuPopup(_list);
        }

        _list.Width = Math.Max(Width, 180);
        _list.SetItems(_items);
        _list.SelectedIndex = _selectedIndex;
        _popup.Fit(new Size(_list.Width, _list.ContentHeight), 320);
        if (!_popup.Visible) _popup.ShowNear(this);
    }

    /// <summary>Open the item list programmatically (same as clicking the field).</summary>
    public void OpenList() => ShowList();

    private void CloseList()
    {
        if (_popup != null && _popup.Visible) _popup.Close();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left) return;
        if (_popup != null && _popup.Visible) CloseList();
        else ShowList();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        // Wheel cycles the selection so a long list does not need to be opened first.
        if (_items.Count > 0 && Focused)
        {
            int next = Math.Clamp(_selectedIndex + (e.Delta > 0 ? -1 : 1), 0, _items.Count - 1);
            SelectedIndex = next;
        }
        base.OnMouseWheel(e);
    }

    protected override bool IsInputKey(Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Down:
            case Keys.Up:
            case Keys.Space:
            case Keys.Enter:
                return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Down:
            case Keys.Space:
                if (e.Alt || e.KeyCode == Keys.Space) { ShowList(); e.Handled = true; }
                else if (_items.Count > 0) { SelectedIndex = Math.Min(_selectedIndex + 1, _items.Count - 1); e.Handled = true; }
                break;
            case Keys.Up:
                if (_items.Count > 0) { SelectedIndex = Math.Max(_selectedIndex - 1, 0); e.Handled = true; }
                break;
            case Keys.Enter:
                ShowList();
                e.Handled = true;
                break;
            case Keys.Escape:
                CloseList();
                break;
        }
    }

    protected override void PaintContent(Graphics g, Rectangle shell)
    {
        var text = SelectedItem;
        var rect = new Rectangle(InsideMargin, 0, Math.Max(4, Width - InsideMargin - 30), Height);
        if (string.IsNullOrEmpty(text))
            DrawText(g, _placeholder, MiuTheme.Font, MiuTheme.Colors.OnSecondaryContainer, rect);
        else
            DrawText(g, text, MiuTheme.Font, ValueColor, rect);

        bool open = _popup != null && _popup.Visible;
        MiuGlyph.Chevron(g, Width - 17, Height / 2f, 9f,
            open || IsFocused ? MiuTheme.Primary : MutedColor, up: open);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _popup?.Dispose();
            _list?.Dispose();
        }
        base.Dispose(disposing);
    }
}
