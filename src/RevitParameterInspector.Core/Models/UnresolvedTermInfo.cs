namespace RevitParameterInspector.Core.Models;

/// <summary>One API name that was looked up but had no dictionary mapping, with its lookup category.</summary>
public sealed class UnresolvedTermInfo
{
    public string? Term { get; set; }
    public DictionaryTermCategory Category { get; set; } = DictionaryTermCategory.ApiTerm;
}
