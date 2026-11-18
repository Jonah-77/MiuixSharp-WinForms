# Changelog

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
