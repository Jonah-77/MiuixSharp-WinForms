using System.Windows.Forms;

namespace MiuixSharp.Theme;

/// <summary>
/// Hooks a control to <see cref="MiuTheme.Changed"/> and unhooks it when the control is
/// disposed. <see cref="MiuTheme.Changed"/> is a <em>static</em> event: a plain
/// <c>MiuTheme.Changed += (_, __) =&gt; Invalidate();</c> in a constructor keeps the control
/// alive forever (the static delegate list is a GC root) and keeps invoking a disposed
/// control. Attach() closes both holes — the handler is removed on dispose, so the static
/// list holds nothing once the control goes away.
/// </summary>
public static class MiuThemeHook
{
    /// <summary>Subscribes <paramref name="onChanged"/> for the lifetime of <paramref name="control"/>.</summary>
    public static void Attach(Control control, Action onChanged)
    {
        if (control == null || onChanged == null) return;

        EventHandler handler = (_, __) =>
        {
            if (control.IsDisposed) return;
            onChanged();
        };

        MiuTheme.Changed += handler;
        control.Disposed += (_, __) => MiuTheme.Changed -= handler;
    }
}
