namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Populated when the selected element is a ViewSheet, Viewport, TitleBlock,
/// ScheduleSheetInstance, or a View placed on a sheet. See HANDOFF Section 18.
/// </summary>
public sealed class SheetContextInfo
{
    public long? SheetId { get; set; }
    public string? SheetNumber { get; set; }
    public string? SheetName { get; set; }

    public long? TitleBlockElementId { get; set; }
    public string? TitleBlockFamilyName { get; set; }
    public string? TitleBlockTypeName { get; set; }

    public List<long> ViewportIds { get; set; } = new();
    public List<long> PlacedViewIds { get; set; } = new();
    public List<long> ScheduleSheetInstanceIds { get; set; } = new();
    public List<long> RevisionIds { get; set; } = new();

    public string? PaperSizeName { get; set; }
    public double? PaperWidth { get; set; }
    public double? PaperHeight { get; set; }
}
