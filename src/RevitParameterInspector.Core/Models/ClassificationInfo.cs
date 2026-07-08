namespace RevitParameterInspector.Core.Models;

/// <summary>Classifies what kind of Revit object the selected element is. See HANDOFF Section 12.</summary>
public sealed class ClassificationInfo
{
    public ElementKind ElementKind { get; set; } = ElementKind.Unknown;
    public CategoryType CategoryType { get; set; } = CategoryType.Unknown;
    public bool IsElementType { get; set; }
    public bool IsFamilyInstance { get; set; }
    public bool IsView { get; set; }
    public bool IsSheet { get; set; }
    public bool IsViewport { get; set; }
    public bool IsTitleBlock { get; set; }
    public bool IsAnnotation { get; set; }
    public bool IsModelElement { get; set; }
    public bool IsDatumElement { get; set; }
    public bool IsSystemFamily { get; set; }
    public bool IsLoadableFamily { get; set; }
    public Discipline Discipline { get; set; } = Discipline.Unknown;

    /// <summary>Which inspection groups (Parameters/Geometry/Location/etc.) are meaningful for this element.</summary>
    public List<string> SupportedInspectionGroups { get; set; } = new();
}
