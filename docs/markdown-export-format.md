# Markdown Export Format

`MarkdownExporter.Build` / `ExportToFile` produces the full, human/AI-readable Markdown export
(HANDOFF Section 32.2). See
`samples/markdown/element-context_Sample_Project.rvt_305001_20260709-101500.md` for a complete
worked example.

This is distinct from **Copy AI Context** (`AiContextComposer`, bound to the "AI Context" UI
tab and the bottom action bar's "Copy AI Context" button): that format covers similar ground
but stays deliberately short (it skips the raw Classification dump and caps how many writable
parameters it lists) since it's meant to be pasted directly into a chat window, not saved as a
reference document. If you want the exhaustive version, use the Markdown export described here.

## Section order

1. **Title** - `# Element Context: {ClassName} ({ElementId})`
2. **Summary** - Element Id, Unique Id, Class Name, Category (with `BuiltInCategory` in
   parentheses when known), Name, Family, Type, Document, Is Linked Element
3. **Classification** - Element Kind, Category Type, Discipline, and the various `Is*` flags
4. **Parameters** - split into `### Writable Parameters (N)` and `### Read-only Parameters (N)`
   tables (Name / Scope / Value / Storage Type / Built-in Parameter)
5. **Geometry** - bounding box min/max/center, size, solid/curve/face/edge/mesh counts, read
   status
6. **Location** - location type, point, rotation, curve start/end/length, level
7. **Relationships** - type, family, host, level, room, space, view owner, sheet, materials
8. **View / Sheet Context** - `### View Context` and `### Sheet Context` subsections, each
   only present when that data exists on the snapshot
9. **Dictionary Notes** - one bullet per resolved dictionary term (`ApiName: LocalizedName
   (Status)`) plus an "Unresolved Terms" bullet listing API names that had no mapping
10. **Suggested API Paths** - a deduplicated list of the `ApiPath` values collected from
    Identity/Geometry/Location, plus the fixed parameter-reading API path

## Conventions

- Every section header (`##`) is always printed, even when the underlying data is null - in
  that case the body is `_Not available._` instead of the section being omitted. This makes it
  safe to `grep` for a section heading without worrying whether it exists.
- Missing/empty scalar values render as `_(none)_` (see `MarkdownFormat.Bullet`).
- Table cells escape pipe characters and collapse embedded newlines
  (`MarkdownFormat.EscapeCell`) so a stray `|` or multi-line parameter value can't break the
  table.
- Bullet values use inline escaping only (newlines collapsed to spaces), not full Markdown
  escaping - values are not expected to contain other Markdown syntax.
