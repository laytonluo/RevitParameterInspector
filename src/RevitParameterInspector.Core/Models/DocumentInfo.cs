namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Describes the active Revit document the inspected element belongs to.
/// Part of the "document" section of <see cref="ElementContextSnapshot"/>. See HANDOFF Section 10.
/// </summary>
public sealed class DocumentInfo
{
    public string? Title { get; set; }
    public string? PathName { get; set; }
    public bool IsWorkshared { get; set; }
    public bool IsLinked { get; set; }
    public string? RevitProductName { get; set; }
    public string? RevitBuildNumber { get; set; }
}
