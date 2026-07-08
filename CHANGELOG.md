# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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
