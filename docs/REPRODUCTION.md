# Miuix 复现度对照清单

本文件记录 MiuixSharp 对 [Miuix](https://github.com/compose-miuix-ui/miuix) 设计语言和组件的复现核对结果。
核对方式：遍历 Miuix 官方源码树（GitHub API tree）、组件文档（`docs/components/**`）与主题文档
（`docs/guide/colors.md`、`theme.md`、`textstyles.md`），逐项与 MiuixSharp 的实现比对。

图例：✅ 完整复现 · ◐ 部分复现（核心视觉一致，省略平台专属能力） · ⬜ 未实现（不适用于 WinForms 或优先级低）

## 1. 主题与颜色系统

| Miuix 能力 | 官方规格 | MiuixSharp | 状态 |
|---|---|---|---|
| 亮色方案 | 官方 lightColorScheme 全部命名色角色 | `MiuColorScheme.Light()` 逐色值移植（ARGB） | ✅ |
| 暗色方案 | 官方 darkColorScheme | `MiuColorScheme.Dark()` 逐色值移植 | ✅ |
| 命名色角色 | primary / onPrimary / primaryVariant / primaryContainer / tertiaryContainer / secondary / surface 分层 / error / outline / dividerLine / windowDimming / slider 系列 / disabled 系列（约 50 个） | 全部字段化，数值与官方文档一致 | ✅ |
| 深浅模式 | System / Light / Dark | `MiuMode` 三态；System 读 Windows `AppsUseLightTheme` 注册表自动跟随 | ✅ |
| Monet 动态色 | seedColor + 6 种 PaletteStyle + Spec2021/2025 | `MiuTheme.Seed` 触发色板生成；`MiuPaletteStyle` 支持 TonalSpot/Neutral/Vibrant/Expressive/Monochrome | ◐ 无 Spec2021/2025 切换（以简化 tonal 生成替代），风格名与行为对齐 |
| 主题响应 | 切换主题全局刷新 | `MiuTheme.Changed` 事件，所有控件订阅并自动重绘 | ✅ |
| 字体样式 | title1=32/title2=24/title3=20/title4=18, main/body/button=17, subtitle=14 Bold, footnote2=11 等 14 种 | `MiuTextStyles` 全部 14 种按 sp≈pt 移植 | ✅ |

## 2. 基础组件（Basic）

| Miuix 组件 | 官方默认规格 | MiuixSharp 实现 | 状态 |
|---|---|---|---|
| Button | 圆角 16dp、MinHeight 40dp、MinWidth 58dp、内边距 16/13dp；primary/secondary/text 三类 | `MiuButton` 5 样式（Filled/Tonal/Outlined/Text/Warning）、16px 圆角、40px 高、hover/press | ✅ |
| Card | 圆角 16dp、内边距 0、按 surface 分层（无投影）、可交互态 | `MiuCard` 16px 圆角、surface 填充、可选投影（默认关闭）、可选左侧强调条；点击/长按反馈未做 | ◐ |
| Switch | checked/unchecked × thumb/track × disabled 共 8 色 | `MiuSwitch` 圆角轨道+滑块，主色/禁用色角色对齐 | ✅ |
| Checkbox | 三态 On/Off/Indeterminate，圆角方块，indeterminate 显示横杠 | `MiuCheckBox` 三态齐全，颜色角色对齐 | ✅ |
| RadioButton | 选中内点 | `MiuRadioButton` 圆环+内点 | ✅ |
| Slider | MinHeight 28dp、KeyPointRadius 3.855dp、magnetThreshold 0.02、showKeyPoints | `MiuSlider` 全部对齐，含关键点/磁吸/ValueChangedFinished | ✅ |
| Divider | 厚度 0.75dp、dividerLine 色；Horizontal/Vertical 两种 | `MiuDivider` 0.75px、双向支持 | ✅ |
| TextField | 圆角 16dp、内边距 16dp、背景 secondaryContainer、label onSecondaryContainer、边框 primary；浮动 label | `MiuInput`（= `MiuField` 圆角外壳 + 内嵌 TextBox）对齐圆角/内边距/配色，聚焦时边框转 primary；`MiuTextBox` 为 40px 高（16px 圆角）版本，并附加历史建议下拉 | ◐ 浮动 label 以 placeholder 替代，无动画 |
| DropdownMenu | 圆角面板、surface 色卡片、行 hover 高亮、选中项强调 | `MiuComboBox` 自绘圆角字段 + 自绘弹层（12px 圆角卡、32px 行高、选中行 tertiaryContainer 底 + primary 粗体） | ✅ 字段与弹层均无原生控件样式 |
| NumberPicker | Miuix 为滚轮/上下选择的 picker | 派生实现 `MiuNumberBox`：字段内右侧 `− / +` 分段步进、分隔线用 dividerLine、直接键入数字 | ◐ 语义按桌面场景推导，非像素级复刻 |
| DatePicker | Miuix 无内置日期选择器 | 派生实现 `MiuDateBox`：圆角字段 + hairline 日历图标 + 自绘月历（32px 格子、选中 primary 填充、今天 primary 描边、可清除） | ◐ 按 Miuix 设计语言推导 |
| IconButton | 40dp 全圆角 | `MiuIconButton` 40dp 圆形，文本字形图标，hover/press | ✅ |
| ProgressIndicator | 线性/圆形、indeterminate 动画 | `MiuProgressIndicator` 双形态， indeterminate 旋转/滑动 | ✅ |
| Surface / Scaffold | 容器与页面骨架 | ⬜ 未实现（WinForms 表单本身即容器） | ⬜ |
| TabRow / NavigationBar / NavigationRail | 导航组件 | ⬜ 未实现 | ⬜ |
| TopAppBar / FloatingToolbar | 顶部/浮动工具栏 | ⬜ 未实现 | ⬜ |
| SearchBar / SmallTitle | 专用输入组件 | `MiuInput` + placeholder 可作为搜索框；SmallTitle 用 Label 字号控制 | ◐ 无胶囊搜索框样式 |
| Snackbar | 轻量提示 | ⬜ 未实现（WinForms 可用 MessageBox / 自绘 Toast 替代） | ⬜ |
| Text / Icon | 文本与图标 | 使用 WinForms 原生 Label / 字符图标 | ◐ |

## 3. 弹层 / 窗口（Overlay / Window）

| Miuix 组件 | MiuixSharp | 状态 |
|---|---|---|
| Dialog / BottomSheet / ListPopup / CascadingListPopup | 使用 WinForms 原生 Form / 上下文菜单 | ⬜ 未复刻（WinForms 有对应原生机制） |
| Window 版本同上 | 同上 | ⬜ |

## 4. Glass 毛玻璃系列

| Miuix 组件 | MiuixSharp | 状态 |
|---|---|---|
| GlassDialog / GlassTopAppBar / GlassTabRow / GlassFloatingActionButton 等 15 项 | — | ⬜ 未实现；WinForms 无原生亚克力/模糊背景，需 DWM 私有 API，平台不匹配 |

## 5. Preference 设置项系列

| Miuix 组件 | MiuixSharp | 状态 |
|---|---|---|
| SwitchPreference / CheckboxPreference / DropdownPreference / SpinnerPreference / ArrowPreference 等 | — | ⬜ 未实现；可用 MiuSwitch/MiuCheckBox + Label 组合替代 |

## 6. 结论

- **主题系统**：官方亮/暗色板、50+ 命名色角色、文字样式、深浅/System 模式 —— 完整复现。
  Monet 动态色为简化实现（风格名对齐，无 Spec 版本开关）。
- **核心基础组件**：Button / Card / Switch / Checkbox（三态）/ RadioButton / Slider / Divider / TextField / IconButton / ProgressIndicator —— 已复现并按官方 dp 数值对齐。
- **未复现部分**均属**平台专属**：Glass 毛玻璃系列（WinForms 无原生亚克力/模糊背景）、Compose 导航组件（Scaffold/TabRow/NavigationBar 等，WinForms 用原生 Form/菜单替代）、Preference 设置页（用 MiuSwitch/MiuCheckBox + Label 组合替代）、Snackbar 与各类弹层（用 WinForms 原生机制）。这些在桌面 WinForms 语境下要么有等价原生机制，要么需平台不支持的能力，属合理取舍。
- WinForms 平台适配（Miuix 无对应概念）：`MiuWinButton`（继承原生 Button 的圆角按钮，兼容旧代码）、
  `MiuSurface`（圆角容器，用于表格/分区卡片）、`MiuGrid`（套用 Miuix 配色的 DataGridView）。
- 额外增强：**历史建议下拉 + 单条删除**（`MiuInput` / `MiuTextBox`），Miuix 原库没有此能力。
- **输入类控件补全**（因 WinForms 的 TextBox/ComboBox/NumericUpDown/DateTimePicker 无法圆角化）：
  统一外壳 `MiuField` + `MiuInput` / `MiuComboBox` / `MiuNumberBox` / `MiuDateBox`，
  附自绘弹层 `MiuPopup`（去原生边框的 ToolStripDropDown）与 `MiuListPanel` / 月历网格。
- **原生外观替换件**（Miuix 原库没有，按其设计语言推导）：
  - `MiuScrollArea`：自绘滚动容器，替掉 WinForms 带箭头按钮的方角滚动条。
  - `MiuLogList`：只读历史列表，自绘细圆角滚动指示器，滚轮滚动。
  - `MiuThumb`：圆角图片框，照片裁进圆角内，替掉原生方角 PictureBox。
  - `MiuActionGrid`：DataGridView 子类，在 base.OnMouseMove 之后设置手型光标，
    因为 DataGridView 自身会在 MouseMove 里重置 Cursor（仅靠 CellMouseEnter 无效）。
  - `MiuTitleBar`：通过 DWM 的 `DWMWA_CAPTION_COLOR` / `DWMWA_TEXT_COLOR`
    把系统标题栏改成 Slate 色，不再是 Windows 默认强调蓝（Win11 22H2 起生效）。

> 复现的是**设计语言与视觉规格**，不是逐行代码移植（Miuix 是 Kotlin/Compose，无法在 C# 运行）。
> 所有复刻均注明出处，见仓库根目录 README 的「致谢」章节。
