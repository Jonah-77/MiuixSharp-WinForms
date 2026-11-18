using System.Drawing;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A rounded text field. Wraps a borderless native TextBox inside the shared
/// <see cref="MiuField"/> shell so the surface, focus ring and padding match Miuix.
/// When <see cref="HistoryEnabled"/> is set it also grows a chevron that drops a
/// suggestion list; every suggestion can be removed from the list via its ✕ button
/// (deletion is a callback, so persistence stays the application's job).
/// </summary>
public class MiuInput : MiuField
{
    private readonly TextBox _box;
    private string _placeholder = string.Empty;
    private bool _multiline;
    private System.Windows.Forms.ScrollBars _scrollBars = System.Windows.Forms.ScrollBars.None;
    private MiuPopup _popup;
    private MiuListPanel _list;
    private List<string> _suggestions = new();
    private bool _suppress;

    public MiuInput()
    {
        Cursor = Cursors.IBeam;

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = MiuTheme.Font,
            ForeColor = MiuTheme.Text,
            BackColor = MiuTheme.Colors.SecondaryContainer,
            TabStop = false,
            AutoSize = true,
        };
        // 获得焦点时只亮边框，不自动弹建议列表：一进表单就盖住下面的字段很打扰。
        // 列表在「开始输入」或「点右侧 ▾」时出现。
        _box.GotFocus += (_, __) => SetFocusState(true);
        _box.LostFocus += (_, __) =>
        {
            SetFocusState(false);
            BeginInvoke(new Action(() =>
            {
                if (!_box.Focused) HideHistory();
            }));
        };
        _box.TextChanged += (_, __) =>
        {
            if (base.Text != _box.Text) base.Text = _box.Text;
            Invalidate();
            if (HistoryEnabled && !_suppress && _box.Focused) ShowHistory();
        };
        _box.KeyDown += BoxKeyDown;
        Controls.Add(_box);

        MiuThemeHook.Attach(this, () =>
        {
            _box.Font = MiuTheme.Font;
            _box.ForeColor = MiuTheme.Text;
            _box.BackColor = MiuTheme.Colors.SecondaryContainer;
            Invalidate();
        });
    }

    /// <summary>
    /// Hint shown while the field is empty. Drawn by the inner TextBox itself, because
    /// anything painted on the shell would be covered by the (opaque) edit control.
    /// </summary>
    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value ?? string.Empty;
            if (_box != null) _box.PlaceholderText = _placeholder;
            Invalidate();
        }
    }

    public bool ReadOnly
    {
        get => _box.ReadOnly;
        set { _box.ReadOnly = value; Invalidate(); }
    }

    public int MaxLength
    {
        get => _box.MaxLength;
        set => _box.MaxLength = value;
    }

    /// <summary>Multi-line mode; the editor then fills the shell with the given scrollbars.</summary>
    public bool Multiline
    {
        get => _multiline;
        set
        {
            if (_multiline == value) return;
            _multiline = value;
            _box.Multiline = value;
            if (!value) _box.ScrollBars = System.Windows.Forms.ScrollBars.None;
            else _box.ScrollBars = _scrollBars;
            PerformLayout();
            Invalidate();
        }
    }

    public System.Windows.Forms.ScrollBars ScrollBars
    {
        get => _scrollBars;
        set
        {
            _scrollBars = value;
            if (value != System.Windows.Forms.ScrollBars.None) Multiline = true;
            if (_multiline) _box.ScrollBars = value;
        }
    }

    /// <summary>Enable the suggestion dropdown for this field.</summary>
    public bool HistoryEnabled { get; set; }

    /// <summary>Maximum number of suggestions kept in the list.</summary>
    public int MaxSuggestions { get; set; } = 12;

    /// <summary>Supplies the suggestion list, most recent first. Only used when <see cref="HistoryEnabled"/>.</summary>
    public Func<List<string>> HistoryProvider { get; set; }

    /// <summary>Called when the user removes a suggestion. Return true to accept the removal.</summary>
    public Func<string, bool> OnDeleteHistory { get; set; }

    /// <summary>Optional filter: return false to hide a given value from the list.</summary>
    public Func<string, bool> AllowHistory { get; set; } = _ => true;

    public override string Text
    {
        get => _box?.Text ?? string.Empty;
        set
        {
            if (_box == null) return;
            var v = value ?? string.Empty;
            if (_box.Text == v) return;
            _box.Text = v;
        }
    }

    public int TextLength => _box?.TextLength ?? 0;

    public void SelectAll() => _box?.SelectAll();

    protected override void OnGotFocus(EventArgs e)
    {
        _box.Focus();
        base.OnGotFocus(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutBox();
    }

    private void LayoutBox()
    {
        if (_box == null) return;

        if (_multiline)
        {
            _box.Multiline = true;
            _box.AutoSize = false;
            int pad = Math.Max(6, InsideMargin - 4);
            _box.SetBounds(pad, 7, Math.Max(20, Width - pad * 2), Math.Max(18, Height - 14));
        }
        else
        {
            _box.Multiline = false;
            _box.AutoSize = true;
            _box.Width = Math.Max(20, Width - InsideMargin - (HistoryEnabled ? 30 : InsideMargin));
            _box.Left = InsideMargin;
            _box.Top = Math.Max(0, (Height - _box.Height) / 2);
        }
    }

    private void BoxKeyDown(object sender, KeyEventArgs e)
    {
        if (!HistoryEnabled || _popup == null || !_popup.Visible) return;
        if (e.KeyCode == Keys.Down && _list.Items.Count > 0)
        {
            _list.SelectedIndex = Math.Min(_list.SelectedIndex + 1, _list.Items.Count - 1);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Up && _list.Items.Count > 0)
        {
            _list.SelectedIndex = Math.Max(_list.SelectedIndex - 1, 0);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Enter && _list.SelectedIndex >= 0)
        {
            Pick(_list.SelectedIndex);
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            HideHistory();
            e.Handled = true;
        }
    }

    private void ShowHistory()
    {
        if (!HistoryEnabled || HistoryProvider == null) return;

        var query = (_box.Text ?? string.Empty).Trim();
        var filter = AllowHistory ?? (_ => true);
        _suggestions = (HistoryProvider() ?? new List<string>())
            .Where(filter)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => query.Length == 0 || x.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            .Distinct()
            .Take(MaxSuggestions)
            .ToList();

        if (_suggestions.Count == 0) { HideHistory(); return; }

        if (_popup == null)
        {
            _list = new MiuListPanel { ShowDelete = OnDeleteHistory != null };
            _list.Picked += Pick;
            _list.RequestDelete += idx =>
            {
                if (idx < 0 || idx >= _suggestions.Count) return;
                var value = _suggestions[idx];
                if (OnDeleteHistory != null && OnDeleteHistory(value)) ShowHistory();
            };
            _popup = new MiuPopup(_list);
        }

        _list.Width = Math.Max(Width, 200);
        _list.SetItems(_suggestions);
        _list.SelectedIndex = -1;
        _popup.Fit(new Size(_list.Width, _list.ContentHeight), 300);
        if (!_popup.Visible) _popup.ShowNear(this);
    }

    private void Pick(int index)
    {
        if (index < 0 || index >= _suggestions.Count) return;
        _suppress = true;
        Text = _suggestions[index];
        _suppress = false;
        HideHistory();
        _box.Focus();
        _box.SelectionStart = _box.TextLength;
    }

    /// <summary>Open the suggestion list programmatically (same as clicking the chevron).</summary>
    public void OpenSuggestions()
    {
        _box.Focus();
        ShowHistory();
    }

    /// <summary>Close the suggestion list if it is open.</summary>
    public void CloseSuggestions() => HideHistory();

    private void HideHistory()
    {
        if (_popup != null && _popup.Visible) _popup.Close();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left) return;
        if (HistoryEnabled && e.X >= Width - 30)
        {
            if (_popup != null && _popup.Visible) HideHistory();
            else { _box.Focus(); ShowHistory(); }
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        HideHistory();
        base.OnMouseWheel(e);
    }

    protected override void PaintContent(Graphics g, Rectangle shell)
    {
        if (HistoryEnabled)
        {
            // 建议列表的箭头只在「悬停 / 聚焦 / 已展开」时出现：常驻的 ▼ 会让整屏表单
            // 看起来像一堆下拉框，而它其实只是个「历史建议」的入口提示。
            bool open = _popup != null && _popup.Visible;
            if (!open && !IsHovered && !IsFocused) return;

            float cx = Width - 17;
            float cy = Height / 2f;
            MiuGlyph.Chevron(g, cx, cy, 9f, open || IsFocused ? MiuTheme.Primary : MutedColor, up: open);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _popup?.Dispose();
            _list?.Dispose();
            _box.Dispose();
        }
        base.Dispose(disposing);
    }
}
