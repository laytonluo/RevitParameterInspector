using System.Collections.Generic;
using RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Dictionary;

/// <summary>Result of loading one dictionary tier from disk: the terms found, plus any non-fatal problems.</summary>
public sealed class DictionaryLoadResult
{
    public IReadOnlyList<DictionaryTermInfo> Terms { get; }
    public IReadOnlyList<string> Warnings { get; }

    public DictionaryLoadResult(IReadOnlyList<DictionaryTermInfo> terms, IReadOnlyList<string> warnings)
    {
        Terms = terms;
        Warnings = warnings;
    }
}
