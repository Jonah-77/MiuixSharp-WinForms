using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// The tall (40dp / radius 16dp) variant of <see cref="MiuInput"/>, matching Miuix's
/// TextField proportions. Kept as its own type so existing layouts that used it keep the
/// oversized, loose shape while compact desktop rows can use <see cref="MiuInput"/> directly.
/// </summary>
[System.Drawing.ToolboxBitmap(typeof(MiuTextBox), "MiuixSharp.Icons.MiuTextBox.bmp")]
[System.ComponentModel.Description("单行文本输入框")]
public class MiuTextBox : MiuInput
{
    public MiuTextBox()
    {
        Height = MiuTheme.ButtonMinHeight; // 40dp
        Radius = MiuTheme.RadiusMd;        // 16dp
        HistoryEnabled = true;
    }
}
