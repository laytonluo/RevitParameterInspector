namespace RevitParameterInspector.Core.Models;

/// <summary>Identifies the selected element. See HANDOFF Section 11.</summary>
public sealed class IdentityInfo
{
    /// <summary>Revit 2024+ moved ElementId storage to 64-bit; store as long, not int.</summary>
    public long ElementId { get; set; }
    public string? ElementIdString { get; set; }
    public string? UniqueId { get; set; }
    public string? Name { get; set; }
    public string? ClassName { get; set; }
    public string? CategoryName { get; set; }
    public long? CategoryId { get; set; }
    public string? BuiltInCategory { get; set; }
    public string? FamilyName { get; set; }
    public string? TypeName { get; set; }
    public long? TypeId { get; set; }
    public string? DocumentTitle { get; set; }
    public string? DocumentPath { get; set; }
    public bool IsLinkedElement { get; set; }
    public string? LinkedDocumentTitle { get; set; }
    public long? LinkedElementId { get; set; }
    public string? ApiPath { get; set; }

    // Dictionary lookups are additive only; ClassName/BuiltInCategory above always stay raw (HANDOFF Section 5.3).
    public string? ClassNameLocalized { get; set; }
    public string? ClassNameDescription { get; set; }
    public DictionaryStatus ClassNameDictionaryStatus { get; set; } = DictionaryStatus.NotFound;

    public string? BuiltInCategoryLocalized { get; set; }
    public string? BuiltInCategoryDescription { get; set; }
    public DictionaryStatus BuiltInCategoryDictionaryStatus { get; set; } = DictionaryStatus.NotFound;
}
