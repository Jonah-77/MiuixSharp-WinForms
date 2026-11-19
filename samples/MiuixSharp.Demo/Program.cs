using System.Drawing;
using System.Windows.Forms;
using MiuixSharp.Controls;
using MiuixSharp.Theme;

namespace MiuixSharp.Demo;

/// <summary>
/// Live demo of MiuixSharp 0.4.4: the interactive surface on top, then the parts a screenshot
/// cannot show — keyboard focus, high-contrast mapping, live metric recalculation, and the
/// window-frame integration.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Optional switches, handy for screenshots and quick checks:
        //   --dark     start in the dark palette
        //   --system   follow the Windows app theme
        foreach (var a in Environment.GetCommandLineArgs())
        {
            if (a.Equals("--dark", StringComparison.OrdinalIgnoreCase)) MiuTheme.Mode = MiuMode.Dark;
            if (a.Equals("--system", StringComparison.OrdinalIgnoreCase)) MiuTheme.Mode = MiuMode.System;
        }
        Application.Run(new DemoForm());
    }
}

internal sealed class DemoForm : Form
{
    private static int D(int dp) => MiuTheme.Dp(dp);

    // Page backdrop: a touch darker than MiuTheme.Surface so the cards read as cards.
    private static Color Page => MiuTheme.IsDark
        ? Color.FromArgb(0x1C, 0x1C, 0x1C)
        : Color.FromArgb(0xEE, 0xF1, 0xF6);
    private static Size S(int w, int h) => new(D(w), D(h));

    private readonly FlowLayoutPanel _flow = new()
    {
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
    };
    private readonly Label _head = new(), _hint = new(), _status = new();
    private readonly MiuCard _c1 = new(), _c2 = new(), _c3 = new();

    // ── card 1: interaction ───────────────────────────────────────────────
    private readonly MiuButton _bFilled = new() { Text = "主色按钮", Style = MiuButtonStyle.Filled, Cursor = Cursors.Hand };
    private readonly MiuButton _bOutlined = new() { Text = "描边按钮", Style = MiuButtonStyle.Outlined };
    private readonly MiuIconButton _icon = new() { Text = "✚", Style = MiuButtonStyle.Filled };
    private readonly MiuSwitch _switch = new() { Checked = true };
    private readonly Label _lSwitch = new();
    private readonly MiuCheckBox _check = new() { Checked = true };
    private readonly Label _lCheck = new();
    private readonly MiuRadioButton _r1 = new() { Checked = true };
    private readonly MiuRadioButton _r2 = new();
    private readonly Label _lRadio = new();
    private readonly MiuChip _chip = new() { Text = "已回收", Tone = MiuChipTone.Success };
    // Derives from Button (legacy compatible) — here so the rounded corners and the 1 px
    // outline of that code path can be inspected at 150 % zoom next to the plain MiuButton.
    private readonly MiuWinButton _bWin = new() { Text = "原生按钮", MiuStyle = MiuButtonStyle.Outlined };
    private readonly MiuSlider _slider = new() { Value = 60 };
    private readonly Label _lValue = new();

    // ── card 2: editors and progress ──────────────────────────────────────
    private readonly MiuTextBox _input = new() { Placeholder = "在这里输入试试…", HistoryEnabled = false };
    private readonly MiuNumberBox _number = new() { Minimum = 1, Maximum = 99, Value = 5, Radius = 12 };
    private readonly MiuComboBox _combo = new() { Radius = 12 };
    private readonly MiuProgressIndicator _linear = new() { Circular = false, Progress = 65 };
    private readonly MiuProgressIndicator _spinner = new() { Circular = true };

    // ── card 3: theme and system ──────────────────────────────────────────
    private readonly MiuButton _bMode = new() { Text = "切换 深/浅", Style = MiuButtonStyle.Tonal };
    private readonly MiuButton _bAccent = new() { Text = "强调色：种子", Style = MiuButtonStyle.Outlined };
    private readonly MiuButton _bFrame = new() { Text = "请求 Mica 背板", Style = MiuButtonStyle.Outlined };
    private readonly MiuSwitch _switchHc = new();
    private readonly Label _lHc = new();
    private readonly MiuSlider _scale = new() { Minimum = 100, Maximum = 200, Value = 100 };
    private readonly Label _lScale = new();

    private int _autoW = -1, _autoH = -1;

    public DemoForm()
    {
        Text = "MiuixSharp · 能力演示";
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Page;
        AutoScaleMode = AutoScaleMode.None;
        KeyPreview = true;

        _head.Text = "MiuixSharp 0.4.4";
        _head.Font = new Font(MiuTheme.Font.FontFamily, 18F, FontStyle.Bold);
        _head.ForeColor = MiuTheme.Text;
        _hint.Text = "Tab 轮换焦点 · 空格激活勾选/开关/按钮 · 方向键调滑块 · 下面还能改缩放、开高对比";
        _hint.Font = MiuTheme.FontSmall;
        _hint.ForeColor = MiuTheme.TextSecondary;

        _status.Font = MiuTheme.FontSmall;
        _status.ForeColor = MiuTheme.TextSecondary;
        _status.BackColor = Page;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.Padding = new Padding(D(16), 0, D(8), 0);

        _c1.Accent = true;

        // One vertical flow panel owns everything: header, hint, status line, then the cards.
        // No docking, so z-order cannot hide a strip behind the scroll host.
        _flow.BackColor = Page;
        _flow.Padding = new Padding(D(16), D(10), D(16), D(14));
        Controls.Add(_flow);
        _flow.Dock = DockStyle.Fill;

        _head.Margin = new Padding(0, 0, 0, D(2));
        _hint.Margin = new Padding(0, 0, 0, D(6));
        _status.Margin = new Padding(0, 0, 0, D(12));
        _c1.Margin = _c2.Margin = _c3.Margin = new Padding(0, 0, 0, D(14));

        _flow.Controls.Add(_head);
        _flow.Controls.Add(_hint);
        _flow.Controls.Add(_status);
        _flow.Controls.Add(_c1);
        _flow.Controls.Add(_c2);
        _flow.Controls.Add(_c3);

        BuildCard1();
        BuildCard2();
        BuildCard3();

        _slider.ValueChanged += (_, __) => { _lValue.Text = "当前值 " + _slider.Value; };
        _lValue.Text = "当前值 " + _slider.Value;

        _bMode.Click += (_, __) => MiuTheme.Mode = MiuTheme.IsDark ? MiuMode.Light : MiuMode.Dark;
        _bAccent.Click += (_, __) =>
        {
            var next = MiuTheme.AccentSource == MiuAccentSource.System ? MiuAccentSource.Seed : MiuAccentSource.System;
            MiuTheme.AccentSource = next;
            ApplyLocale();
        };
        _bFrame.Click += (_, __) =>
        {
            bool ok = MiuBackdrop.Apply(this, MiuSystemBackdrop.Mica);
            _frameNote = ok
                ? "已请求 Mica（Win11 22H2+）；WinForms 客户区自绘会盖住它，能看到的只有标题栏"
                : "本机不支持 Mica（需要 Win11 22H2+）";
            ApplyLocale();
        };
        _switchHc.CheckedChanged += (_, __) =>
        {
            // true = force high contrast on, false = follow the Windows setting.
            MiuTheme.ForceHighContrast = _switchHc.Checked ? true : null;
            MiuTheme.RefreshHighContrast();
        };
        _scale.ValueChanged += (_, __) =>
        {
            MiuTheme.SetScale(_scale.Value / 100f);
            _lScale.Text = _scale.Value + "%";
            ApplyWindowSize();
            LayoutAll();
        };

        _r1.Click += (_, __) => { _r2.Checked = false; };
        _r2.Click += (_, __) => { _r1.Checked = false; };

        _combo.SetItems(new[] { "待回收", "已回收", "客户留存", "逾期" });
        _combo.SelectedIndex = 1;

        MiuDpi.Watch(this);
        MiuTheme.Changed += (_, __) => { ApplyLocale(); LayoutAll(); };
        MiuBackdrop.Attach(this);
        ApplyWindowSize();
        ApplyLocale();
        LayoutAll();
    }

    private string _frameNote = "";

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // The ctor runs before the handle exists, so DPI (and therefore MiuTheme.Scale)
        // may still be 1.0 there. Re-apply the window size once the real scale is known,
        // otherwise the dp content is laid out at 150 % inside a 100 % window and clips.
        ApplyWindowSize();
        LayoutAll();
        // Adding autoscroll children leaves the panel scrolled down; pin it back to the top.
        _flow.ScrollControlIntoView(_head);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutAll();
    }

    // ── construction ─────────────────────────────────────────────────────
    private Label Lbl(string text = "", bool strong = false)
    {
        var l = new Label
        {
            Text = text,
            Font = strong ? MiuTheme.FontTitle : MiuTheme.Font,
            ForeColor = MiuTheme.Text,
            BackColor = Color.Transparent,
            AutoSize = false,
        };
        return l;
    }

    private void CardTitle(MiuCard card, string text)
    {
        var l = Lbl(text, true);
        l.Name = "title";
        card.Controls.Add(l);
    }

    private void BuildCard1()
    {
        CardTitle(_c1, "一、交互与键盘");
        _c1.Controls.AddRange(new Control[]
        {
            _bFilled, _bOutlined, _icon, _switch, _lSwitch, _check, _lCheck,
            _r1, _r2, _lRadio, _chip, _slider, _lValue, _bWin,
        });
        _lSwitch.Text = "开关";
        _lCheck.Text = "勾选";
        _lRadio.Text = "单选";
        AddFocusDemo();
    }

    private void AddFocusDemo()
    {
        // One button whose only job is to move focus, so the focus rectangle is easy to see.
        var focus = new MiuButton { Text = "把焦点交给开关", Style = MiuButtonStyle.Text };
        focus.Click += (_, __) => _switch.Focus();
        _c1.Controls.Add(focus);
        focus.Name = "focusBtn";
    }

    private void BuildCard2()
    {
        CardTitle(_c2, "二、输入与进度");
        _c2.Controls.AddRange(new Control[] { _input, _number, _combo, _linear, _spinner, Lbl("线性 / 环形，可确定也可不确定") });
    }

    private void BuildCard3()
    {
        CardTitle(_c3, "三、主题与系统");
        _c3.Controls.AddRange(new Control[]
        {
            _bMode, _bAccent, _bFrame, _switchHc, _lHc, _scale, _lScale, Lbl("缩放范围 1.0–5.0，超出会被夹住；内容按 dp 重算，不是位图拉伸"),
        });
        _lHc.Text = "高对比模拟";
        _lScale.Text = "100%";
    }

    // ── layout ───────────────────────────────────────────────────────────
    private void ApplyWindowSize()
    {
        var area = Screen.FromPoint(Cursor.Position).WorkingArea;
        int w = Math.Min(D(760), area.Width - D(40));
        int h = Math.Min(D(700), area.Height - D(80));
        MinimumSize = S(560, 520);
        if (_autoW < 0 || (Width == _autoW && Height == _autoH)) Size = new Size(w, h);
        _autoW = w;
        _autoH = h;
    }

    private static void At(MiuCard card, Control c, int x, int y, int w, int h)
        => c.Bounds = new Rectangle(card.Padding.Left + D(x), card.Padding.Top + D(y), D(w), D(h));

    private void LayoutAll()
    {
        int pad = _flow.Padding.Horizontal;
        int gutter = D(20);   // space for the scrollbar so the cards never sit under it
        int w = Math.Max(D(520), _flow.ClientSize.Width - pad - gutter);
        int title = D(28);

        _head.Width = w; _head.Height = D(36);
        _hint.Width = w; _hint.Height = D(24);
        _status.Width = w; _status.Height = D(30);
        _c1.Width = _c2.Width = _c3.Width = w;
        _c1.Height = D(214); _c2.Height = D(150); _c3.Height = D(206);

        At(_c1, _c1.Controls["title"], 20, 10, 260, 26);
        At(_c1, _bFilled, 20, title + 14, 120, 40);
        At(_c1, _bOutlined, 152, title + 14, 120, 40);
        At(_c1, _icon, 284, title + 14, 40, 40);
        At(_c1, _switch, 340, title + 21, 46, 26);
        At(_c1, _lSwitch, 396, title + 20, 90, 24);
        At(_c1, _check, 20, title + 64, 20, 20);
        At(_c1, _lCheck, 48, title + 62, 70, 24);
        At(_c1, _r1, 130, title + 64, 20, 20);
        At(_c1, _r2, 186, title + 64, 20, 20);
        At(_c1, _lRadio, 216, title + 62, 90, 24);
        At(_c1, _chip, 330, title + 62, 80, 26);
        At(_c1, _bWin, 430, title + 58, 130, 32);
        At(_c1, _slider, 20, title + 96, 380, 28);
        At(_c1, _lValue, 410, title + 96, 120, 24);
        At(_c1, _c1.Controls["focusBtn"], 540, title + 88, 150, 36);

        At(_c2, _c2.Controls["title"], 20, 10, 260, 26);
        At(_c2, _input, 20, 52, 260, 40);
        At(_c2, _number, 292, 52, 120, 40);
        At(_c2, _combo, 424, 52, 150, 40);
        At(_c2, _linear, 20, 108, 240, 6);
        At(_c2, _spinner, 276, 92, 40, 40);
        At(_c2, _c2.Controls.OfType<Label>().Last(), 332, 100, 240, 24);

        At(_c3, _c3.Controls["title"], 20, 10, 260, 26);
        At(_c3, _bMode, 20, 52, 120, 40);
        At(_c3, _bAccent, 152, 52, 150, 40);
        At(_c3, _bFrame, 314, 52, 150, 40);
        At(_c3, _switchHc, 486, 59, 46, 26);
        At(_c3, _lHc, 542, 58, 100, 24);
        At(_c3, _scale, 20, 112, 300, 28);
        At(_c3, _lScale, 330, 112, 60, 24);
        At(_c3, _c3.Controls.OfType<Label>().Last(), 20, 148, 520, 24);
    }

    // ── status line ──────────────────────────────────────────────────────
    private void ApplyLocale()
    {
        _bAccent.Text = MiuTheme.AccentSource == MiuAccentSource.System ? "强调色：系统" : "强调色：种子";
        _head.ForeColor = MiuTheme.Text;
        _hint.ForeColor = MiuTheme.TextSecondary;
        _status.BackColor = Page;
        _status.ForeColor = MiuTheme.TextSecondary;
        _flow.BackColor = Page;
        _status.Text =
            $"主题 {(_isDarkHint() ? "深色" : "浅色")} · 高对比 {(MiuTheme.IsHighContrast ? "开" : "关")}" +
            $" · 缩放 {MiuTheme.Scale * 100:0}% · 强调色 {MiuTheme.AccentSource}" +
            (_frameNote.Length > 0 ? " · " + _frameNote : "");

        // Plain Labels don't follow the theme on their own — repaint their ink here,
        // otherwise every caption inside the cards stays black on a black dark card.
        PaintLabels(this);
    }

    private static void PaintLabels(Control root)
    {
        foreach (Control c in root.Controls)
        {
            if (c is Label l) l.ForeColor = MiuTheme.Text;
            if (c.HasChildren) PaintLabels(c);
        }
    }

    /// <summary>IsDark on its own is cheap; the caption only needs a word.</summary>
    private bool _isDarkHint() => MiuTheme.IsDark;
}
