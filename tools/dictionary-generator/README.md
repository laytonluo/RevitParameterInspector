# Dictionary generator

Reproduces `dictionary/zh-TW/builtin_categories.json` and
`dictionary/zh-TW/builtin_parameters_common.json` from a running Revit, so those two
machine-generated files can be re-derived, diffed, and regenerated for a new Revit version
instead of being an opaque blob.

`LabelUtils.GetLabelFor(BuiltInCategory)` / `GetLabelFor(BuiltInParameter)` returns exactly the
label Revit's own UI shows for that enum member, in whatever language the running Revit is
localized to. That makes it the one authoritative starting point for a locale dictionary - but
only a *starting* point, see "Why every entry ships as NeedsReview" below.

## Why this is a dev tool, not part of the add-in

`LabelUtils` only works inside a running Revit process, so generating the dictionary requires
loading something into Revit. That is a one-off maintenance action, not something the Inspector
itself should ever do, so it lives here as a separate throwaway add-in rather than as a command
in `RevitParameterInspector.Revit`.

## Step 1 - dump the labels from Revit

```powershell
dotnet build tools/dictionary-generator/RpiLabelGen.csproj -c Release -p:RevitVersion=2024
```

Deploy the built `RpiLabelGen.dll` plus an `.addin` manifest pointing at
`RpiLabelGen.DumpLabelsCommand` into `%APPDATA%\Autodesk\Revit\Addins\<version>\`, start Revit,
**open any project** (the Add-Ins tab is not shown with no document open), then run
**Add-Ins > External Tools > Dump Revit Labels**.

It writes `%LOCALAPPDATA%\RpiLabelGen\labels-<version>.json`:

```json
{
  "revitVersion": "2024",
  "revitBuild": "24.3.40.26",
  "language": "Chinese_Traditional",
  "generatedAt": "2026-08-24T00:12:59Z",
  "categories": [{ "n": "OST_Doors", "l": "門" }],
  "parameters": [{ "n": "ALL_MODEL_MARK", "l": "標記" }]
}
```

The command is `TransactionMode.ReadOnly` and never touches the model.

Two details worth knowing if you modify it:

- It iterates `Enum.GetNames`, not `Enum.GetValues`. `BuiltInParameter` has aliases (several
  names sharing one numeric value) and `GetValues` collapses those, silently dropping names the
  dictionary is supposed to key on.
- Enum members with no user-facing label (`GetLabelFor` throws, or returns empty) are skipped.
  That is expected for a large share of the enum, not an error - those members have no
  terminology to record.

## Step 2 - turn a dump into dictionary files

```bash
python tools/dictionary-generator/build_dictionary.py \
    --primary   %LOCALAPPDATA%\RpiLabelGen\labels-2024.json \
    --crosscheck %LOCALAPPDATA%\RpiLabelGen\labels-2023.json \
    --out dictionary/zh-TW
```

`--primary` should be the **lowest Revit version this repo supports** (2024): every enum name in
it is one the add-in can actually look up. `--crosscheck` is optional and changes no output - it
only reports how many labels Autodesk retranslated between the two versions, and which enum
names exist in one but not the other.

That cross-check is worth running. Between Revit 2023 and 2024, Autodesk changed 31 zh-TW
labels, including outright corrections - `PHY_MATERIAL_PARAM_SHEAR_PERPENDICULAR` (a *shear*
parameter) was labelled 互垂於紋理的拉力 ("tensile") in 2023 and 互垂於紋理的剪力 ("shear") in
2024. Generating from the oldest Revit you happen to have installed would bake those mistakes
into the dictionary.

## Why every entry ships as `NeedsReview`

`CONTRIBUTING.md` rules 3 and 9 say local industry usage wins over a literal or Autodesk-official
translation. Everything this tool emits *is* the Autodesk-official translation, so none of it has
earned `Reviewed` yet - a human still has to confirm each term against Taiwan BIM/CAD practice
and promote it. Known examples already visible in the output:

- `OST_Levels` is labelled 多個樓層, where practitioners say 樓層.
- 52 parameters have labels Autodesk never translated at all (`FABRICATION_PRODUCT_DATA_OEM` is
  `OEM`, `DIRECTCONTEXT3D_APPLICATION_ID` is `ApplicationId`). They are kept, because that is
  genuinely what Revit's UI shows, but they carry no translation value.

Fields that are a constant across every generated entry are deliberately **not** written:
`locale` (the loader fills it in from the folder name), `priority` (`DictionaryTermMatcher`
never reads it), `keywords` (nothing machine-generated to put there), and `notes` (one identical
sentence repeated thousands of times belongs here, once, not in every entry - `source` carries
the provenance). Together they were ~800 KB of the ~2.3 MB first draft.
