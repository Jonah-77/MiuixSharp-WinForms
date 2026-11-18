using System.Windows.Forms;
using MiuixSharp.Theme;

namespace MiuixSharp.Theme;

/// <summary>
/// Keeps <see cref="MiuTheme.Scale"/> in sync with the window's DPI.
///
/// MiuixSharp draws in device pixels but every metric is expressed in dp (Miuix's unit).
/// WinForms already rescales control *bounds* and fonts when the DPI changes; this helper
/// covers the other half — the values controls use inside OnPaint (radii, stroke widths,
/// glyph sizes, inner margins), which WinForms does not touch.
///
/// Usage (once per top-level window):
/// <code>MiuDpi.Watch(this);   // in the Form constructor</code>
/// </summary>
public static class MiuDpi
{
    /// <summary>Follows <paramref name="form"/>'s device DPI, now and on every DPI change.</summary>
    public static void Watch(Form form)
    {
        if (form == null) return;

        // Do NOT switch AutoScaleMode here: MiuixSharp scales itself from dp at draw time,
        // and WinForms' own scaling on top of that would double every metric.
        MiuTheme.SetDpi(form.DeviceDpi);

        form.HandleCreated += (_, __) => MiuTheme.SetDpi(form.DeviceDpi);
        form.DpiChanged += (_, e) => MiuTheme.SetDpi(e.DeviceDpiNew);
        form.DpiChangedAfterParent += (_, __) => MiuTheme.SetDpi(form.DeviceDpi);
    }

    /// <summary>Dp → px for a specific control (use when one window may sit on another monitor).</summary>
    public static int Dp(Control control, float dp) =>
        control == null ? MiuTheme.Dp(dp) : (int)System.Math.Round(dp * control.DeviceDpi / 96f);
}
