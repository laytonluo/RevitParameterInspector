using System.Collections.Generic;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Convenience entry point tying <see cref="DictionaryLoader"/>, <see cref="DictionaryTermMatcher"/>,
/// and <see cref="DictionaryResolver"/> together: load every priority tier for a locale and
/// hand back a ready-to-use resolver. Loading problems never throw - they surface as
/// <see cref="DictionaryEngineLoadResult.Warnings"/> instead (HANDOFF Sections 5.2, 20.4, 35, 37).
/// Wiring this into the element readers and a UI Dictionary tab is left for a later step.
/// </summary>
public static class DictionaryEngine
{
    /// <summary>
    /// Loads dictionary terms for <paramref name="locale"/> from each directory in
    /// <paramref name="directoriesInPriorityOrder"/> (first = highest priority, e.g. a user
    /// override folder, then a community folder, then the built-in directory shipped with
    /// the add-in - see HANDOFF Section 20.3).
    /// </summary>
    public static DictionaryEngineLoadResult Load(IEnumerable<string> directoriesInPriorityOrder, string locale)
    {
        var warnings = new List<string>();
        var tiers = new List<IReadOnlyList<DictionaryTermInfo>>();

        foreach (var directory in directoriesInPriorityOrder)
        {
            var result = DictionaryLoader.LoadFromDirectory(directory, locale);
            tiers.Add(result.Terms);
            warnings.AddRange(result.Warnings);
        }

        var merged = DictionaryTermMatcher.Merge(tiers);
        var resolver = new DictionaryResolver(merged);

        return new DictionaryEngineLoadResult(resolver, warnings);
    }
}
