# Getting Started

RevitParameterInspector is a read-only Revit add-in that builds a structured
`ElementContextSnapshot` for a selected element and displays it in a WPF window (Summary,
Parameters, Geometry, Location, Relationships, View/Sheet Context, Dictionary, Raw JSON, AI
Context), then lets you export it as JSON, Markdown, or Excel, or copy a concise AI-readable
summary to the clipboard.

See the [README](../README.md) for the project's positioning and current implementation
status, and `HANDOFF_RevitParameterInspector_V1_Full.md` for the full V1 specification.

## Prerequisites

- .NET SDK 8.0+ (also used to build the `net48` targets via multi-targeting)
- Visual Studio 2022 (17.8+) or the `dotnet` CLI
- One of Revit 2024, 2025, or 2026 installed, for `RevitAPI.dll`/`RevitAPIUI.dll` and for
  actually running the add-in

## Building

The solution multi-targets so a single checkout can build for any supported Revit version:

| Target Framework | Revit Version |
|---|---|
| `net48` | 2024 |
| `net8.0-windows` | 2025 (default) |
| `net8.0-windows` with `-p:RevitVersion=2026` | 2026 |

```bash
# Revit 2024 (net48)
dotnet build src/RevitParameterInspector.Revit -f net48

# Revit 2025 (net8.0-windows, default)
dotnet build src/RevitParameterInspector.Revit -f net8.0-windows

# Revit 2026 (net8.0-windows)
dotnet build src/RevitParameterInspector.Revit -f net8.0-windows -p:RevitVersion=2026
```

By default the build looks for Revit at `C:\Program Files\Autodesk\Revit <version>`. If your
installation lives elsewhere, override it:

```bash
dotnet build src/RevitParameterInspector.Revit -f net8.0-windows -p:RevitInstallDir="D:\Autodesk\Revit 2025"
```

The build output (`RevitParameterInspector.Revit.dll` plus its `Core`/`UI`/`Export`/
`Dictionary` dependencies and a copy of the `dictionary/` folder) lands in
`src/RevitParameterInspector.Revit/bin/<Debug|Release>/<TargetFramework>/`.

Alternatively, open `RevitParameterInspector.sln` in Visual Studio and build there.

## Loading it into Revit

`install/addin/` and `install/bundle/` have ready-to-use manifests - see
[install/README.md](../install/README.md) for the two options (a per-version manifest you copy
next to the build output, or a single machine-wide bundle covering every installed version).
The short version:

1. Build the project for your Revit version (see above).
2. Copy the matching `install/addin/RevitParameterInspector<version>.addin` next to the build
   output DLLs in `%APPDATA%\Autodesk\Revit\Addins\<version>\` - or run
   `install/bundle/build-bundle.ps1` and copy the resulting `.bundle` folder to
   `%ProgramData%\Autodesk\ApplicationPlugins\` instead.
3. Start Revit. The command becomes available under **Add-Ins > External Tools**. There is no
   dedicated ribbon panel yet (also an open item in the README's status list).

## Using it

1. Select a single element in the Revit model (or run the command with nothing selected - it
   will prompt you to pick one; picking one of several selected elements isn't implemented
   yet, so the first of a multi-selection is used).
2. Run **RevitParameterInspector** from Add-Ins > External Tools.
3. The Inspector window opens with the built snapshot across its tabs. Use **Export JSON /
   Export Markdown / Export Excel** or **Copy AI Context** at the bottom to get the data out.

See `samples/json`, `samples/markdown`, and `samples/excel` for example output from each
format, and `dictionary/zh-TW` for the (still partial) built-in terminology dictionary - see
`CONTRIBUTING.md` if you'd like to help fill it in.
