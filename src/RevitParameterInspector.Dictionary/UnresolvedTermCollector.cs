using System;
using System.Collections.Generic;

namespace RevitParameterInspector.Dictionary;

/// <summary>
/// Tracks distinct API names that were looked up but had no dictionary mapping, so
/// contributors can see what's missing. See HANDOFF Section 29 ("unresolved terms list").
/// </summary>
public sealed class UnresolvedTermCollector
{
    private readonly HashSet<string> _unresolved = new(StringComparer.OrdinalIgnoreCase);

    public void Record(string apiName)
    {
        if (!string.IsNullOrWhiteSpace(apiName))
        {
            _unresolved.Add(apiName);
        }
    }

    public IReadOnlyCollection<string> GetUnresolvedTerms() => _unresolved;
}
