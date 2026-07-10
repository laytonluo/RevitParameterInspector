using System;
using System.Collections.Generic;

using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Tracks distinct API names that were looked up but had no dictionary mapping, with the
/// category each lookup came from, so the Dictionary page can group them and they can serve
/// as a build list for future dictionary content. See HANDOFF Section 29.
/// </summary>
public sealed class UnresolvedTermCollector
{
    private readonly Dictionary<string, UnresolvedTermInfo> _unresolved = new(StringComparer.OrdinalIgnoreCase);

    public void Record(string apiName, DictionaryTermCategory category)
    {
        if (!string.IsNullOrWhiteSpace(apiName) && !_unresolved.ContainsKey(apiName))
        {
            _unresolved.Add(apiName, new UnresolvedTermInfo { Term = apiName, Category = category });
        }
    }

    public IReadOnlyCollection<UnresolvedTermInfo> GetUnresolvedTerms() => _unresolved.Values;
}
