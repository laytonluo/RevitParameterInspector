# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

## [0.3.0] - 2026-08-22

### Added

- Ribbon panel: a "ParameterInspector" panel now sits on the Add-Ins tab (next to any other
  installed add-in's panel), with two independent large buttons - **Inspector** and **IDs
  List Ex**, both with their own icon - `RevitParameterInspectorApplication`
  (`IExternalApplication`) registers it in `OnStartup`. The old Add-Ins > External Tools
  command entries are gone. (`PushButtonData` requires a non-empty `Text`; the icon-only
  buttons originally planned to match the icons' own embedded text were not possible - see
  Fixed below.)
- Merged inspect button: `InspectSelectionOrActiveViewCommand` replaces the previous two
  separate commands (`InspectActiveViewCommand` + `PickElementCommand`) with one - it inspects
  the current selection when something is selected, otherwise the active view, reusing
  `SelectionReader.GetFirstSelectedOrActiveView()` (the same logic already used by Reselect).
  The explicit "always prompt to pick" command is removed; there is no ribbon-facing
  replacement for it.
- "Selection Ids List Ex": a new lightweight panel (`SelectionIdsListCommand` +
  `SelectionIdsListWindow`) showing the active view id and, when something is selected, the
  selected element ids grouped by category. Every button both copies (where applicable) and
  closes the panel. Two states:
  - Nothing selected: view id only, with **複製視圖ID** (copies the view id, then closes) /
    **關閉**.
  - Something selected: view id + categorized selection ids, with **複製ID** (ids only,
    comma-separated, no labels), **複製訊息** (the full displayed text as-is), **複製訊息(OST)**
    (an English-labeled, `OST_`-category version of the same text, copy-only - the displayed
    panel itself stays in Revit's UI language), and **關閉**.
  - New `SelectionIdsSnapshot`/`SelectionCategoryGroup` (Core), `SelectionIdsSnapshotBuilder`
    (Revit, groups by the stable `BuiltInCategory` name so same-labeled categories in
    different disciplines never merge), and `SelectionIdsListViewModel` (UI).
- About Me tab now shows the running add-in's version (`MainWindowViewModel.VersionText`, read
  from the assembly's informational version - set from `Directory.Build.props`'s
  `RevitParameterInspectorVersion`, with the SDK's auto-appended `+<git-sha>` build metadata
  stripped - so the tab never needs a manual update on the next version bump).

### Changed

- Add-in version bumped to 0.3.0 (`Directory.Build.props`).
- Multi-selection is now resolved deterministically: `SelectionReader.GetFirstSelectedOrActiveView`
  sorts the current selection by `ElementId` ascending and takes the lowest, instead of
  whatever order `Selection.GetElementIds()` happened to return (click/selection order). Applies
  to both the RevitParameterInspector button and the Reselect button (shared logic).
- `RevitParameterInspector.Revit` now sets `UseWPF=true` (needed for `PushButtonData.LargeImage`
  / `BitmapImage`) and ships `ICON/*.png` next to the built assembly under an `icons/` folder,
  the same convention already used for the `dictionary/` folder. `install/bundle/build-bundle.ps1`
  copies `icons/` into each bundled version alongside `dictionary/`.
- `.addin` manifests (`install/addin/`, `install/bundle/.../Contents/<version>/`) now register a
  single `Type="Application"` entry (`RevitParameterInspectorApplication`) instead of two
  `Type="Command"` entries.

### Fixed

- Both ribbon buttons originally shipped with an empty `Text` (icon-only, matching the source
  icon artwork which already has the label baked in). `PushButtonData`'s constructor rejects an
  empty `Text` (`Autodesk.Revit.Exceptions.ArgumentException: The value cannot be empty.
  Parameter name: Text`) - confirmed on Revit 2026, and this validation is not new to 2026, so
  it would have failed identically on every supported version. Buttons now have short Text
  ("Inspector" / "IDs List Ex").
- `RevitParameterInspectorApplication.OnStartup` wrapped both `RibbonPanel.AddItem` calls in a
  single `try`/`catch`: if building the first button's `PushButtonData` (icon decoding, in
  particular) threw for any reason, the panel ended up with *neither* button and no visible
  error - reported as "the panel shows up but both buttons and icons are missing" on Revit
  2026. Each button is now added independently, and any failure now shows a `TaskDialog` with
  the actual exception instead of failing silently. Icon loading also now decodes from bytes
  (`File.ReadAllBytes` + `MemoryStream`) instead of `BitmapImage.UriSource`, avoiding a class of
  file-handle/URI-timing issues at Revit's early startup.
- Excel export (`ClosedXML`/`DocumentFormat.OpenXml`) was silently missing from the
  net8.0-windows build output (Revit 2025/2026), breaking **Export Excel** at runtime on those
  versions - only the net48 (Revit 2024) build carried them, because .NET Framework always
  copies package-reference assemblies next to the output while a .NET (non-net48) class
  library does not by default. Fixed by setting `CopyLocalLockFileAssemblies=true` on
  `RevitParameterInspector.Revit.csproj`.
- `SelectionIdsListWindow` had a redundant "Selection Ids List Extra" text box duplicating the
  window's own title bar - removed. The window is also no longer a fixed 480px wide
  (`SizeToContent="WidthAndHeight"` instead), since the ELEMENT-state button row (four buttons,
  long Chinese captions) silently overflowed past the fixed width and clipped the last
  (**關閉**) button off-screen entirely - reported as "there's no close button" when it was
  actually just invisible. Button captions in that row were also shortened (複製ID / 複製訊息 /
  複製訊息(OST) / 關閉). `MaxWidth="560"` was then added so a long multi-category id list wraps
  instead of stretching the window arbitrarily wide.

## [0.2.0] - 2026-07-16

### Added

- "Research by ID" hyperlinks: ElementId-valued cells now render as blue underlined links,
  and double-clicking one selects that element in Revit and re-inspects it in place
  (`IReselectRequestHandler.RequestInspectById`, routed through the existing Reselect
  ExternalEvent). Applies to the Relationships tab (fields whose name ends in `Id`/`Ids`;
  `UniqueId` is excluded, and list values like `MaterialIds` become one link per id) and to
  the View / Sheet Context tab's ElementId column. Invalid ids (empty, `-1`) stay plain text.
- View / Sheet Context: project-wide visibility scan for regular model elements - one row per
  non-template `View3D`/`ViewPlan`/`ViewSection` (elevations are `ViewSection`s, so they are
  covered) whose collector contains the inspected element. Each view row's `Additional Info`
  now shows its sheet placement (`Viewport: <name> (ID <id>) | Sheet: <number> - <name>
  (ID <id>)`), or `Viewport: N/A | Sheet: N/A` when the view is not placed on any sheet;
  placed sheets also get their own Sheet row. The existing direct-relationship behavior for
  inspected Views/Sheets/Viewports is unchanged.

### Changed

- Add-in version bumped to 0.2.0 (`Directory.Build.props`).
- Inspect-by-id deliberately selects without zooming (`ShowElements` is not called), since
  linked ids frequently point at views, element types, and other non-physical elements.

## [0.1.0] - 2026-07-11

### Added

- Repository structure scaffolding (`src/`, `dictionary/`, `docs/`, `samples/`, `install/`).
- Solution structure with `Core`, `Revit`, `UI`, `Export`, and `Dictionary` project skeletons.
- Core data model skeleton (`ElementContextSnapshot` and related types).
- Revit external command, selection reader (current selection + pick), and Identity/
  Classification/Parameter builders.
- WPF Inspector UI with Summary/Parameters/Geometry/Location/Relationships/Raw JSON tabs,
  plus parameter search and instance/type scope filtering.
- Export layer: JSON, Markdown, and Excel exporters, plus Copy AI Context.
- Dictionary Engine: file-based loader, multi-tier priority merge (user override > community >
  built-in), resolver with raw-API-name fallback, and unresolved-term tracking.
- Dictionary Engine wired into Identity (ClassName, BuiltInCategory), Classification
  (ElementKind), and Parameters (BuiltInParameter only - Shared/Project/Family parameters are
  never translated).
- `ViewContextInfoBuilder` and `SheetContextInfoBuilder`, plus a corresponding
  "View / Sheet Context" UI tab.
- View/Sheet Context rendering in Markdown export, AI Context, and Excel export
  (`View_Sheet_Context` sheet).
- Dictionary and AI Context UI tabs, backed by a newly populated `ElementContextSnapshot.
  Dictionary` term list and `UnresolvedDictionaryTerms`.
- Rendering of actual resolved/unresolved dictionary terms (not just a bare count) across
  Markdown, AI Context, and Excel exports.
- README rewritten with a positioning statement, supported Revit versions, and a per-module
  implementation status checklist.

### Fixed

- `ObjectInspector` no longer silently drops enum-typed fields (e.g. `Discipline`,
  `GeometryReadStatus`, `LocationType`) that share a namespace with `ElementContextSnapshot`;
  they were being mistaken for nested objects with no properties and vanishing with no error.
