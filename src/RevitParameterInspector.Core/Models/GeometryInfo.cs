namespace RevitParameterInspector.Core.Models;

/// <summary>
/// Lightweight geometry summary (bounding box + simple counts). Full geometry traversal is
/// intentionally out of scope for V1 performance reasons. See HANDOFF Section 14.
/// </summary>
public sealed class GeometryInfo
{
    public bool HasBoundingBox { get; set; }
    public Point3D? BoundingBoxMin { get; set; }
    public Point3D? BoundingBoxMax { get; set; }
    public Point3D? BoundingBoxCenter { get; set; }
    public Point3D? BoundingBoxSize { get; set; }

    public double? WidthX { get; set; }
    public double? DepthY { get; set; }
    public double? HeightZ { get; set; }

    public int SolidCount { get; set; }
    public int CurveCount { get; set; }
    public int FaceCount { get; set; }
    public int EdgeCount { get; set; }
    public int MeshCount { get; set; }

    public bool GeometryAvailable { get; set; }
    public GeometryReadStatus GeometryReadStatus { get; set; } = GeometryReadStatus.NotAttempted;
    public string? ApiPath { get; set; }
}
