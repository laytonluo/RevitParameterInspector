namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Placement data. Distinct from <see cref="GeometryInfo"/>: this describes where the
/// element is placed, not its shape/range. See HANDOFF Section 15.
/// </summary>
public sealed class LocationInfo
{
    public LocationType LocationType { get; set; } = LocationType.Unknown;
    public bool HasLocation { get; set; }

    public Point3D? Point { get; set; }
    public double? RotationRadians { get; set; }
    public double? RotationDegrees { get; set; }

    public Point3D? CurveStartPoint { get; set; }
    public Point3D? CurveEndPoint { get; set; }
    public double? CurveLength { get; set; }
    public Point3D? CurveDirection { get; set; }

    public long? LevelId { get; set; }
    public string? LevelName { get; set; }
    public double? Offset { get; set; }
    public string? ApiPath { get; set; }
}
