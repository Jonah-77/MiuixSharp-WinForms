using System.Drawing;

namespace MiuixSharp.Theme;

/// <summary>
/// A faithful C# port of Miuix's named color-role system.
/// Values mirror the official non-Monet light/dark scheme documented at
/// docs/guide/colors.md of compose-miuix-ui/miuix (ARGB alpha omitted).
/// </summary>
public sealed class MiuColorScheme
{
    // Primary
    public Color Primary, OnPrimary, PrimaryVariant, OnPrimaryVariant;
    public Color PrimaryContainer, OnPrimaryContainer;
    public Color TertiaryContainer, OnTertiaryContainer, TertiaryContainerVariant, OnTertiaryContainerVariant;

    // Error
    public Color Error, OnError, ErrorContainer, OnErrorContainer;

    // Secondary (neutral surfaces)
    public Color Secondary, OnSecondary, SecondaryVariant, OnSecondaryVariant;
    public Color SecondaryContainer, OnSecondaryContainer, SecondaryContainerVariant, OnSecondaryContainerVariant;

    // Disabled family
    public Color DisabledPrimary, DisabledOnPrimary;
    public Color DisabledPrimaryButton, DisabledOnPrimaryButton;
    public Color DisabledSecondary, DisabledOnSecondary;
    public Color DisabledOnSurface;

    // Background & surface
    public Color Background, OnBackground, OnBackgroundVariant;
    public Color Surface, OnSurface, OnSurfaceSecondary, OnSurfaceVariantSummary, OnSurfaceVariantActions;
    public Color SurfaceVariant, SurfaceContainer, OnSurfaceContainer, OnSurfaceContainerVariant;
    public Color SurfaceContainerHigh, OnSurfaceContainerHigh, SurfaceContainerHighest, OnSurfaceContainerHighest;

    // Lines
    public Color Outline, DividerLine, WindowDimming;

    // Slider
    public Color SliderKeyPoint, SliderKeyPointForeground, SliderBackground;

    /// <summary>Official Miuix light color scheme (non-Monet).</summary>
    public static MiuColorScheme Light() => new()
    {
        Primary = C(0x3482FF), OnPrimary = C(0xFFFFFF), PrimaryVariant = C(0x3482FF), OnPrimaryVariant = C(0xAECDFF),
        PrimaryContainer = C(0x5D9BFF), OnPrimaryContainer = C(0xFFFFFF),
        TertiaryContainer = C(0xEAF2FF), OnTertiaryContainer = C(0x3482FF),
        TertiaryContainerVariant = C(0xEAF2FF), OnTertiaryContainerVariant = C(0x3482FF),
        Error = C(0xE94634), OnError = C(0xFFFFFF), ErrorContainer = C(0xFDF6F4), OnErrorContainer = C(0x410002),
        Secondary = C(0xE6E6E6), OnSecondary = C(0xFFFFFF), SecondaryVariant = C(0xF0F0F0), OnSecondaryVariant = C(0x303030),
        SecondaryContainer = C(0xF0F0F0), OnSecondaryContainer = C(0xA9A9A9),
        SecondaryContainerVariant = C(0xF0F0F0), OnSecondaryContainerVariant = C(0xA8A8A8),
        DisabledPrimary = C(0xC2D9FF), DisabledOnPrimary = C(0xF3F8FF),
        DisabledPrimaryButton = C(0xC2D9FF), DisabledOnPrimaryButton = C(0xFFFFFF),
        DisabledSecondary = C(0xF0F0F0), DisabledOnSecondary = C(0xFCFCFC), DisabledOnSurface = C(0xB2B2B2),
        Background = C(0xFFFFFF), OnBackground = C(0x000000), OnBackgroundVariant = C(0x8C93B0),
        Surface = C(0xF7F7F7), OnSurface = C(0x000000), OnSurfaceSecondary = C(0x000000, 0xCC),
        OnSurfaceVariantSummary = C(0x000000, 0x99), OnSurfaceVariantActions = C(0x000000, 0x66),
        SurfaceVariant = C(0xFFFFFF), SurfaceContainer = C(0xFFFFFF), OnSurfaceContainer = C(0x000000), OnSurfaceContainerVariant = C(0x959595),
        SurfaceContainerHigh = C(0xE8E8E8), OnSurfaceContainerHigh = C(0xA2A2A2),
        SurfaceContainerHighest = C(0xE8E8E8), OnSurfaceContainerHighest = C(0x000000),
        Outline = C(0xD9D9D9), DividerLine = C(0xE0E0E0), WindowDimming = C(0x000000, 0x4D),
        SliderKeyPoint = C(0xA3B3CD, 0x4D), SliderKeyPointForeground = C(0x6EB5FF), SliderBackground = C(0x000000, 0x0F),
    };

    /// <summary>Official Miuix dark color scheme (non-Monet).</summary>
    public static MiuColorScheme Dark() => new()
    {
        Primary = C(0x277AF7), OnPrimary = C(0xFFFFFF), PrimaryVariant = C(0x0073DD), OnPrimaryVariant = C(0x99C7F1),
        PrimaryContainer = C(0x338FE4), OnPrimaryContainer = C(0xFFFFFF),
        TertiaryContainer = C(0x2B3B54), OnTertiaryContainer = C(0x4788FF),
        TertiaryContainerVariant = C(0x505050), OnTertiaryContainerVariant = C(0x4788FF),
        Error = C(0xF12522), OnError = C(0x690005), ErrorContainer = C(0x2E0603), OnErrorContainer = C(0xFFDAD6),
        Secondary = C(0x505050), OnSecondary = C(0xFFFFFF), SecondaryVariant = C(0x434343), OnSecondaryVariant = C(0xD9D9D9),
        SecondaryContainer = C(0x434343), OnSecondaryContainer = C(0x7C7C7C),
        SecondaryContainerVariant = C(0x4F4F4F), OnSecondaryContainerVariant = C(0x959595),
        DisabledPrimary = C(0x253E64), DisabledOnPrimary = C(0x677993),
        DisabledPrimaryButton = C(0x253E64), DisabledOnPrimaryButton = C(0x677893),
        DisabledSecondary = C(0x3F3F3F), DisabledOnSecondary = C(0x797979), DisabledOnSurface = C(0x666666),
        Background = C(0x242424), OnBackground = C(0xFFFFFF, 0xE6), OnBackgroundVariant = C(0x787E96),
        Surface = C(0x000000), OnSurface = C(0xF2F2F2), OnSurfaceSecondary = C(0xFFFFFF, 0xCC),
        OnSurfaceVariantSummary = C(0xFFFFFF, 0x80), OnSurfaceVariantActions = C(0xFFFFFF, 0x66),
        SurfaceVariant = C(0x242424), SurfaceContainer = C(0x242424), OnSurfaceContainer = C(0xFFFFFF, 0xE6), OnSurfaceContainerVariant = C(0x737373),
        SurfaceContainerHigh = C(0x242424), OnSurfaceContainerHigh = C(0x666666),
        SurfaceContainerHighest = C(0x2D2D2D), OnSurfaceContainerHighest = C(0xE9E9E9),
        Outline = C(0x404040), DividerLine = C(0x393939), WindowDimming = C(0x000000, 0x99),
        SliderKeyPoint = C(0x7A8AA6, 0x4D), SliderKeyPointForeground = C(0x5DAAFF), SliderBackground = C(0xFFFFFF, 0x26),
    };

    /// <summary>
    /// High-contrast scheme: when Windows high-contrast is active every role falls back to a
    /// system colour. Self-painted controls that keep their own palette become unreadable in
    /// that mode, so the whole scheme is remapped instead (Windows accessibility guidance).
    /// </summary>
    public static MiuColorScheme HighContrast() => new()
    {
        Primary = SystemColors.Highlight, OnPrimary = SystemColors.HighlightText,
        PrimaryVariant = SystemColors.Highlight, OnPrimaryVariant = SystemColors.HighlightText,
        PrimaryContainer = SystemColors.Highlight, OnPrimaryContainer = SystemColors.HighlightText,
        TertiaryContainer = SystemColors.Window, OnTertiaryContainer = SystemColors.Highlight,
        TertiaryContainerVariant = SystemColors.Window, OnTertiaryContainerVariant = SystemColors.Highlight,
        Error = SystemColors.HotTrack, OnError = SystemColors.Window,
        ErrorContainer = SystemColors.Window, OnErrorContainer = SystemColors.WindowText,
        Secondary = SystemColors.Control, OnSecondary = SystemColors.WindowText,
        SecondaryVariant = SystemColors.Control, OnSecondaryVariant = SystemColors.WindowText,
        SecondaryContainer = SystemColors.Control, OnSecondaryContainer = SystemColors.WindowText,
        SecondaryContainerVariant = SystemColors.Control, OnSecondaryContainerVariant = SystemColors.WindowText,
        DisabledPrimary = SystemColors.Control, DisabledOnPrimary = SystemColors.GrayText,
        DisabledPrimaryButton = SystemColors.Control, DisabledOnPrimaryButton = SystemColors.GrayText,
        DisabledSecondary = SystemColors.Control, DisabledOnSecondary = SystemColors.GrayText,
        DisabledOnSurface = SystemColors.GrayText,
        Background = SystemColors.Window, OnBackground = SystemColors.WindowText, OnBackgroundVariant = SystemColors.WindowText,
        Surface = SystemColors.Window, OnSurface = SystemColors.WindowText, OnSurfaceSecondary = SystemColors.WindowText,
        OnSurfaceVariantSummary = SystemColors.WindowText, OnSurfaceVariantActions = SystemColors.GrayText,
        SurfaceVariant = SystemColors.Window, SurfaceContainer = SystemColors.Window,
        OnSurfaceContainer = SystemColors.WindowText, OnSurfaceContainerVariant = SystemColors.WindowText,
        SurfaceContainerHigh = SystemColors.Control, OnSurfaceContainerHigh = SystemColors.WindowText,
        SurfaceContainerHighest = SystemColors.Control, OnSurfaceContainerHighest = SystemColors.WindowText,
        Outline = SystemColors.WindowText, DividerLine = SystemColors.WindowText, WindowDimming = Color.Black,
        SliderKeyPoint = SystemColors.GrayText, SliderKeyPointForeground = SystemColors.Highlight,
        SliderBackground = SystemColors.Control,
    };

    private static Color C(int rgb, int alpha = 0xFF) =>
        Color.FromArgb(alpha, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
}
