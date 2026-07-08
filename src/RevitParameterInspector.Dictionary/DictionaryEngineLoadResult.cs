using System.Collections.Generic;

namespace RevitParameterInspector.Dictionary;

/// <summary>Result of <see cref="DictionaryEngine.Load"/>: a ready-to-use resolver plus any non-fatal load warnings.</summary>
public sealed class DictionaryEngineLoadResult
{
    public DictionaryResolver Resolver { get; }
    public IReadOnlyList<string> Warnings { get; }

    public DictionaryEngineLoadResult(DictionaryResolver resolver, IReadOnlyList<string> warnings)
    {
        Resolver = resolver;
        Warnings = warnings;
    }
}
