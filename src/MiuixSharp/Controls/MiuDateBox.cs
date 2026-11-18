using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A date field plus a month-grid popup. Miuix has no date picker of its own, so this is
/// derived from its design language: the same rounded tonal shell as a text field, a
/// hairline calendar glyph, and a custom month grid (32px cells, primary-filled selection,
/// primary ring on today) instead of the native DateTimePicker drop-down.
/// </summary>
public class MiuDateBox : MiuField
{
    private DateTime _value = DateTime.Today;
    private bool _checked = true;
    private bool _hoverClear;
    private string _format = "yyyy-MM-dd";
    private MiuPopup _popup;
    private MiuCalendarPanel _calendar;

    public MiuDateBox()
    {
        Cursor = Cursors.Hand;
    }

    public MiuDateBox(string value, bool nullable) : this()
    {
        Nullable = nullable;
        ApplyText(value);
    }

    /// <summary>When true the field may be empty, shown as the placeholder and cleared with ✕.</summary>
    public bool Nullable { get; set; }

    public string Format
    {
        get => _format;
        set { _format = string.IsNullOrEmpty(value) ? "yyyy-MM-dd" : value; Invalidate(); }
    }

    public string Placeholder { get; set; } = "不限";

    public DateTime Value
    {
        get => _value;
        set
        {
            bool changed = !_checked || _value.Date != value.Date;
            _value = value.Date;
            _checked = true;
            Invalidate();
            if (changed) ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The value formatted for storage, or null when nothing is set.</summary>
    public string DateText => _checked ? _value.ToString(_format) : null;

    public event EventHandler ValueChanged;

    /// <summary>Set the field from a stored string ("yyyy-MM-dd"); empty means "not set" when nullable.</summary>
    public void ApplyText(string text)
    {
        if (!string.IsNullOrWhiteSpace(text) && DateTime.TryParse(text, out var parsed))
        {
            _value = parsed.Date;
            _checked = true;
        }
        else
        {
            _value = DateTime.Today;
            _checked = !Nullable;
        }
        Invalidate();
    }

    private bool CanClear => Nullable && _checked;
    private Rectangle ClearRect => new(Width - 54, 3, 24, Height - 6);

    private void ShowCalendar()
    {
        if (_calendar == null)
        {
            _calendar = new MiuCalendarPanel();
            _calendar.Picked += d =>
            {
                Value = d;
                CloseCalendar();
            };
            _calendar.Cleared += () =>
            {
                Checked = false;
                CloseCalendar();
            };
            _popup = new MiuPopup(_calendar);
        }

        _calendar.Reset(_checked ? _value : DateTime.Today, Nullable);
        _popup.Fit(new Size(_calendar.Width, _calendar.Height), 460);
        if (!_popup.Visible) _popup.ShowNear(this);
    }

    /// <summary>Open the month grid programmatically (same as clicking the field).</summary>
    public void OpenCalendar() => ShowCalendar();

    /// <summary>Close the month grid if it is open.</summary>
    public void ClosePopup() => CloseCalendar();

    private void CloseCalendar()
    {
        if (_popup != null && _popup.Visible) _popup.Close();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        bool hot = CanClear && ClearRect.Contains(e.Location);
        if (hot != _hoverClear)
        {
            _hoverClear = hot;
            Cursor = hot ? Cursors.Hand : Cursors.Hand;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        if (_hoverClear) { _hoverClear = false; Invalidate(); }
        base.OnMouseLeave(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left) return;
        if (CanClear && ClearRect.Contains(e.Location))
        {
            Checked = false;
            CloseCalendar();
            return;
        }
        if (_popup != null && _popup.Visible) CloseCalendar();
        else ShowCalendar();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (Focused)
        {
            _value = _value.AddDays(e.Delta > 0 ? 1 : -1);
            _checked = true;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        base.OnMouseWheel(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Down or Keys.Up || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Down:
            case Keys.Space:
            case Keys.Enter:
                ShowCalendar();
                e.Handled = true;
                break;
            case Keys.Delete:
                if (Nullable) Checked = false;
                e.Handled = true;
                break;
            case Keys.Left:
            case Keys.Right:
                _value = _value.AddDays(e.KeyCode == Keys.Left ? -1 : 1);
                _checked = true;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                break;
        }
    }

    protected override void PaintContent(Graphics g, Rectangle shell)
    {
        var text = _checked ? _value.ToString(_format) : null;
        var rect = new Rectangle(InsideMargin, 0, Math.Max(4, Width - InsideMargin - 54), Height);
        if (string.IsNullOrEmpty(text))
            DrawText(g, Placeholder, MiuTheme.Font, MiuTheme.Colors.OnSecondaryContainer, rect);
        else
            DrawText(g, text, MiuTheme.Font, ValueColor, rect);

        if (CanClear && _hoverClear)
        {
            var disc = new Rectangle(ClearRect.X + 2, ClearRect.Y + 2, ClearRect.Width - 4, ClearRect.Height - 4);
            MiuGlyph.HoverDisc(g, disc, MiuTheme.Colors.SurfaceContainerHigh);
            MiuGlyph.Cross(g, disc.X + disc.Width / 2f, disc.Y + disc.Height / 2f, 8f, MiuTheme.TextSecondary);
        }

        bool open = _popup != null && _popup.Visible;
        MiuGlyph.Calendar(g, Width - 18, Height / 2f, 14f,
            open || IsFocused ? MiuTheme.Primary : MutedColor, open ? 1.8f : 1.4f);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _popup?.Dispose();
            _calendar?.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Month grid used by <see cref="MiuDateBox"/>. Monday-first, six rows, primary-filled
/// selection, primary ring around today, optional "clear" row at the bottom.
/// </summary>
internal sealed class MiuCalendarPanel : MiuPopupCard
{
    private const int Cell = 32;
    private const int PadX = 12;
    private const int HeaderH = 40;
    private const int WeekH = 24;
    private const int ClearH = 34;
    private const int BottomPad = 10;

    private DateTime _month;
    private DateTime? _selected;
    private bool _showClear;
    private int _hoverDay;
    private int _hoverNav;
    private bool _hoverClear;

    public event Action<DateTime> Picked;
    public event Action Cleared;

    public MiuCalendarPanel()
    {
        Cursor = Cursors.Hand;
        Reset(DateTime.Today, false);
    }

    public void Reset(DateTime value, bool showClear)
    {
        _selected = value.Date;
        _month = new DateTime(value.Year, value.Month, 1);
        _showClear = showClear;
        _hoverDay = 0;
        _hoverNav = 0;
        _hoverClear = false;
        Width = PadX * 2 + Cell * 7;
        Height = HeaderH + WeekH + Cell * 6 + (_showClear ? ClearH : 0) + BottomPad;
        Invalidate();
    }

    private int GridTop => HeaderH + WeekH;
    private int DayCount => DateTime.DaysInMonth(_month.Year, _month.Month);
    private int Offset => ((int)_month.DayOfWeek + 6) % 7; // Monday-first

    private Rectangle PrevRect => new(Width - PadX - 60, 7, 26, 26);
    private Rectangle NextRect => new(Width - PadX - 30, 7, 26, 26);
    private Rectangle ClearRect => new(PadX, GridTop + Cell * 6 + 2, Cell * 7, ClearH - 6);

    private Rectangle CellRect(int index)
    {
        int row = index / 7, col = index % 7;
        return new Rectangle(PadX + col * Cell, GridTop + row * Cell, Cell, Cell);
    }

    private int DayAt(Point p)
    {
        if (p.Y < GridTop || p.X < PadX) return 0;
        int col = (p.X - PadX) / Cell;
        int row = (p.Y - GridTop) / Cell;
        if (col > 6 || row > 5) return 0;
        int day = row * 7 + col - Offset + 1;
        return day >= 1 && day <= DayCount ? day : 0;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int day = DayAt(e.Location);
        int nav = PrevRect.Contains(e.Location) ? -1 : NextRect.Contains(e.Location) ? 1 : 0;
        bool clear = _showClear && ClearRect.Contains(e.Location);
        if (day != _hoverDay || nav != _hoverNav || clear != _hoverClear)
        {
            _hoverDay = day;
            _hoverNav = nav;
            _hoverClear = clear;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hoverDay = 0;
        _hoverNav = 0;
        _hoverClear = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        _month = _month.AddMonths(e.Delta > 0 ? -1 : 1);
        Invalidate();
        base.OnMouseWheel(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;

        if (PrevRect.Contains(e.Location)) { _month = _month.AddMonths(-1); _hoverDay = 0; Invalidate(); return; }
        if (NextRect.Contains(e.Location)) { _month = _month.AddMonths(1); _hoverDay = 0; Invalidate(); return; }
        if (_showClear && ClearRect.Contains(e.Location)) { Cleared?.Invoke(); return; }

        int day = DayAt(e.Location);
        if (day > 0) Picked?.Invoke(new DateTime(_month.Year, _month.Month, day));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Header: "2026年11月" on the left, ‹ › on the right.
        DrawText(g, _month.ToString("yyyy年M月"), MiuTheme.Font, MiuTheme.Text,
            new Rectangle(PadX, 0, Width - PadX * 2 - 64, HeaderH));

        if (_hoverNav == -1)
            using (var b = new SolidBrush(MiuTheme.Colors.SurfaceContainerHigh))
            using (var p = RoundedPath.Rect(PrevRect, MiuTheme.Dp(8))) g.FillPath(b, p);
        if (_hoverNav == 1)
            using (var b = new SolidBrush(MiuTheme.Colors.SurfaceContainerHigh))
            using (var p = RoundedPath.Rect(NextRect, MiuTheme.Dp(8))) g.FillPath(b, p);

        MiuGlyph.Arrow(g, PrevRect.X + 13, PrevRect.Y + 13, 11f, MiuTheme.TextSecondary, right: false);
        MiuGlyph.Arrow(g, NextRect.X + 13, NextRect.Y + 13, 11f, MiuTheme.TextSecondary, right: true);

        // Weekday header.
        string[] names = { "一", "二", "三", "四", "五", "六", "日" };
        for (int i = 0; i < 7; i++)
        {
            var r = new Rectangle(PadX + i * Cell, HeaderH, Cell, WeekH);
            TextRenderer.DrawText(g, names[i], MiuTheme.FontSmall, r, MiuTheme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        using var todayRing = new Pen(MiuTheme.Primary, 1.4f);
        for (int i = 0; i < 42; i++)
        {
            int day = i - Offset + 1;
            if (day < 1 || day > DayCount) continue;

            var cell = CellRect(i);
            var inner = new Rectangle(cell.X + 2, cell.Y + 2, Cell - 4, Cell - 4);
            bool isSelected = _selected.HasValue
                && _selected.Value.Year == _month.Year
                && _selected.Value.Month == _month.Month
                && _selected.Value.Day == day;
            bool isToday = DateTime.Today.Year == _month.Year
                && DateTime.Today.Month == _month.Month
                && DateTime.Today.Day == day;

            if (isSelected)
            {
                using var p = RoundedPath.Rect(inner, MiuTheme.Dp(9));
                using var b = new SolidBrush(MiuTheme.Primary);
                g.FillPath(b, p);
            }
            else if (day == _hoverDay)
            {
                using var p = RoundedPath.Rect(inner, MiuTheme.Dp(9));
                using var b = new SolidBrush(MiuTheme.Colors.SurfaceContainerHigh);
                g.FillPath(b, p);
            }

            if (isToday && !isSelected)
            {
                using var p = RoundedPath.Rect(inner, MiuTheme.Dp(9));
                g.DrawPath(todayRing, p);
            }

            Color fg = isSelected ? MiuTheme.OnPrimary : MiuTheme.Text;
            TextRenderer.DrawText(g, day.ToString(CultureInfo.InvariantCulture), MiuTheme.Font, cell, fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        if (_showClear)
        {
            if (_hoverClear)
                using (var b = new SolidBrush(MiuTheme.Colors.TertiaryContainer))
                using (var p = RoundedPath.Rect(ClearRect, MiuTheme.Dp(8))) g.FillPath(b, p);
            TextRenderer.DrawText(g, "清除（不限）", MiuTheme.Font,
                ClearRect, _hoverClear ? MiuTheme.Primary : MiuTheme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }
}
