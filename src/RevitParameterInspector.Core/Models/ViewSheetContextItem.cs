namespace RevitParameterInspector.Core.Models;

/// <summary>
/// One UI-ready row for the View / Sheet Context page: which View or Sheet context is
/// directly related to the inspected element (HANDOFF_Update_ViewSheetContext_V1 Section 4).
/// This is deliberately simpler than <see cref="ViewContextInfo"/>/<see cref="SheetContextInfo"/>,
/// which remain as the detailed data structures.
/// </summary>
public sealed class ViewSheetContextItem
{
    public string? ContextType { get; set; }
    public string? Name { get; set; }
    public string? ElementId { get; set; }
    public string? UniqueId { get; set; }
    public string? AdditionalInfo { get; set; }
}
