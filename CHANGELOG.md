# Changelog

## 0.4.2

Platform pass: the library now behaves like a citizen of Windows, not just a screenshot.
No breaking API changes; two behaviours became automatic (see Notes).

**Accessibility**

- Buttons (`MiuButton`, `MiuIconButton`), `MiuCheckBox`, `MiuRadioButton`, `MiuSwitch` and
  `MiuSlider` are focusable and keyboard-operable: `Space` (and `Enter` on the button family)
  activates them, arrows / `Home` / `End` / `PageUp` / `PageDown` move the slider through its
  new `SmallChange` / `LargeChange` steps, and a focus rectangle is painted inside the rounded
  shape. `MiuWinButton` (derived from `Button`) paints its own focus cue too, because it skips
  the native `OnPaint`.
- Each of them overrides `CreateAccessibilityInstance()` with the right `AccessibleRole`
  (`PushButton` / `CheckButton` / `RadioButton` / `Slider` / `ProgressBar`), a `DefaultAction`
  and, for the check family, `Checked` / `Mixed` states; `MiuSlider`'s accessible object also
  exposes its numeric `Value`.
- `MiuProgressIndicator` no longer runs its animation timer at design time or while hidden
  (`DesignMode` / `Visible` guarded, restarted on `HandleCreated`).

**High contrast**

- `MiuTheme.RespectSystemHighContrast` (default `true`) and `MiuTheme.IsHighContrast`; while
  Windows is in a high-contrast theme, `MiuTheme.Colors` returns a `MiuColorScheme.HighContrast()`
  mapping of the whole role set onto `SystemColors`. `UserPreferenceChanged` /
  `DisplaySettingsChanged` re-apply it live.

**Designer**

- 22 controls now carry `[ToolboxBitmap]` (16×16, embedded from `src/MiuixSharp/Icons/`),
  `[Description]` and a `[DefaultEvent]` (buttons `Click`, check family `CheckedChanged`,
  slider `ValueChanged`).

**DPI**

- Every control with a constructor-fixed size now has `ApplyMetrics()` (`MiuField`, `MiuCard`,
  `MiuChip`, `MiuSwitch`, `MiuSlider`, `MiuProgressIndicator`): metrics are recomputed when the
  scale or theme changes, so `DpiChanged` or `MiuTheme.SetScale()` resizes the control instead
  of leaving it at its birth size. A size the caller set explicitly is preserved.
- GDI pooling: `MiuResources.Brush/Pen/RoundPen` and `MiuPathCache` are shared, cleared on theme
  or scale changes; the slider, switch and progress indicator now paint from the pool instead of
  allocating per frame.

**System integration**

- `MiuTheme.AccentSource` (`Seed` | `System`) with `MiuTheme.ReadSystemAccent()`.
- New `MiuBackdrop`: `Apply` / `Attach` set the themed caption bar, rounded window corners and
  an optional Mica / Acrylic system backdrop through `DwmSetWindowAttribute`.
- `MiuTheme.SetWarning` / `ClearWarning` and `SetSuccess` / `ClearSuccess`: product colours
  without forking the library. `Success` is also theme-aware now (lighter green on dark).

**Fixes**

- A 1 px stroke drawn on the same rounded path that a control clips itself to was half clipped,
  which shows up as a dashed border at 150 % (`MiuWinButton`, `MiuSurface`, `MiuButton`): the
  fill keeps the full path and the stroke is now inset by one pixel.
- `MiuScrollArea` re-measures automatically when children are added to or removed from its
  content (`ControlAdded` / `ControlRemoved`, with a re-entrancy guard around `RefreshLayout`);
  calling `RefreshLayout()` by hand still works.
- `MiuChip` had the last hard-coded pixel height (`26`), and its `Success` tone used a fixed
  light green that stayed bright on a dark surface.

**Notes**

- The package now builds for `net8.0-windows` **and** `net6.0-windows`, ships a `.snupkg` with
  SourceLink (`Microsoft.SourceLink.GitHub`), and includes the toolbox bitmaps as resources.
- `MiuScrollArea` and the `ApplyMetrics` behaviour are opt-out-by-override, not opt-in.

## 0.4.1

Code review pass over the whole library. No API changes; behaviour fixes only.

**Fixes**

- `MiuTheme.Changed` is a static event, and 16 controls subscribed with a one-line lambda in
  their constructor — the static delegate list kept every control alive after `Dispose()`,
  and a disposed control kept receiving repaints (`MiuButton`'s handler even set `Height`).
  Subscriptions now go through `MiuThemeHook.Attach`, which unhooks on `Disposed` and
  ignores callbacks for disposed controls.
- `MiuSlider` threw `ArgumentException` whenever `Minimum` was set above `Maximum`
  (`Math.Clamp` requires `min <= max`); `Value`/`Minimum`/`Maximum` and the paint/geometry
  math now clamp against the sorted pair. `Value` also stopped raising `ValueChanged`
  when the clamped value did not actually change (dragging past either end used to fire
  on every mouse move).
- `MiuSwitch.Checked` raised `CheckedChanged` even when the value was unchanged, which
  ping-pongs in two-way bindings.
- GDI leaks per paint/resize: `StringFormat` in `MiuButton`, `MiuChip`, `MiuIconButton`,
  `MiuWinButton`; a `GraphicsPath` in `MiuThumb`'s drop border; and three `UpdateRegion`
  implementations (`MiuSurface`, `MiuThumb`, `MiuLogList`) that assigned `Region` without
  disposing the previous one — they now use `RoundedPath.ApplyRegion`.
- `MiuThumb` and `MiuScrollArea` cached their colours in field initializers, so they kept
  the light palette after a Dark switch; both now resolve the theme role on read (`null`
  means "follow the theme") and repaint on theme changes. `MiuLogList` and `MiuThumb` also
  had no theme subscription at all and never repainted.
- `MiuCard.Shadow` tested `Mode == MiuMode.Light`, so `MiuMode.System` + a dark desktop still
  drew the light-theme shadow; it now resolves through `IsDark`.
- `MiuTheme.IsDark` hit the registry on every read, and `Colors` reads it on every colour
  lookup (i.e. every paint). The answer is now cached for 1 s and invalidated on `Mode`
  changes — still follows the user flipping the Windows setting.
- `MiuField`'s default height was the only hard-coded pixel metric left (`Height = 34`);
  it is dp now, so fields match the rest of the layout at high DPI.

## 0.4.0

First public release.

**DPI and scaling**

- `MiuTheme` gains `Scale`, `Dp()`, `Dpf()`, `ClampScale()`, `SetScale()`, `SetDpi()`.
  Every painted metric (radius, stroke, padding, control size) is now expressed in dp
  and converted at paint time.
- New `MiuDpi.Watch(form)` helper syncs the window's `DeviceDpi` into `MiuTheme.Scale`
  on `HandleCreated` and `DpiChanged`. `AutoScaleMode` is deliberately left alone to
  avoid double scaling.
- Scale range is clamped to **1.0 – 5.0**, matching the Windows custom-scaling range of
  100% – 500%. Out-of-range values are clamped, not thrown.
- Sub-pixel strokes keep a 0.75 px floor so the 0.75 dp divider never disappears.

**Fixes**

- Nine self-painted controls (button, icon button, progress, slider, chip, switch,
  checkbox, radio, divider) now support transparent backgrounds. The previous white
  blocks in dark mode came from WinForms painting a system background where Miuix
  specifies `Color.Unspecified` (no background).
- `MiuTitleBar.Apply` resolves `MiuMode.System` through `MiuTheme.IsDark`; the showcase
  now calls it on handle creation, so the title bar follows the active theme.
- `MiuLogList.SurfaceColor` no longer caches a colour at construction and follows the
  theme like every other control.

**Showcase**

- Runs as `PerMonitorV2`; window size, minimum size and every layout coordinate are
  dp-based, so 150% displays no longer squeeze labels.
- New `--scale <x>` argument for manual scale overrides.
- Live theme toggle, and a single-file feature page at `docs/index.html`
  (published on GitHub Pages).

**Docs**

- README: new "DPI and scaling" section with the reasoning behind the range.
- Handover doc: new sections 4.4 (DPI and scaling), 8.13 (white blocks in dark mode),
  8.14 (containers that did not follow a scaled control).
- Both screenshots reshot at the 1:1 (96 dpi) baseline.

**Packaging**

- Solution file added — `dotnet build -c Release` now works from the repository root.
- NuGet metadata (project URL, repository, tags, packaged README) and CI build + pack.

## 0.3.0

Internal milestone: 24 controls across three families, 51 colour roles, 14 text styles.
Never published.
