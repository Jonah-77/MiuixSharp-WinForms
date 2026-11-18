using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// The tall (40dp / radius 16dp) variant of <see cref="MiuInput"/>, matching Miuix's
/// TextField proportions. Kept as its own type so existing layouts that used it keep the
/// oversized, loose shape while compact desktop rows can use <see cref="MiuInput"/> directly.
/// </summary>
public class MiuTextBox : MiuInput
{
    public MiuTextBox()
    {
        Height = MiuTheme.ButtonMinHeight; // 40dp
        Radius = MiuTheme.RadiusMd;        // 16dp
        HistoryEnabled = true;
    }
}
