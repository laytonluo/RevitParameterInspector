using System;
using System.Collections.Generic;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Merges dictionary terms from multiple priority-ordered tiers (HANDOFF Section 20.3:
/// user override &gt; community &gt; built-in) into a single apiName-keyed lookup. The first
/// tier that defines a given API name wins; later tiers only fill in gaps.
/// </summary>
public static class DictionaryTermMatcher
{
    public static IReadOnlyDictionary<string, DictionaryTermInfo> Merge(
        IEnumerable<IReadOnlyList<DictionaryTermInfo>> tiersInPriorityOrder)
    {
        var merged = new Dictionary<string, DictionaryTermInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var tier in tiersInPriorityOrder)
        {
            foreach (var term in tier)
            {
                var key = term.ApiName ?? term.TermKey;
                if (string.IsNullOrWhiteSpace(key) || merged.ContainsKey(key!))
                {
                    continue;
                }

                merged[key!] = term;
            }
        }

        return merged;
    }
}
