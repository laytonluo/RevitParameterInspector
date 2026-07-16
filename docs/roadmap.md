# Roadmap

For what's already implemented, see the README's
[Implementation status](../README.md#implementation-status). This doc covers what's next.

## Remaining V1 work

Items still open from `HANDOFF_RevitParameterInspector_V1_Full.md`'s V1 scope:

- Ribbon panel / button registration (both commands currently only run via Add-Ins >
  External Tools - see `docs/getting-started.md`)
- Filling in the dictionary content gaps tracked in `docs/dictionary-contribution-guide.md`
  (most `BuiltInCategory`/`BuiltInParameter` mappings and about two-thirds of HANDOFF's initial
  zh-TW seed term list)
- Wiring the packaging in `install/` into a CI/release pipeline (currently a manual, local
  `build-bundle.ps1` script - see `install/README.md`)
- Verification against a real Revit installation - none of the `RevitParameterInspector.Revit`
  code (builders, readers, the external command) has been exercised inside a running Revit
  instance yet

## V1.1

From HANDOFF Section 43:

- Improve dictionary coverage
- Add unresolved term export (the data already exists on
  `ElementContextSnapshot.UnresolvedDictionaryTerms` and is rendered in every export format;
  a dedicated standalone export - e.g. a flat list for dictionary contributors to work from -
  is still open)
- Add more `BuiltInCategory` mappings
- Add more `BuiltInParameter` mappings
- Add an "inspect related element" action (jump from a Relationships/View/Sheet Context id to
  inspecting that element directly)

## V1.2

- Batch inspect selected elements
- Category schema export
- Family type schema export
- Tag candidate parameter export
- Export unresolved dictionary terms (as a dedicated deliverable, e.g. for bulk PR review)

## V2.0

- Read-only MCP bridge
- Local context provider
- AI workflow integration
- Project Profile candidate extraction
- Drawing Production Matrix support
- Community dictionary package versioning

## Explicit non-goals

Per HANDOFF Section 42, these are out of scope regardless of version: a full RevitLookup
clone, a full MCP server, an AI chatbot or embedded AI assistant, model modification, parameter
writing, family editing, QA/QC automation, a Project Profile Framework, a Drawing Production
Matrix implementation (only its future export as a V2.0 candidate), a cloud backend, user
login/SaaS licensing, full-model batch scanning, automatic translation of custom parameters, and
complex charts/analytics dashboards.
