using System;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Builds placement data (distinct from shape/range geometry). See HANDOFF Section 15.
/// </summary>
public static class LocationInfoBuilder
{
    public static CoreModels.LocationInfo Build(Element element)
    {
        var info = new CoreModels.LocationInfo { ApiPath = "Element.Location" };

        ReadLocation(element, info);
        ReadLevel(element, info);

        return info;
    }

    private static void ReadLocation(Element element, CoreModels.LocationInfo info)
    {
        Location? location;
        try
        {
            location = element.Location;
        }
        catch
        {
            info.LocationType = CoreModels.LocationType.Unknown;
            return;
        }

        if (location is null)
        {
            info.LocationType = CoreModels.LocationType.None;
            return;
        }

        try
        {
            switch (location)
            {
                case LocationPoint locationPoint:
                    info.LocationType = CoreModels.LocationType.LocationPoint;
                    info.HasLocation = true;
                    info.Point = ToPoint3D(locationPoint.Point);
                    info.RotationRadians = locationPoint.Rotation;
                    info.RotationDegrees = locationPoint.Rotation * 180.0 / Math.PI;
                    break;

                case LocationCurve locationCurve:
                    info.LocationType = CoreModels.LocationType.LocationCurve;
                    info.HasLocation = true;
                    var curve = locationCurve.Curve;
                    var start = curve.GetEndPoint(0);
                    var end = curve.GetEndPoint(1);
                    info.CurveStartPoint = ToPoint3D(start);
                    info.CurveEndPoint = ToPoint3D(end);
                    info.CurveLength = curve.Length;
                    info.CurveDirection = ToPoint3D((end - start).Normalize());
                    break;

                default:
                    info.LocationType = CoreModels.LocationType.Unsupported;
                    info.HasLocation = true;
                    break;
            }
        }
        catch
        {
            info.LocationType = CoreModels.LocationType.Unknown;
        }
    }

    private static void ReadLevel(Element element, CoreModels.LocationInfo info)
    {
        try
        {
            var levelId = element.LevelId;
            if (levelId is null || levelId == ElementId.InvalidElementId)
            {
                return;
            }

            info.LevelId = RevitCompatibility.GetIdValue(levelId);
            info.LevelName = (element.Document.GetElement(levelId) as Level)?.Name;
        }
        catch
        {
            // LevelId is not meaningful for every element type.
        }
    }

    private static CoreModels.Point3D ToPoint3D(XYZ xyz) => new(xyz.X, xyz.Y, xyz.Z);
}
