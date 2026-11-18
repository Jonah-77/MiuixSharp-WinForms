using System.Drawing;

namespace MiuixSharp.Theme;

/// <summary>Whether to use the light, dark, or system-following scheme.</summary>
public enum MiuMode { Light, Dark, System }

/// <summary>Palette styles for seed-derived ("Monet") colors — ported from Miuix.</summary>
public enum MiuPaletteStyle { TonalSpot, Neutral, Vibrant, Expressive, Monochrome }

/// <summary>
/// Global design tokens — a C# port of MiuixTheme.
/// Default colors are the exact official non-Monet Miuix light/dark schemes.
/// Set <see cref="Seed"/> to switch to seed-derived (Monet-style) colors.
/// </summary>
public static class MiuTheme
{
    private static MiuMode _mode = MiuMode.Light;
    private static MiuPaletteStyle _style = MiuPaletteStyle.TonalSpot;
    private static MiuColorScheme _light = MiuColorScheme.Light();
    private static MiuColorScheme _dark = MiuColorScheme.Dark();
    private static Color _seed = Color.FromArgb(0x34, 0x82, 0xFF);
    private static TonalPalette _palette = new(_seed);
    private static bool _monet;

    private static readonly Font _font = new("Microsoft YaHei", 9F);
    private static readonly Font _fontSm = new("Microsoft YaHei", 8.25F);
    private static readonly Font _fontLg = new("Microsoft YaHei", 12F, FontStyle.Bold);

    /// <summary>Raised whenever the theme changes so controls can repaint.</summary>
    public static event EventHandler Changed;

    public static MiuMode Mode
    {
        get => _mode;
        set { _mode = value; _darkCacheAt = long.MinValue; Changed?.Invoke(null, EventArgs.Empty); }
    }

    // Resolving System mode reads the registry, and Colors is consulted from every paint
    // path. Cache the answer briefly instead of hitting HKCU on each repaint; the 1 s window
    // still follows the user flipping the Windows setting.
    private static bool _darkCache;
    private static long _darkCacheAt = long.MinValue;

    /// <summary>True when dark colors are active (resolves System via Windows setting).</summary>
    public static bool IsDark
    {
        get
        {
            if (_mode == MiuMode.Dark) return true;
            if (_mode == MiuMode.Light) return false;

            long now = Environment.TickCount64;
            if (_darkCacheAt != long.MinValue && now - _darkCacheAt < 1000) return _darkCache;

            _darkCache = ReadSystemDark();
            _darkCacheAt = now;
            return _darkCache;
        }
    }

    private static bool ReadSystemDark()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var v = key?.GetValue("AppsUseLightTheme");
            return v is int i && i == 0;
        }
        catch { return false; }
    }

    /// <summary>Active color scheme (respects current resolved mode).</summary>
    public static MiuColorScheme Colors => IsDark ? _dark : _light;

    public static MiuPaletteStyle PaletteStyle
    {
        get => _style;
        set { _style = value; if (_monet) RebuildMonet(); }
    }

    /// <summary>Setting a seed switches on seed-derived (Monet-style) colors.</summary>
    public static Color Seed
    {
        get => _seed;
        set { _seed = value; _monet = true; RebuildMonet(); }
    }

    /// <summary>Use the exact official Miuix color scheme (no seed derivation).</summary>
    public static void UseOfficialScheme()
    {
        _monet = false;
        _light = MiuColorScheme.Light();
        _dark = MiuColorScheme.Dark();
        Changed?.Invoke(null, EventArgs.Empty);
    }

    private static void RebuildMonet()
    {
        _palette = new TonalPalette(_seed, _style);
        var l = MiuColorScheme.Light();
        var d = MiuColorScheme.Dark();
        l.Primary = _palette.Tone(40); l.OnPrimary = Color.White;
        l.PrimaryContainer = _palette.Tone(90); l.OnPrimaryContainer = _palette.Tone(10);
        l.TertiaryContainer = _palette.Tone(95); l.OnTertiaryContainer = _palette.Tone(40);
        d.Primary = _palette.Tone(80); d.OnPrimary = _palette.Tone(10);
        d.PrimaryContainer = _palette.Tone(30); d.OnPrimaryContainer = _palette.Tone(90);
        d.TertiaryContainer = _palette.Tone(25); d.OnTertiaryContainer = _palette.Tone(80);
        _light = l; _dark = d;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    public static TonalPalette Palette => _palette;

    // ---- Semantic aliases used by the controls ----
    public static Color Primary => Colors.Primary;
    public static Color OnPrimary => Colors.OnPrimary;
    public static Color PrimaryContainer => Colors.PrimaryContainer;
    public static Color OnPrimaryContainer => Colors.OnPrimaryContainer;
    public static Color Background => Colors.Background;
    public static Color Surface => Colors.Surface;
    public static Color SurfaceAlt => Colors.SurfaceContainerHigh;
    public static Color Text => Colors.OnSurface;
    public static Color TextSecondary => Colors.OnSurfaceVariantSummary;
    public static Color Outline => Colors.Outline;

    /// <summary>Miuix error color (rendered as the restrained "warning" accent).</summary>
    public static Color Warning => Colors.Error;
    public static Color WarningContainer => Colors.ErrorContainer;
    public static Color Success => Color.FromArgb(0x1E, 0x7A, 0x4B);

    // ---- Type ----
    public static Font Font => _font;
    public static Font FontSmall => _fontSm;
    public static Font FontTitle => _fontLg;

    // ---- Scale (DPI) ----
    // Miuix metrics are dp. WinForms draws in device pixels, so every hard-coded
    // metric below is a *dp value* and must go through Dp() at the point of use —
    // never cache the converted pixel value in a field (see 交接文档 8.11/8.13).
    //
    // The scale is bounded on purpose, and the bounds follow the platform rather than taste:
    // Windows' own display scaling only offers 100%–500% (custom scaling below 100% is not
    // possible at all), and MiuixSharp text is specified in pt, so a factor under 1.0 would
    // shrink the boxes while leaving the glyphs the same size and clip every label.
    public const float MinScale = 1.0f;   // 96 dpi  (100%)  — Windows' floor
    public const float MaxScale = 5.0f;   // 480 dpi (500%)  — Windows' ceiling

    private static float _scale = 1f;

    /// <summary>Current UI scale: device DPI / 96. 1.0 at 100%, 1.5 at 150%.</summary>
    public static float Scale => _scale;

    /// <summary>Clamps any requested scale into the supported range.</summary>
    public static float ClampScale(float scale) => Math.Clamp(scale, MinScale, MaxScale);

    /// <summary>
    /// Converts a dp metric to device pixels. Rounds up to at least 1px so hairlines
    /// (0.75dp divider, 1dp border) never disappear at 100%.
    /// </summary>
    public static int Dp(float dp) =>
        dp <= 0 ? 0 : Math.Max(1, (int)Math.Round(dp * _scale));

    /// <summary>Same as <see cref="Dp(float)"/> but keeps sub-pixel precision for pens.</summary>
    public static float Dpf(float dp) => Math.Max(0.75f, dp * _scale);

    /// <summary>
    /// Sets the scale explicitly (app-level zoom). Values outside
    /// <see cref="MinScale"/>..<see cref="MaxScale"/> are clamped, never rejected.
    /// </summary>
    public static void SetScale(float scale)
    {
        var s = ClampScale(scale);
        if (Math.Abs(s - _scale) < 0.0001f) return;
        _scale = s;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Sets the scale from a window's device DPI and raises <see cref="Changed"/> so every
    /// live control re-measures. Called by <see cref="MiuDpi"/>; call it yourself if you
    /// host MiuixSharp controls outside a Form.
    /// </summary>
    public static void SetDpi(int deviceDpi)
    {
        SetScale(deviceDpi > 0 ? deviceDpi / 96f : 1f);
    }

    // ---- Metrics — exact Miuix defaults, expressed in dp (≈ px @ 96dpi) ----
    public static int RadiusSm => 12;
    public static int RadiusMd => 16;   // ButtonDefaults.CornerRadius = 16dp
    public static int RadiusLg => 16;   // CardDefaults.CornerRadius = 16dp
    public static int ButtonMinHeight => 40; // ButtonDefaults.MinHeight = 40dp
    public static int Space => 12;

    public static void Refresh() => Changed?.Invoke(null, EventArgs.Empty);
}
