using System;
using System.Collections.Generic;

using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Looks up a single API name against the merged term index. If no mapping exists, returns
/// a NotFound placeholder (localizedName/description null, keywords empty) so callers fall
/// back to the raw API name without special-casing "no dictionary" (HANDOFF Section 20.4).
/// Also accumulates every distinct term it successfully resolved during this instance's
/// lifetime, so a caller can surface "which dictionary terms applied to this element" (HANDOFF
/// Section 29) without having to re-derive it from the built snapshot.
/// </summary>
public sealed class DictionaryResolver
{
    private readonly IReadOnlyDictionary<string, DictionaryTermInfo> _termsByApiName;
    private readonly List<DictionaryTermInfo> _resolvedTerms = new();
    private readonly HashSet<string> _resolvedApiNames = new(StringComparer.OrdinalIgnoreCase);

    public UnresolvedTermCollector UnresolvedTerms { get; }

    /// <summary>Distinct terms (by API name) successfully resolved so far, in first-seen order.</summary>
    public IReadOnlyList<DictionaryTermInfo> ResolvedTerms => _resolvedTerms;

    public DictionaryResolver(
        IReadOnlyDictionary<string, DictionaryTermInfo> termsByApiName,
        UnresolvedTermCollector? unresolvedTermCollector = null)
    {
        _termsByApiName = termsByApiName;
        UnresolvedTerms = unresolvedTermCollector ?? new UnresolvedTermCollector();
    }

    public DictionaryTermInfo Resolve(string apiName)
    {
        if (!string.IsNullOrWhiteSpace(apiName) && _termsByApiName.TryGetValue(apiName, out var term))
        {
            if (_resolvedApiNames.Add(apiName))
            {
                _resolvedTerms.Add(term);
            }

            return term;
        }

        UnresolvedTerms.Record(apiName);
        return new DictionaryTermInfo
        {
            ApiName = apiName,
            Status = DictionaryStatus.NotFound,
        };
    }
}
