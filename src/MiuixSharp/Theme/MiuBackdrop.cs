using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MiuixSharp.Theme;

/// <summary>Window-backdrop materials understood by the Windows 11 DWM.</summary>
public enum MiuSystemBackdrop
{
    /// <summary>Plain desktop (no material).</summary>
    None = 1,
    /// <summary>Blurred wallpaper sampled behind the window (Mica).</summary>
    Mica = 2,
    /// <summary>Acrylic blur.</summary>
    Acrylic = 3,
    /// <summary>Mica variant for tabbed shells.</summary>
    Tabbed = 4,
}

/// <summary>
/// Window-frame extras that WinForms does not expose: a dark caption bar that follows the
/// active Miuix theme, rounded window corners, and the Windows 11 system backdrop
/// (Mica / Acrylic) behind the caption.
///
/// All of it is best-effort through <c>DwmSetWindowAttribute</c>: on Windows 10 or older
/// builds the calls fail silently and the window keeps its default frame. Call after the
/// handle exists (or use <see cref="Attach"/>), and call again after a theme change.
/// </summary>
public static class MiuBackdrop
{
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    /// <summary>
    /// Applies the dark/light caption bar, rounded corners and (optionally) the system
    /// backdrop to <paramref name="form"/>. Safe to call repeatedly.
    /// </summary>
    /// <returns>true when the DWM accepted the backdrop attribute (Windows 11 22621+).</returns>
    public static bool Apply(Form form, MiuSystemBackdrop backdrop = MiuSystemBackdrop.None)
    {
        if (form == null || form.IsDisposed || form.Handle == IntPtr.Zero) return false;

        int dark = MiuTheme.IsDark ? 1 : 0;
        int round = 2; // DWMWCP_ROUND
        int material = (int)backdrop;

        try { DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int)); } catch { }
        try { DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int)); } catch { }
        try { return DwmSetWindowAttribute(form.Handle, DWMWA_SYSTEMBACKDROP_TYPE, ref material, sizeof(int)) == 0; }
        catch { return false; }
    }

    /// <summary>
    /// Keeps the frame in sync with the theme for the lifetime of the form: applies once the
    /// handle exists, re-applies on every theme change, and detaches when the form is disposed.
    /// </summary>
    public static void Attach(Form form, MiuSystemBackdrop backdrop = MiuSystemBackdrop.None)
    {
        if (form == null) return;

        void ApplyNow() => Apply(form, backdrop);

        EventHandler onTheme = (_, __) => { if (!form.IsDisposed) ApplyNow(); };
        form.HandleCreated += (_, __) => ApplyNow();
        form.Disposed += (_, __) => MiuTheme.Changed -= onTheme;
        MiuTheme.Changed += onTheme;
        if (form.IsHandleCreated) ApplyNow();
    }
}
