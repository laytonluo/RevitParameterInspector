# Installing RevitParameterInspector

There are two ways to register the add-in with Revit. Both need the project built first - see
[docs/build-guide.md](../docs/build-guide.md).

## Option 1: `addin/` - per-user, per-version manifests

`install/addin/` has one ready-to-use `.addin` manifest per supported version
(`RevitParameterInspector2024.addin`, `...2025.addin`, `...2026.addin`). Each one points at
`.\RevitParameterInspector.Revit.dll` - a path relative to the manifest's own location - so:

1. Build the project for your Revit version (see the build guide).
2. Copy that version's `.addin` file **and** the entire build output folder
   (`src/RevitParameterInspector.Revit/bin/<Debug|Release>/<TargetFramework>/` - the `.dll`
   files plus the `dictionary/` folder) into
   `%APPDATA%\Autodesk\Revit\Addins\<version>\`, so the `.addin` file and
   `RevitParameterInspector.Revit.dll` sit side by side.
3. Start Revit. The command is available under **Add-Ins > External Tools**.

This is per-user (Roaming `%APPDATA%`) and per-Revit-version - if you use multiple Revit
versions, repeat this for each one.

## Option 2: `bundle/` - a single machine-wide bundle for all versions

`install/bundle/RevitParameterInspector.bundle/` follows Autodesk's multi-version "bundle"
convention (the same layout used by many open-source Revit add-ins): a `PackageContents.xml`
at the bundle root declares which Revit series map to which `.addin` manifest under
`Contents/<version>/`. Revit auto-discovers any `*.bundle` folder placed under
`%ProgramData%\Autodesk\ApplicationPlugins\`, covering every installed Revit version from one
copy.

The `.addin` manifests under `Contents/2024|2025|2026/` are already committed; what's missing
until you build is the actual `RevitParameterInspector.Revit.dll` (and its dependencies +
`dictionary/` folder) alongside each one. `build-bundle.ps1` does that for you:

```powershell
# Build and package all three versions (needs all three installed)
.\install\bundle\build-bundle.ps1

# Or just the ones you have installed
.\install\bundle\build-bundle.ps1 -Versions 2025,2026 -Configuration Release
```

Then copy the whole `RevitParameterInspector.bundle` folder to
`%ProgramData%\Autodesk\ApplicationPlugins\` and start Revit.

The DLLs/`.pdb`/`dictionary/` files the script copies into `Contents/<version>/` are
build output, not source - they're excluded via `.gitignore` (only the `.addin` manifests and
`PackageContents.xml` are tracked).

## Which one should I use?

- Testing a single version quickly, or only ever using one Revit version: `addin/`.
- Distributing to others, or running multiple Revit versions on the same machine: `bundle/`.

Neither is wired into a CI/release pipeline yet - see `docs/roadmap.md`.
