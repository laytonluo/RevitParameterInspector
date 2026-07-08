namespace RevitParameterInspector.Core.Models;

/// <summary>High-level kind of the inspected element. See HANDOFF Section 12.</summary>
public enum ElementKind
{
    ModelElement,
    AnnotationElement,
    View,
    Sheet,
    Viewport,
    TitleBlock,
    ElementType,
    Datum,
    Material,
    Unknown,
}

/// <summary>Mirrors Revit's CategoryType without taking a Revit API dependency in Core.</summary>
public enum CategoryType
{
    Model,
    Annotation,
    Internal,
    AnalyticalModel,
    Invalid,
    Unknown,
}

/// <summary>Discipline classification for elements and views. See HANDOFF Sections 12 and 17.</summary>
public enum Discipline
{
    Architectural,
    Structural,
    Mechanical,
    Electrical,
    Plumbing,
    Coordination,
    Unknown,
}

/// <summary>Whether a parameter is an instance, type, or system-level parameter. See HANDOFF Section 13.</summary>
public enum ParameterScope
{
    Instance,
    Type,
    System,
    Unknown,
}

/// <summary>Origin of a parameter definition. See HANDOFF Section 13.</summary>
public enum ParameterKind
{
    BuiltInParameter,
    SharedParameter,
    ProjectParameter,
    FamilyParameter,
    Unknown,
}

/// <summary>Placement mechanism used by an element. See HANDOFF Section 15.</summary>
public enum LocationType
{
    LocationPoint,
    LocationCurve,
    None,
    Unsupported,
    Unknown,
}

/// <summary>Result of a geometry read attempt. See HANDOFF Section 14.</summary>
public enum GeometryReadStatus
{
    Ok,
    NotAttempted,
    Unsupported,
    Failed,
}

/// <summary>Dictionary lookup/review status for a term or parameter. See HANDOFF Sections 13, 19, and 20.4.</summary>
public enum DictionaryStatus
{
    Default,
    Reviewed,
    CommunitySuggested,
    Deprecated,
    NeedsReview,
    NotFound,
}
