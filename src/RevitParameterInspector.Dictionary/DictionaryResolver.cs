using System.Collections.Generic;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Looks up a single API name against the merged term index. If no mapping exists, returns
/// a NotFound placeholder (localizedName/description null, keywords empty) so callers fall
/// back to the raw API name without special-casing "no dictionary" (HANDOFF Section 20.4).
/// </summary>
public sealed class DictionaryResolver
{
    private readonly IReadOnlyDictionary<string, DictionaryTermInfo> _termsByApiName;

    public UnresolvedTermCollector UnresolvedTerms { get; }

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
