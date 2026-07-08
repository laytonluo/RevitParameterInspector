using Autodesk.Revit.DB;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Builds a lightweight geometry summary: bounding box plus simple counts. Full geometry
/// traversal is intentionally out of scope for V1 performance reasons. See HANDOFF Section 14.
/// </summary>
public static class GeometryInfoBuilder
{
    public static CoreModels.GeometryInfo Build(Element element)
    {
        var info = new CoreModels.GeometryInfo
        {
            ApiPath = "Element.get_BoundingBox(null) / Element.get_Geometry(Options)",
        };

        ReadBoundingBox(element, info);
        ReadGeometryCounts(element, info);

        return info;
    }

    private static void ReadBoundingBox(Element element, CoreModels.GeometryInfo info)
    {
        try
        {
            var boundingBox = element.get_BoundingBox(null);
            if (boundingBox is null)
            {
                return;
            }

            info.HasBoundingBox = true;
            info.BoundingBoxMin = ToPoint3D(boundingBox.Min);
            info.BoundingBoxMax = ToPoint3D(boundingBox.Max);
            info.BoundingBoxCenter = new CoreModels.Point3D(
                (boundingBox.Min.X + boundingBox.Max.X) / 2.0,
                (boundingBox.Min.Y + boundingBox.Max.Y) / 2.0,
                (boundingBox.Min.Z + boundingBox.Max.Z) / 2.0);

            var sizeX = boundingBox.Max.X - boundingBox.Min.X;
            var sizeY = boundingBox.Max.Y - boundingBox.Min.Y;
            var sizeZ = boundingBox.Max.Z - boundingBox.Min.Z;
            info.BoundingBoxSize = new CoreModels.Point3D(sizeX, sizeY, sizeZ);
            info.WidthX = sizeX;
            info.DepthY = sizeY;
            info.HeightZ = sizeZ;
        }
        catch
        {
            info.GeometryReadStatus = CoreModels.GeometryReadStatus.Failed;
        }
    }

    private static void ReadGeometryCounts(Element element, CoreModels.GeometryInfo info)
    {
        try
        {
            var options = new Options { ComputeReferences = false, DetailLevel = ViewDetailLevel.Coarse };
            var geometryElement = element.get_Geometry(options);

            if (geometryElement is null)
            {
                if (info.GeometryReadStatus == CoreModels.GeometryReadStatus.NotAttempted)
                {
                    info.GeometryReadStatus = CoreModels.GeometryReadStatus.Unsupported;
                }

                return;
            }

            CountGeometry(geometryElement, info);
            info.GeometryAvailable = true;

            if (info.GeometryReadStatus == CoreModels.GeometryReadStatus.NotAttempted)
            {
                info.GeometryReadStatus = CoreModels.GeometryReadStatus.Ok;
            }
        }
        catch
        {
            info.GeometryReadStatus = CoreModels.GeometryReadStatus.Failed;
        }
    }

    private static void CountGeometry(GeometryElement geometryElement, CoreModels.GeometryInfo info)
    {
        foreach (var geoObject in geometryElement)
        {
            switch (geoObject)
            {
                case Solid solid when solid.Volume > 0:
                    info.SolidCount++;
                    info.FaceCount += solid.Faces.Size;
                    info.EdgeCount += solid.Edges.Size;
                    break;
                case Curve:
                    info.CurveCount++;
                    break;
                case Mesh:
                    info.MeshCount++;
                    break;
                case GeometryInstance geometryInstance:
                    var instanceGeometry = geometryInstance.GetInstanceGeometry();
                    if (instanceGeometry is not null)
                    {
                        CountGeometry(instanceGeometry, info);
                    }

                    break;
            }
        }
    }

    private static CoreModels.Point3D ToPoint3D(XYZ xyz) => new(xyz.X, xyz.Y, xyz.Z);
}
