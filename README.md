# RevitParameterInspector

A read-only Revit add-in that inspects a selected element's parameters, geometry, location,
relationships, and view/sheet context, then exports the result as JSON, Markdown, Excel, or
AI-readable context. Optional file-based dictionary support maps common Revit API terms to
zh-TW terminology without ever touching raw API names or user-created parameters.

See `HANDOFF_RevitParameterInspector_V1_Full.md` for the full V1 specification driving this
implementation.

## What this is / is not

RevitParameterInspector is a structured element-context inspector, parameter browser, and
JSON/Markdown/Excel/AI-context exporter for Revit API developers, BIM automation engineers,
and AI-assisted coding workflows.

**RevitParameterInspector is not intended to replace [RevitLookup](https://github.com/jeremytammik/RevitLookup).**
RevitLookup is a mature, general-purpose Revit database exploration tool built for deep object
graph traversal. This project instead focuses on structured context extraction, dictionary-ready
terminology mapping, and AI-readable exports. It is not a model modification tool, a parameter
writer, a family editor, a QA/QC engine, or an AI chatbot.

## Supported Revit versions

| Target Framework | Revit Version | Notes |
|---|---|---|
| `net48` | 2024 | |
| `net8.0-windows` | 2025 (default) | |
| `net8.0-windows` | 2026 | build with `-p:RevitVersion=2026` |

Version differences are isolated in `RevitParameterInspector.Revit/Compatibility`. Revit
install location can be overridden with `-p:RevitInstallDir=...` if Revit is installed outside
`C:\Program Files\Autodesk\Revit <version>`.

## Implementation status

This is under active development against the HANDOFF spec. Current state:

### Core (`RevitParameterInspector.Core`)
- [x] `ElementContextSnapshot` and all sub-models (Identity, Classification, Parameters,
      Geometry, Location, Relationships, ViewContext, SheetContext, DictionaryTermInfo,
      ExportMetadata)
- [x] `ObjectInspector` reflection helper shared by the UI grids and Excel sheets

### Revit integration (`RevitParameterInspector.Revit`)
- [x] External command with current-selection / pick-element workflow
- [x] Identity, Classification, Parameter (instance + type), Geometry, Location,
      Relationship, View Context, and Sheet Context builders
- [x] Dictionary Engine wired into Identity (ClassName, BuiltInCategory), Classification
      (ElementKind), and Parameters (BuiltInParameter only - user-created parameters are
      never translated)
- [ ] Ribbon panel / button registration
- [ ] Dedicated "Pick Element" command distinct from the main inspect command

### UI (`RevitParameterInspector.UI`)
- [x] All 9 tabs: Summary, Parameters, Geometry, Location, Relationships, View/Sheet
      Context, Dictionary, Raw JSON, AI Context
- [x] Parameter search and instance/type scope filter

### Export (`RevitParameterInspector.Export`)
- [x] JSON export (full snapshot)
- [x] Markdown export (all sections, including View/Sheet Context and resolved/unresolved
      Dictionary terms)
- [x] Excel export (one sheet per section, including `View_Sheet_Context` and `Dictionary`)
- [x] Copy AI Context (concise Markdown for pasting into chat/AI tools)

### Dictionary (`RevitParameterInspector.Dictionary`)
- [x] File-based loader, multi-tier priority merge, resolver with raw-API-name fallback,
      unresolved-term tracking
- [ ] zh-TW dictionary content is a partial draft (`api_terms.json`,
      `view_sheet_terms.json` have example entries; `builtin_categories.json`,
      `builtin_parameters_common.json`, `family_terms.json`, `geometry_terms.json`,
      `parameter_terms.json` are still empty placeholders)

### Documentation (`docs/`)
- [x] getting-started, build-guide, roadmap, revit-version-support,
      ai-json-schema, markdown-export-format, excel-export-format,
      dictionary-contribution-guide

### Samples (`samples/`)
- [x] One representative example (a Viewport on a sheet) exercising every section, in
      `samples/json`, `samples/markdown`, and `samples/excel`

### Install packaging (`install/`)
- [x] Per-version `.addin` manifests (`install/addin/`) and a multi-version
      `.bundle` (`install/bundle/`, with a `build-bundle.ps1` packaging script) - see
      `install/README.md`

### Not started
- Verification against a real Revit installation (not available in the environment this was
  built in - Revit API references resolve but haven't been exercised inside Revit itself; see
  `docs/revit-version-support.md`)
- CI/release pipeline for the install packaging (currently a manual, local script)

## License

MIT - see `LICENSE`. See `NOTICE.md` for attribution notes relative to the original
RevitElementBipChecker concept this project modernizes.
