using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// Recolours the operating-system title bar so it matches the Miuix palette instead of
/// the default Windows accent blue. Implemented through the undocumented-but-stable
/// <c>DwmSetWindowAttribute</c> attributes available since Windows 11 22H2
/// (DWMWA_CAPTION_COLOR / DWMWA_TEXT_COLOR).
///
/// On older Windows versions the calls simply fail and the system title bar keeps its
/// default look — the helper is safe to call unconditionally.
/// </summary>
public static class MiuTitleBar
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_CAPTION_COLOR = 35;
    private const int DWMWA_TEXT_COLOR = 36;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    /// <summary>Apply the current theme's colours to the title bar of <paramref name="form"/>.</summary>
    public static void Apply(Form form)
    {
        if (form == null) return;
        var caption = MiuTheme.IsDark
            ? MiuTheme.Colors.SurfaceContainer
            : MiuTheme.Colors.SurfaceContainerHigh;
        var text = MiuTheme.IsDark
            ? MiuTheme.Colors.OnSurface
            : MiuTheme.Colors.OnSurface;
        Apply(form, caption, text);
    }

    /// <summary>Apply explicit colours to the title bar of <paramref name="form"/>.</summary>
    public static void Apply(Form form, Color caption, Color text)
    {
        if (form == null || form.IsDisposed) return;
        if (form.Handle == IntPtr.Zero) return;

        int dark = MiuTheme.IsDark ? 1 : 0;
        try { DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int)); } catch { }

        int c = ToColorRef(caption);
        int t = ToColorRef(text);
        try { DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref c, sizeof(int)); } catch { }
        try { DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref t, sizeof(int)); } catch { }
    }

    /// <summary>COLORREF is 0x00BBGGRR — blue first, not red.</summary>
    private static int ToColorRef(Color c) => c.R | (c.G << 8) | (c.B << 16);
}
