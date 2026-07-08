# Dictionary Contribution Guide

This is a practical, "what to actually do" companion to the rules already documented in
[CONTRIBUTING.md](../CONTRIBUTING.md#contributing-to-the-dictionary) - read that first for the
file layout, the term entry schema, and the 10 contribution rules (never translate user-created
parameters, keep raw API names unchanged, use `NeedsReview` when unsure, keep zh-TW/zh-CN
separate, etc.). This doc covers the loading model and gives you a concrete starting checklist.

## How entries get loaded and merged

`DictionaryEngine.Load` reads every `*.json` file under `{directory}/{locale}/` for each
directory you give it, in priority order (HANDOFF Section 20.3):

1. A user override directory (not wired up anywhere yet - a future configuration option)
2. A community directory (also not wired up yet)
3. The built-in directory shipped with the add-in (`dictionary/`, copied next to the built DLL
   - see [build-guide.md](build-guide.md))

The **first** tier that defines a given `apiName` wins; later tiers only fill gaps
(`DictionaryTermMatcher.Merge`). Right now only tier 3 (the built-in `dictionary/` folder in
this repo) actually exists, so every contribution today goes there.

A single malformed file or missing directory never breaks the whole load - problems surface as
warnings (`DictionaryEngineLoadResult.Warnings`), and terms that were never found at all get
tracked separately per-element as `UnresolvedDictionaryTerms` (visible in the Dictionary UI tab
and in every export format - see [ai-json-schema.md](ai-json-schema.md#dictionary-term)). If
you're not sure what's missing, inspect a few different element types and check their
Dictionary tab / Unresolved Terms list.

## Starting checklist: HANDOFF's initial zh-TW term list

HANDOFF Section 21 lists 30 initial terms the default zh-TW dictionary should eventually cover.
Here's where each one stands today:

| API Name | zh-TW | Note | Status | Suggested file |
|---|---|---|---|---|
| `Element` | 元件 | General Revit element | ✅ Added | `api_terms.json` |
| `Category` | 類別 | Revit category | ✅ Added | `api_terms.json` |
| `Document` | 專案文件 | RVT/RFA document | ✅ Added | `api_terms.json` |
| `Parameter` | 參數 | Element parameter | ✅ Added | `api_terms.json` |
| `Family` | 族群 | Revit Family | ✅ Added | `api_terms.json` |
| `FamilySymbol` | 族群類型 | Type object | ⬜ Missing | `family_terms.json` |
| `FamilyInstance` | 族群實例 | Placed family instance | ⬜ Missing | `family_terms.json` |
| `ElementType` | 元件類型 | Type-level element | ⬜ Missing | `api_terms.json` |
| `View` | 視圖 | General view | ⬜ Missing | `view_sheet_terms.json` |
| `ViewPlan` | 平面視圖 | Floor plan / ceiling plan | ⬜ Missing | `view_sheet_terms.json` |
| `ViewSection` | 剖面視圖 | Section view | ⬜ Missing | `view_sheet_terms.json` |
| `ViewElevation` | 立面視圖 | Elevation view | ⬜ Missing | `view_sheet_terms.json` |
| `View3D` | 3D視圖 | 3D view | ⬜ Missing | `view_sheet_terms.json` |
| `ViewSheet` | 圖紙 | Sheet container | ✅ Added | `view_sheet_terms.json` |
| `Viewport` | 視埠 | Taiwan CAD/BIM usage | ✅ Added | `view_sheet_terms.json` |
| `TitleBlock` | 圖框 | Sheet title block | ✅ Added | `view_sheet_terms.json` |
| `Schedule` | 明細表 | Schedule view | ⬜ Missing | `view_sheet_terms.json` |
| `Legend` | 圖例 | Legend view | ⬜ Missing | `view_sheet_terms.json` |
| `CropBox` | 裁切框 | View crop region | ⬜ Missing | `geometry_terms.json` |
| `ScopeBox` | 範圍框 | Scope box | ⬜ Missing | `geometry_terms.json` |
| `BoundingBox` | 外包框 | Element bounding box | ⬜ Missing | `geometry_terms.json` |
| `LocationPoint` | 定位點 | Point-based location | ⬜ Missing | `geometry_terms.json` |
| `LocationCurve` | 定位線 | Curve-based location | ⬜ Missing | `geometry_terms.json` |
| `XYZ` | 座標點 | Revit XYZ | ⬜ Missing | `geometry_terms.json` |
| `Host` | 宿主元件 | Host relationship | ⬜ Missing | `api_terms.json` |
| `Level` | 樓層 | Level datum | ⬜ Missing | `api_terms.json` |
| `Workset` | 工作集 | Worksharing workset | ⬜ Missing | `api_terms.json` |
| `Phase` | 階段 | Phase data | ⬜ Missing | `api_terms.json` |
| `Material` | 材料 | Material | ⬜ Missing | `api_terms.json` |
| `Room` | 房間 | Architectural room | ⬜ Missing | `api_terms.json` |
| `Space` | 空間 | MEP space | ⬜ Missing | `api_terms.json` |

`builtin_categories.json` and `builtin_parameters_common.json` (for `BuiltInCategory` and
`BuiltInParameter` enum names respectively) don't have a HANDOFF seed list at all yet - these
are the biggest open gap, since they're what actually drives `Identity.BuiltInCategoryLocalized`
and every `ParameterInfoRecord.LocalizedName` for built-in parameters. Picking a handful of the
most common `BuiltInCategory`/`BuiltInParameter` values you use often and adding them is one of
the most useful first contributions.

The zh-TW/zh-CN translations above are a draft per HANDOFF Section 20.5 - if your industry
usage differs, correct them (with a note explaining why, per rule 6 in CONTRIBUTING.md) rather
than treating the table as final.

## Workflow

1. Pick a term (from the checklist above, or one you noticed in `UnresolvedDictionaryTerms`
   while inspecting an element).
2. Add an entry to the appropriate `dictionary/zh-TW/*.json` file, following the schema in
   CONTRIBUTING.md.
3. Rebuild `RevitParameterInspector.Revit` (the dictionary folder is copied to the output
   directory on build - see [build-guide.md](build-guide.md#output-layout)) and re-inspect an
   element that uses that term to confirm it resolves.
4. Open a Pull Request (CONTRIBUTING.md rule 10).
