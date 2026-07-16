# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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
