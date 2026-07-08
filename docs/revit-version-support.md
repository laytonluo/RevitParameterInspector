# Revit Version Support

## Supported versions

| Revit Version | Target Framework | How to select it |
|---|---|---|
| 2024 | `net48` | default for this TFM |
| 2025 | `net8.0-windows` | default for this TFM |
| 2026 | `net8.0-windows` | `-p:RevitVersion=2026` |

See [build-guide.md](build-guide.md) for the exact build commands and how `RevitInstallDir`
resolution works.

## Why the split

- **Revit 2024** introduced 64-bit `ElementId` support; `ElementId.IntegerValue` became
  obsolete in favor of `ElementId.Value` (a `long`). This project's minimum supported version
  is 2024, so `Core` models already store ids as `long` everywhere and there is no
  `IntegerValue`/`Value` branching to do.
- **Revit 2025** moved the Revit API itself onto .NET 8, so 2025+ add-ins must be built as
  `net8.0-windows`, not `net48`. 2024 remains on `net48`/.NET Framework.
- **Revit 2026** also targets `net8.0-windows`; the `RevitVersion` MSBuild property
  distinguishes it from 2025 for the handful of cases where API behavior differs between the
  two.

## Where version differences live

All Revit-version-specific code should go in
`src/RevitParameterInspector.Revit/Compatibility/RevitCompatibility.cs`, gated by the
`REVIT2024` / `REVIT2025` / `REVIT2026` / `REVIT2024_OR_GREATER` / `REVIT2025_OR_GREATER`
define constants the `.csproj` sets based on the resolved version (see build-guide.md's table).
Builders and readers elsewhere should call into `RevitCompatibility` rather than `#if` on Revit
version themselves - today it's just `RevitCompatibility.GetIdValue(ElementId)`, since 2024+
already has 64-bit ids uniformly, but this is the seam for anything that diverges between 2025
and 2026 in the future.

## Current verification status

None of `RevitParameterInspector.Revit` (external command, builders, readers) has been
exercised inside an actual running Revit instance in any of 2024/2025/2026 yet - see
[build-guide.md](build-guide.md#tests) and the README's implementation status. The Revit API
references (`RevitAPI.dll`/`RevitAPIUI.dll`) resolve correctly against a real install via
`RevitInstallDir`, but end-to-end behavior inside Revit itself is unverified. If you have a
Revit install and can test this, contributions confirming (or fixing) behavior on a specific
version are very welcome.
