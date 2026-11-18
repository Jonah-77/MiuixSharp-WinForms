# Credits / 致谢

## Design language borrowed from Miuix

This project's **visual design language** is derived from
[Miuix](https://github.com/compose-miuix-ui/miuix), an open-source design system and component
library created by [yukonga](https://github.com/yukonga) and the Miuix contributors, inspired by
Xiaomi's HyperOS / MIUI aesthetics.

Specifically, MiuixSharp re-implements in C# the following ideas documented in the upstream
project:

- The named color-role system and the official light/dark color values
  (`docs/guide/colors.md`) — ported value-for-value.
- The theme mode system: Light / Dark / System and Monet dynamic colors with palette styles
  (`docs/guide/theme.md`).
- The 14 text styles (`docs/guide/textstyles.md`).
- Component default metrics: 16dp rounded corners, 40dp button height, 28dp slider,
  0.75dp divider, 3.855dp slider key points (`docs/components/basic/*`).

Miuix is licensed under the **Apache License 2.0**. Copyright of the original design and code
belongs to the Miuix authors.

## What this project is NOT

- It is **not** a port of Miuix source code. Miuix is written in Kotlin / Jetpack Compose and
  cannot run on .NET; every line here was written independently in C# for WinForms.
- It is **not** affiliated with, endorsed by, sponsored by, or officially connected to the
  Miuix project, its authors, or Xiaomi.
- The name "MiuixSharp" indicates "a Miuix-inspired library for C#", not an official product.

## Trademarks

"Miuix", "Xiaomi", "HyperOS", and "MIUI" are trademarks of their respective owners. They are
mentioned here only to attribute the design inspiration; no endorsement is implied.

## Other

- Fonts: Microsoft YaHei (ships with Windows) is used as the default UI font.
- No third-party C# packages are required beyond the .NET 8 Windows Desktop workload.

If you are the owner of Miuix and have any concerns about this attribution, please open an issue —
I will adjust the naming or credits accordingly.
