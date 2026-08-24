"""Turn RpiLabelGen's raw label dumps into RevitParameterInspector dictionary files.

Primary source is the Revit 2024 dump (the repo's minimum supported version, so every enum
name in it is one the add-in can actually look up). The 2023 dump, when present, is used only
as a cross-check: it reports how many labels Autodesk changed between versions and which enum
names exist in 2023 but not 2024. Nothing 2023-only is written into the dictionary.

Usage:
    python build_dictionary.py --primary labels-2024.json [--crosscheck labels-2023.json]
                               --out <repo>/dictionary/zh-TW
"""

import argparse
import json
import pathlib
import sys

CONTRIBUTOR = "a0917-cell"
# CONTRIBUTING rules 3 and 9: Autodesk's official label is a starting point, not automatically
# the term Taiwan practitioners use - so every generated entry ships as NeedsReview.
STATUS = "NeedsReview"

# Fields deliberately NOT emitted per entry, because at this volume every repeated constant is
# real weight in a file the add-in re-reads on every inspect:
#   locale    - DictionaryLoader already fills it in from the {locale} folder name.
#   priority  - DictionaryTermMatcher merges first-tier-wins and never reads Priority.
#   keywords  - deserializes to an empty list anyway; nothing machine-generated to put in it.
#   notes     - one identical sentence repeated thousands of times; it belongs in the
#               contribution guide once, not in every entry. `source` carries the provenance.

TARGETS = {
    "categories": ("builtin_categories.json", "BuiltInCategory"),
    "parameters": ("builtin_parameters_common.json", "BuiltInParameter"),
}


def load(path):
    with open(path, encoding="utf-8") as handle:
        return json.load(handle)


def load_reviewed(path):
    """Existing entries a human has touched, keyed by apiName.

    Regenerating must never throw away review work, otherwise the first corrected term makes
    these files un-regenerable. Anything still sitting at the generator's own default status
    (NeedsReview) is fair game to overwrite; anything promoted away from it is carried through
    untouched.
    """
    if not path.is_file():
        return {}

    try:
        with open(path, encoding="utf-8") as handle:
            existing = json.load(handle)
    except (ValueError, OSError):
        return {}

    return {
        entry["apiName"]: entry
        for entry in existing
        if entry.get("apiName") and entry.get("status") not in (None, STATUS)
    }


def build_entries(pairs, category, source, reviewed):
    entries = []
    for pair in pairs:
        api_name = pair["n"]
        if api_name in reviewed:
            entries.append(reviewed.pop(api_name))
            continue

        entries.append(
            {
                "termKey": api_name,
                "apiName": api_name,
                "localizedName": pair["l"],
                "category": category,
                "status": STATUS,
                "source": source,
                "lastUpdated": None,  # filled in by main() from the dump's own timestamp
                "contributor": CONTRIBUTOR,
            }
        )
    return entries


def crosscheck(primary, other):
    """Reports label drift between two Revit versions. Pure reporting - changes no output."""
    report = []
    for key, (_, _label) in TARGETS.items():
        primary_map = {pair["n"]: pair["l"] for pair in primary.get(key, [])}
        other_map = {pair["n"]: pair["l"] for pair in other.get(key, [])}

        differing = sorted(
            name
            for name, label in other_map.items()
            if name in primary_map and primary_map[name] != label
        )
        only_other = sorted(set(other_map) - set(primary_map))
        only_primary = sorted(set(primary_map) - set(other_map))

        report.append(
            {
                "kind": key,
                "primary_count": len(primary_map),
                "crosscheck_count": len(other_map),
                "label_differs": differing,
                "only_in_crosscheck": only_other,
                "only_in_primary": only_primary,
            }
        )
    return report


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--primary", required=True)
    parser.add_argument("--crosscheck")
    parser.add_argument("--out", required=True)
    args = parser.parse_args()

    primary = load(args.primary)
    version = primary.get("revitVersion", "?")
    language = primary.get("language", "?")
    generated_at = (primary.get("generatedAt") or "")[:10]
    source = "Autodesk LabelUtils (Revit {0} {1})".format(version, language)

    out_dir = pathlib.Path(args.out)
    if not out_dir.is_dir():
        sys.exit("Output directory does not exist: {0}".format(out_dir))

    written = []
    orphaned = []
    for key, (filename, category) in TARGETS.items():
        target = out_dir / filename
        reviewed = load_reviewed(target)
        reviewed_count = len(reviewed)

        entries = build_entries(primary.get(key, []), category, source, reviewed)
        for entry in entries:
            entry.setdefault("lastUpdated", generated_at)
            if entry["status"] == STATUS:
                entry["lastUpdated"] = generated_at

        # Reviewed entries whose enum member no longer exists in the primary Revit version:
        # kept (someone did the work) but reported, since they can no longer resolve.
        for api_name, entry in sorted(reviewed.items()):
            entries.append(entry)
            orphaned.append("{0}: {1}".format(filename, api_name))

        if reviewed_count:
            print("{0}: preserved {1} reviewed entries".format(filename, reviewed_count))

        with open(target, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(entries, handle, ensure_ascii=False, indent=2)
            handle.write("\n")
        written.append((filename, len(entries), target.stat().st_size))

    for filename, count, size in written:
        print("{0}: {1} entries, {2:.1f} KB".format(filename, count, size / 1024.0))

    for line in orphaned:
        print("kept reviewed entry whose enum member is gone from Revit {0} -> {1}".format(version, line))

    if args.crosscheck:
        report = crosscheck(primary, load(args.crosscheck))
        # Next to the dumps, not in the repo: this is a diagnostic, not dictionary content.
        report_path = pathlib.Path(args.primary).resolve().parent / "labels-crosscheck.json"
        with open(report_path, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(report, handle, ensure_ascii=False, indent=2)
        print("\ncross-check vs {0}:".format(args.crosscheck))
        for section in report:
            print(
                "  {0}: primary {1}, other {2}, label differs {3}, only-in-other {4}, only-in-primary {5}".format(
                    section["kind"],
                    section["primary_count"],
                    section["crosscheck_count"],
                    len(section["label_differs"]),
                    len(section["only_in_crosscheck"]),
                    len(section["only_in_primary"]),
                )
            )
        print("  full lists -> {0}".format(report_path))


if __name__ == "__main__":
    main()
