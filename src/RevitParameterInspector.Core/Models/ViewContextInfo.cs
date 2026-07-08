namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Populated when the selected element is a View, belongs to a View, is a Viewport, or is
/// placed on a Sheet. Read-only in V1. See HANDOFF Section 17.
/// </summary>
public sealed class ViewContextInfo
{
    public long? ViewId { get; set; }
    public string? ViewName { get; set; }
    public string? ViewType { get; set; }
    public double? Scale { get; set; }

    public bool CropBoxActive { get; set; }
    public bool CropBoxVisible { get; set; }
    public Point3D? CropBoxMin { get; set; }
    public Point3D? CropBoxMax { get; set; }
    public Point3D? CropBoxSize { get; set; }

    public long? ViewTemplateId { get; set; }
    public string? ViewTemplateName { get; set; }
    public string? DetailLevel { get; set; }
    public string? DisplayStyle { get; set; }
    public Discipline Discipline { get; set; } = Discipline.Unknown;

    public long? AssociatedLevelId { get; set; }
    public string? AssociatedLevelName { get; set; }
}
