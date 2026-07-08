# Contributing

> General code contribution guidelines (code style, PR process) are not yet written. This
> file currently covers the Dictionary contribution rules from
> `HANDOFF_RevitParameterInspector_V1_Full.md`, Sections 20 and 39.

## Contributing to the Dictionary

The Dictionary is a file-based, community-editable terminology mapping layer. It is optional
and non-blocking: the Inspector works with or without it, and a missing or invalid dictionary
file never crashes the add-in - the raw Revit API name is always shown as a fallback.

### Where dictionary files live

```text
dictionary/
├─ zh-TW/
│  ├─ api_terms.json
│  ├─ view_sheet_terms.json
│  ├─ geometry_terms.json
│  ├─ parameter_terms.json
│  ├─ family_terms.json
│  ├─ builtin_categories.json
│  └─ builtin_parameters_common.json
└─ en-US/
   └─ api_terms.json
```

Each file is a JSON array of term entries for that locale. Add new terms to the file that
matches their category, or create a new locale folder following the same layout.

### Term entry schema

```json
{
  "termKey": "Viewport",
  "apiName": "Viewport",
  "localizedName": "視埠",
  "locale": "zh-TW",
  "description": "Taiwan CAD/BIM usage",
  "keywords": ["viewport", "視埠"],
  "category": "ViewSheetTerm",
  "priority": 100,
  "status": "Reviewed",
  "source": "BuiltIn",
  "notes": "Optional: region-specific usage notes"
}
```

- `apiName` must match the raw Revit API identifier exactly: a class name (`Viewport`), a
  `BuiltInCategory` enum name (`OST_Doors`), or a `BuiltInParameter` enum name
  (`ALL_MODEL_MARK`) - never a user-facing label, since those vary by Revit's own UI language.
- `status` is one of `Default`, `Reviewed`, `CommunitySuggested`, `Deprecated`, `NeedsReview`,
  `NotFound`. Use `NeedsReview` when you're not fully confident in a translation.
- `lastUpdated` and `contributor` are optional but encouraged for traceability.

### Rules

1. **Do not translate user-created parameters.** The dictionary only targets Revit API terms,
   class names, `BuiltInCategory`, `BuiltInParameter`, and common Revit concepts - never
   Shared Parameters, Project Parameters, Family Parameters, or other custom/company-specific
   fields. The Dictionary Engine already enforces this for parameters (it only looks up
   `BuiltInParameter` entries), but keep it in mind when adding new term files too.
2. **Keep API names unchanged.** The raw API name (`apiName`) must always remain the exact
   Revit identifier. Dictionary values are additive metadata only, never a replacement.
3. **Prefer industry-accepted terminology** over a literal or Autodesk-official translation
   when local industry usage differs.
4. **Add aliases via `keywords`** when terminology varies across sources or usage.
5. **Use `NeedsReview`** for the `status` field when you're unsure a translation is correct.
6. **Include `notes`** for region-specific usage (e.g. why a term is translated a particular
   way for Taiwan BIM/CAD practice).
7. **Keep zh-TW and zh-CN separate.** Do not merge or share entries across these locales even
   when the translation happens to be identical.
8. **Do not remove English API terms.** `apiName` and `termKey` must never be deleted or
   renamed to a non-API value.
9. **Do not force an official translation** if it conflicts with established industry usage -
   prefer what practitioners actually say.
10. **Use a Pull Request** for any addition or correction so it can be reviewed.

### Example

`Viewport` is mapped to `視埠` for zh-TW because that is the term commonly used in Taiwan
CAD/BIM practice, even though it differs from a literal translation. The raw API name
`Viewport` is preserved unchanged everywhere in the Inspector's output.
