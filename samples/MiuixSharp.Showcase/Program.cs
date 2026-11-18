using System.Drawing;
using System.Drawing.Drawing2D;
using MiuixSharp.Controls;
using MiuixSharp.Theme;

namespace MiuixSharp.Showcase;

internal static class Program
{

    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Length > 0 && args[0] == "--dark") MiuTheme.Mode = MiuMode.Dark;
        for (int i = 0; i + 1 < args.Length; i++)
            if (args[i] == "--scale") MiuTheme.SetScale(float.Parse(args[i + 1]));
        Application.Run(new ShowcaseForm());

    }
}

internal class ShowcaseForm : Form
{
    private Panel _root;
    private readonly Bitmap _samplePic = CreateSamplePic();

    private static Bitmap CreateSamplePic()
    {
        var pic = new Bitmap(120, 90);
        using (var pg = Graphics.FromImage(pic))
        {
            pg.Clear(Color.SteelBlue);
            pg.FillEllipse(new SolidBrush(Color.Orange), 24, 16, 72, 54);
        }
        return pic;
    }
    // Fictional sample data only — no real customer names or business data.
    private readonly List<string> _customers = new()
    {
        "示例客户 A", "示例客户 B", "示例客户 C", "示例客户 D", "示例客户 E"
    };

    public ShowcaseForm()
    {
        Text = "MiuixSharp · 控件展示";
        // The whole window is laid out in dp, so its own size has to follow the scale too —
        // otherwise a 150% monitor renders 1.5x content inside a 100% window and clips it.
        MiuDpi.Watch(this);
        ApplyWindowSize();
        MiuTheme.Changed += (_, __) => ApplyWindowSize();
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = MiuTheme.Background;
        MiuTheme.Changed += (_, __) => { BackColor = MiuTheme.Background; MiuTitleBar.Apply(this); Invalidate(true); };
        // The OS caption bar is DWM-drawn, so it must be recoloured after the handle exists.
        HandleCreated += (_, __) => MiuTitleBar.Apply(this);
        Build();
    }

    private int _autoWidth = -1, _autoHeight = -1;

    /// <summary>
    /// 1020×1230 dp is the design size (the size the screenshots are taken at). On a
    /// higher-DPI monitor the window grows with the content; the user may still resize
    /// freely above the minimum, and the scroll host absorbs the rest.
    /// </summary>
    private void ApplyWindowSize()
    {
        int w = MiuTheme.Dp(1020), h = MiuTheme.Dp(1230);
        MinimumSize = new Size(w, MiuTheme.Dp(480));
        if (_autoWidth < 0 || Width == _autoWidth) Width = w;
        if (_autoHeight < 0 || Height == _autoHeight) Height = h;
        _autoWidth = w;
        _autoHeight = h;
    }

    // The demo lays everything out in dp (the Miuix unit); these helpers do the dp→px step
    // exactly once, so the whole window keeps its proportions at any DPI or zoom level.
    private static int D(int dp) => MiuTheme.Dp(dp);
    private static Point P(int x, int y) => new Point(D(x), D(y));
    private static Size S(int w, int h) => new Size(D(w), D(h));

    private void Build()
    {
        var rootHost = new MiuScrollArea { Dock = DockStyle.Fill, BackColor = MiuTheme.Background };
        var root = new Panel { BackColor = MiuTheme.Background, Padding = new Padding(D(28)) };
        rootHost.SetContent(root);

        // Header
        var title = new Label { Text = "MiuixSharp", Font = new Font("Microsoft YaHei", 22, FontStyle.Bold),
            ForeColor = MiuTheme.Text, AutoSize = true, Location = P(28, 24) };
        var sub = new Label { Text = "A Miuix-inspired control library for WinForms · 借鉴 Miuix 设计语言",
            Font = MiuTheme.Font, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(28, 82) };

        var themeBtn = new MiuButton { Text = "◐ 切换 深/浅 主题", Style = MiuButtonStyle.Tonal,
            Location = P(760, 36), Size = S(200, 40) };
        themeBtn.Click += (_, __) =>
        {
            MiuTheme.Mode = MiuTheme.Mode == MiuMode.Light ? MiuMode.Dark : MiuMode.Light;
            Rebuild();
        };

        root.Controls.Add(title); root.Controls.Add(sub); root.Controls.Add(themeBtn);

        int y = 124;
        _root = root;

        // ---- Card 1: Buttons ----
        var c1 = Card("Buttons · 按钮（5 种样式）", ref y, 122);
        int bx = 28;
        c1.Controls.AddAt(new MiuButton { Text = "主色 Filled", Style = MiuButtonStyle.Filled, Location = P(bx, 54), Size = S(128, 40) });
        c1.Controls.AddAt(new MiuButton { Text = "柔和 Tonal", Style = MiuButtonStyle.Tonal, Location = P(bx + 136, 54), Size = S(128, 40) });
        c1.Controls.AddAt(new MiuButton { Text = "描边 Outlined", Style = MiuButtonStyle.Outlined, Location = P(bx + 272, 54), Size = S(148, 40) });
        c1.Controls.AddAt(new MiuButton { Text = "文字 Text", Style = MiuButtonStyle.Text, Location = P(bx + 428, 54), Size = S(112, 40) });
        c1.Controls.AddAt(new MiuButton { Text = "警示 Warning", Style = MiuButtonStyle.Warning, Location = P(bx + 548, 54), Size = S(148, 40) });
        c1.Controls.AddAt(new MiuSwitch { Checked = true, Location = P(bx + 720, 56) });
        c1.Controls.AddAt(new Label { Text = "Switch", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(bx + 774, 62) });

        // ---- Card 2: Inputs with history ----
        var c2 = Card("Input · 输入框 / 下拉 / 数字 / 日期", ref y, 162);
        var nameBox = new MiuTextBox { Placeholder = "客户 / 收件人（点右侧 ▾ 看历史）", Location = P(28, 54), Size = S(380, 44), HistoryEnabled = true };
        nameBox.HistoryProvider = () => _customers;
        nameBox.OnDeleteHistory = v => { _customers.Remove(v); return true; };
        c2.Controls.AddAt(nameBox);

        var plain = new MiuTextBox { Placeholder = "普通输入框（无历史）", Location = P(428, 54), Size = S(280, 44), HistoryEnabled = false };
        c2.Controls.AddAt(plain);

        var dateBox = new MiuDateBox("2026-11-18", true) { Location = P(28, 112), Size = S(200, 40), Radius = 12 };
        c2.Controls.AddAt(dateBox);

        var combo = new MiuComboBox { Location = P(248, 112), Size = S(180, 40), Radius = 12 };
        combo.SetItems(new[] { "待回收", "已回收", "客户留存", "损耗报废" });
        combo.SelectedIndex = 0;
        c2.Controls.AddAt(combo);

        var num = new MiuNumberBox { Minimum = 1, Maximum = 99, Value = 5, Location = P(448, 112), Size = S(140, 40), Radius = 12 };
        c2.Controls.AddAt(num);


        // ---- Card 3: Chips ----
        var c3 = Card("Chips · 状态标签", ref y, 118);
        string[] chipText = { "待回收", "已回收", "必回", "免回", "客户留存", "逾期" };
        MiuChipTone[] tones = { MiuChipTone.Warning, MiuChipTone.Success, MiuChipTone.Primary, MiuChipTone.Muted, MiuChipTone.Neutral, MiuChipTone.Warning };
        for (int i = 0; i < chipText.Length; i++)
        {
            var chip = new MiuChip { Text = chipText[i], Tone = tones[i], Location = P(28 + i * 130, 54) };
            c3.Controls.AddAt(chip);
        }

        // ---- Card 4: Selection & progress ----
        var c4 = Card("Selection · 选择 / 滑块 / 分隔线", ref y, 136);
        c4.Controls.AddAt(new MiuCheckBox { Checked = true, Location = P(28, 54) });
        c4.Controls.AddAt(new MiuCheckBox { State = MiuToggleState.Indeterminate, Location = P(28, 92) });
        c4.Controls.AddAt(new Label { Text = "Checkbox 勾选 / 半选", Font = MiuTheme.Font, ForeColor = MiuTheme.Text, AutoSize = true, Location = P(56, 54) });
        c4.Controls.AddAt(new MiuRadioButton { Checked = true, Location = P(56, 92) });
        c4.Controls.AddAt(new Label { Text = "Radio 选中", Font = MiuTheme.Font, ForeColor = MiuTheme.Text, AutoSize = true, Location = P(84, 95) });
        var slider = new MiuSlider { Value = 65, Location = P(260, 62), Size = S(300, 32) };
        slider.KeyPoints.AddRange(new[] { 0, 25, 50, 75, 100 });
        c4.Controls.AddAt(slider);
        c4.Controls.AddAt(new Label { Text = "Slider · 65", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(260, 100) });
        var divider = new MiuDivider { Location = P(620, 48), Size = S(280, 1) };
        c4.Controls.AddAt(divider);
        c4.Controls.AddAt(new MiuSwitch { Checked = true, Location = P(620, 72) });
        c4.Controls.AddAt(new Label { Text = "Switch on", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(676, 76) });

        // ---- Card 5: IconButton & progress ----
        var c5 = Card("IconButton / Progress · 图标按钮与进度", ref y, 142);
        c5.Controls.AddAt(new MiuIconButton { Text = "✚", Style = MiuButtonStyle.Filled, Location = P(28, 50) });
        c5.Controls.AddAt(new MiuIconButton { Text = "♥", Location = P(78, 50) });
        c5.Controls.AddAt(new MiuIconButton { Text = "⚙", Location = P(128, 50) });
        c5.Controls.AddAt(new Label { Text = "40dp 圆形图标按钮", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(28, 96) });
        var spinner = new MiuProgressIndicator { Circular = true, Location = P(260, 48) };
        c5.Controls.AddAt(spinner);
        var linear = new MiuProgressIndicator { Circular = false, Progress = 70, Location = P(320, 62), Size = S(220, 6) };
        c5.Controls.AddAt(linear);
        c5.Controls.AddAt(new Label { Text = "Circular / Linear · indeterminate & determinate", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(320, 78) });

        // ---- Card 6: native-chrome replacements ----
        var c6 = Card("Native look · 圆角缩略图 / 历史列表 / 自绘滚动", ref y, 186);

        // 圆角缩略图（图片裁进圆角内）
        var thumb = new MiuThumb { Image = _samplePic, Radius = 14, Location = P(28, 52), Size = S(120, 90) };
        c6.Controls.AddAt(thumb);
        c6.Controls.AddAt(new Label { Text = "MiuThumb", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(28, 156) });

        // 历史列表（自绘滚动指示器，无原生方角滚动条）
        var log = new MiuLogList { Location = P(176, 52), Size = S(380, 96), Radius = 10, LineHeight = 20 };
        log.SetLines(new[]
        {
            "2026-11-18 10:22  create  · 批量新建",
            "2026-11-18 10:23  update  · 状态  待回收 → 已回收",
            "2026-11-18 10:31  update  · 备注  催还一次",
            "2026-11-18 11:02  update  · 寄出日期  2026-11-16",
            "2026-11-18 14:18  update  · 状态  已回收 → 待回收",
            "2026-11-18 15:40  note  · 客户要求下周寄回",
            "2026-11-18 16:05  update  · 约定归还日  2026-12-01",
            "2026-11-18 17:20  update  · 颜色  藏青",
        });
        c6.Controls.AddAt(log);
        c6.Controls.AddAt(new Label { Text = "MiuLogList · 滚轮滚动", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(176, 156) });

        // 自绘滚动容器
        var inner = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, WrapContents = false,
            BackColor = MiuTheme.Colors.SecondaryContainer, Padding = new Padding(D(8)),
        };
        for (int i = 0; i < 8; i++)
        {
            inner.Controls.Add(new Label
            {
                Text = "ScrollArea row " + (i + 1),
                Font = MiuTheme.FontSmall, ForeColor = MiuTheme.Text,
                AutoSize = true, Margin = new Padding(D(2), D(1), D(2), D(1)),
            });
        }
        var scroll = new MiuScrollArea { Location = P(584, 52), Size = S(300, 96) };
        scroll.SetContent(inner);
        c6.Controls.AddAt(scroll);
        c6.Controls.AddAt(new Label { Text = "MiuScrollArea", Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true, Location = P(584, 156) });

        // footer note
        var note = new Label
        {
            Text = "Official Miuix light/dark scheme  ·  Seed + palette styles  ·  16dp rounded components",
            Font = MiuTheme.FontSmall, ForeColor = MiuTheme.TextSecondary, AutoSize = true,
            Location = P(28, y + 6)
        };
        root.Controls.Add(note);

        Controls.Clear();
        Controls.Add(rootHost);
    }

    private MiuCard Card(string title, ref int y, int height)
    {
        var card = new MiuCard { Location = P(20, y), Size = S(940, height) };
        var lbl = new Label { Text = title, Font = new Font("Microsoft YaHei", 10.5F, FontStyle.Bold),
            ForeColor = MiuTheme.Text, AutoSize = true, Location = P(18, 14) };
        card.Controls.AddAt(lbl);
        _root.Controls.Add(card);
        y += height + 18;
        return card;
    }

    private void Rebuild() { SuspendLayout(); Build(); ResumeLayout(true); }
}

internal static class Ext
{
    public static void AddAt(this Control.ControlCollection c, Control ctrl) => c.Add(ctrl);
}

