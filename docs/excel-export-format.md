# Excel Export Format

`ExcelExporter.ExportToFile` writes one `.xlsx` workbook (via ClosedXML) per HANDOFF Section
32.3, with one sheet per section. See
`samples/excel/element-context_Sample_Project.rvt_305001_20260709-101500.xlsx` for a complete
worked example. CSV export is intentionally not implemented (HANDOFF Section 32.4 - JSON is
better for machines, Excel better for BIM users, Markdown better for AI/humans).

## Sheets (in order)

| Sheet | Columns | Notes |
|---|---|---|
| `Summary` | Field / Value | Same fields as the Summary UI tab |
| `Parameters_All` | Name, Localized Name, Scope, Value Display, Value Raw, Storage Type, Built-in Parameter, Parameter Kind, Is Read Only, Has Value, API Path, Description | All parameters, instance + type |
| `Parameters_Instance` | (same columns) | Filtered to `Scope == Instance` |
| `Parameters_Type` | (same columns) | Filtered to `Scope == Type` |
| `Geometry` | Field / Value | Reflection dump via `ObjectInspector` |
| `Location` | Field / Value | Reflection dump via `ObjectInspector` |
| `Relationships` | Field / Value | Reflection dump via `ObjectInspector` |
| `View_Sheet_Context` | Field / Value | `ObjectInspector` dump of `ViewContext` (prefixed `View.`) followed by `SheetContext` (prefixed `Sheet.`) |
| `Dictionary` | Term Key, API Name, Localized Name, Locale, Description, Status, Source, Notes | One row per resolved term, followed by an "Unresolved Terms" block (bold header row + one row per unresolved API name) when any exist |
| `Raw_Metadata` | Field / Value | Schema Version, Generated At, Revit Version, Addin Version, Document Title/Path, Is Workshared, Export Format/File Name/Exported At, plus one row per entry in the snapshot's `Raw` dictionary |

## Conventions

- Every "Field / Value" sheet is built by `ObjectInspector.ToFieldRows`, the same helper the
  WPF Geometry/Location/Relationships/View-Sheet-Context tabs use - so the Excel sheet and the
  corresponding UI tab always show the same fields, in the same order, with the same
  formatting (`Point3D` values render as `"X, Y, Z"`; enums render as their name, not a number
  - unlike the JSON export, see [ai-json-schema.md](ai-json-schema.md)).
- Every sheet has its header row bolded and frozen (`FreezeRows(1)`), and columns are
  auto-sized to their content (`AdjustToContents()`).
- If no dictionary is loaded (or nothing resolved), the `Dictionary` sheet's first data row
  instead reads "No dictionary is loaded in this build; all names are raw Revit API names."
- `ExportMetadata` is set immediately before this workbook is written, so `Raw_Metadata`'s
  Export Format/File Name/Exported At always describe this specific Excel export, not
  whichever format was exported most recently for the same in-memory snapshot.
