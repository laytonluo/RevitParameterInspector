namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Represents one parameter row. Raw API names must never be overwritten by dictionary
/// values; dictionary fields (LocalizedName, Description, Keywords) are additive only.
/// See HANDOFF Sections 5.3, 5.4, and 13.
/// </summary>
public sealed class ParameterInfoRecord
{
    public string? Name { get; set; }
    public string? LocalizedName { get; set; }
    public string? Description { get; set; }
    public List<string> Keywords { get; set; } = new();
    public string? ApiPath { get; set; }

    public ParameterScope Scope { get; set; } = ParameterScope.Unknown;
    public ParameterKind ParameterKind { get; set; } = ParameterKind.Unknown;

    public string? StorageType { get; set; }
    public string? ValueRaw { get; set; }
    public string? ValueDisplay { get; set; }
    public string? UnitType { get; set; }
    public string? DataType { get; set; }
    public string? GroupName { get; set; }
    public string? GroupTypeId { get; set; }
    public string? BuiltInParameter { get; set; }
    public long? BuiltInParameterId { get; set; }

    public bool IsShared { get; set; }
    public bool IsProjectParameter { get; set; }
    public bool IsFamilyParameter { get; set; }
    public bool IsBuiltIn { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsWritable { get; set; }
    public bool HasValue { get; set; }

    public long? SourceElementId { get; set; }
    public ElementKind? SourceElementKind { get; set; }

    public DictionaryStatus DictionaryStatus { get; set; } = DictionaryStatus.NotFound;
}
