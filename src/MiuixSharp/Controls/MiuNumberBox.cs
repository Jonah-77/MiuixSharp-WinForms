using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A numeric stepper. Miuix ships no spinner, so this derives one from its own design
/// language: the field keeps the rounded tonal shell and gains a segmented
/// "− / +" zone on the right, divided by a hairline instead of the native
/// NumericUpDown arrows.
/// </summary>
public class MiuNumberBox : MiuField
{
    private const int ButtonWidth = 24;

    private readonly TextBox _box;
    private decimal _value;
    private decimal _minimum;
    private decimal _maximum = 999;
    private decimal _increment = 1;
    private int _hoverButton = -1; // 0 = minus, 1 = plus
    private bool _updating;

    public MiuNumberBox()
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
            TextAlign = HorizontalAlignment.Left,
        };
        _box.GotFocus += (_, __) => SetFocusState(true);
        _box.LostFocus += (_, __) => { SetFocusState(false); NormalizeText(); };
        _box.TextChanged += (_, __) => OnTextEdited();
        Controls.Add(_box);

        MiuThemeHook.Attach(this, () =>
        {
            _box.Font = MiuTheme.Font;
            _box.ForeColor = MiuTheme.Text;
            _box.BackColor = MiuTheme.Colors.SecondaryContainer;
            Invalidate();
        });
    }

    public decimal Value
    {
        get => _value;
        set
        {
            decimal v = Clamp(value);
            if (v == _value) { SyncText(); return; }
            _value = v;
            SyncText();
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public decimal Minimum
    {
        get => _minimum;
        set { _minimum = value; if (_maximum < _minimum) _maximum = _minimum; Value = _value; Invalidate(); }
    }

    public decimal Maximum
    {
        get => _maximum;
        set { _maximum = value; if (_minimum > _maximum) _minimum = _maximum; Value = _value; Invalidate(); }
    }

    public decimal Increment
    {
        get => _increment;
        set { _increment = value <= 0 ? 1 : value; }
    }

    private string _placeholder = string.Empty;

    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value ?? string.Empty;
            if (_box != null) _box.PlaceholderText = _placeholder;
        }
    }

    public event EventHandler ValueChanged;

    private decimal Clamp(decimal v) => Math.Min(_maximum, Math.Max(_minimum, v));

    private void SyncText()
    {
        if (_box == null) return;
        string text = _value.ToString("0.###", CultureInfo.CurrentCulture);
        if (_box.Text == text) return;
        _updating = true;
        _box.Text = text;
        _updating = false;
    }

    private void OnTextEdited()
    {
        if (_updating) return;
        if (decimal.TryParse(_box.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal parsed))
        {
            decimal v = Clamp(parsed);
            if (v != _value)
            {
                _value = v;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        Invalidate();
    }

    private void NormalizeText()
    {
        if (_box.Text.Length == 0) { _box.Text = _value.ToString("0.###", CultureInfo.CurrentCulture); return; }
        SyncText();
    }

    private void Step(int direction)
    {
        Value = _value + _increment * direction;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutBox();
    }

    private void LayoutBox()
    {
        if (_box == null) return;
        _box.Multiline = false;
        _box.AutoSize = true;
        _box.Left = InsideMargin;
        _box.Width = Math.Max(18, Width - InsideMargin - ButtonWidth * 2 - 14);
        _box.Top = Math.Max(0, (Height - _box.Height) / 2);
    }

    private Rectangle MinusRect => new(Width - ButtonWidth * 2 - 6, 3, ButtonWidth, Height - 6);
    private Rectangle PlusRect => new(Width - ButtonWidth - 6, 3, ButtonWidth, Height - 6);

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int hit = PlusRect.Contains(e.Location) ? 1 : MinusRect.Contains(e.Location) ? 0 : -1;
        if (hit != _hoverButton)
        {
            _hoverButton = hit;
            Cursor = hit >= 0 ? Cursors.Hand : Cursors.IBeam;
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        if (_hoverButton != -1)
        {
            _hoverButton = -1;
            Cursor = Cursors.IBeam;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        if (PlusRect.Contains(e.Location)) Step(1);
        else if (MinusRect.Contains(e.Location)) Step(-1);
        else _box.Focus();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (Focused) Step(e.Delta > 0 ? 1 : -1);
        base.OnMouseWheel(e);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Up) { Step(1); e.Handled = true; }
        else if (e.KeyCode == Keys.Down) { Step(-1); e.Handled = true; }
    }

    protected override void PaintContent(Graphics g, Rectangle shell)
    {
        // Hairline between the two stepper halves, mirroring Miuix's divider weight.
        int splitX = Width - ButtonWidth - 4;
        using (var pen = new Pen(MiuTheme.Colors.DividerLine, 1))
            g.DrawLine(pen, splitX, Height * 0.3f, splitX, Height * 0.7f);

        Color glyph = Enabled ? MiuTheme.TextSecondary : MiuTheme.Colors.DisabledOnSurface;

        if (_hoverButton == 0 && Enabled)
            using (var b = new SolidBrush(MiuTheme.Colors.SurfaceContainerHigh))
            using (var p = RoundedPath.Rect(new Rectangle(MinusRect.X + 2, MinusRect.Y + 1, MinusRect.Width - 4, MinusRect.Height - 2), MiuTheme.Dp(7)))
                g.FillPath(b, p);
        if (_hoverButton == 1 && Enabled)
            using (var b = new SolidBrush(MiuTheme.Colors.SurfaceContainerHigh))
            using (var p = RoundedPath.Rect(new Rectangle(PlusRect.X + 2, PlusRect.Y + 1, PlusRect.Width - 4, PlusRect.Height - 2), MiuTheme.Dp(7)))
                g.FillPath(b, p);

        MiuGlyph.Minus(g, MinusRect.X + MinusRect.Width / 2f, MinusRect.Y + MinusRect.Height / 2f, 9f,
            _hoverButton == 0 ? MiuTheme.Primary : glyph);
        MiuGlyph.Plus(g, PlusRect.X + PlusRect.Width / 2f, PlusRect.Y + PlusRect.Height / 2f, 9f,
            _hoverButton == 1 ? MiuTheme.Primary : glyph);

    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _box.Dispose();
        base.Dispose(disposing);
    }
}
