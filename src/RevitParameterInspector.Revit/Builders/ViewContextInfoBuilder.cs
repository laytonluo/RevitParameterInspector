using System;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Builds view context: populated when the selected element is itself a View (including
/// ViewSheet), a Viewport, or an element placed in/owned by a view. Each property read is
/// independently guarded so one unsupported view type (e.g. a schedule with no crop box)
/// does not blank out the rest of the section. See HANDOFF Section 17.
/// </summary>
public static class ViewContextInfoBuilder
{
    /// <summary>Returns null when the element has no meaningful view context.</summary>
    public static CoreModels.ViewContextInfo? Build(Element element)
    {
        var view = ResolveTargetView(element, element.Document);
        if (view is null)
        {
            return null;
        }

        var info = new CoreModels.ViewContextInfo
        {
            ViewId = RevitCompatibility.GetIdValue(view.Id),
            ViewName = SafeGetName(view),
            ViewType = SafeGet(() => view.ViewType.ToString()),
        };

        ReadScale(view, info);
        ReadCropBox(view, info);
        ReadViewTemplate(view, element.Document, info);
        ReadDetailAndDisplay(view, info);
        ReadDiscipline(view, info);
        ReadAssociatedLevel(view, info);

        return info;
    }

    private static View? ResolveTargetView(Element element, Document document)
    {
        if (element is View view)
        {
            return view;
        }

        if (element is Viewport viewport)
        {
            return SafeGetView(document, viewport.ViewId);
        }

        return SafeGetView(document, SafeGetOwnerViewId(element));
    }

    private static View? SafeGetView(Document document, ElementId? viewId)
    {
        if (viewId is null || viewId == ElementId.InvalidElementId)
        {
            return null;
        }

        try
        {
            return document.GetElement(viewId) as View;
        }
        catch
        {
            return null;
        }
    }

    private static ElementId? SafeGetOwnerViewId(Element element)
    {
        try
        {
            var ownerViewId = element.OwnerViewId;
            return ownerViewId == ElementId.InvalidElementId ? null : ownerViewId;
        }
        catch
        {
            return null;
        }
    }

    private static void ReadScale(View view, CoreModels.ViewContextInfo info)
    {
        try
        {
            info.Scale = view.Scale;
        }
        catch
        {
            // Not every view type has a meaningful scale (e.g. schedules).
        }
    }

    private static void ReadCropBox(View view, CoreModels.ViewContextInfo info)
    {
        try
        {
            info.CropBoxActive = view.CropBoxActive;
            info.CropBoxVisible = view.CropBoxVisible;

            var cropBox = view.CropBox;
            if (cropBox is not null)
            {
                info.CropBoxMin = ToPoint3D(cropBox.Min);
                info.CropBoxMax = ToPoint3D(cropBox.Max);
                info.CropBoxSize = new CoreModels.Point3D(
                    cropBox.Max.X - cropBox.Min.X,
                    cropBox.Max.Y - cropBox.Min.Y,
                    cropBox.Max.Z - cropBox.Min.Z);
            }
        }
        catch
        {
            // Crop box is not supported by every view type.
        }
    }

    private static void ReadViewTemplate(View view, Document document, CoreModels.ViewContextInfo info)
    {
        try
        {
            var templateId = view.ViewTemplateId;
            if (templateId == ElementId.InvalidElementId)
            {
                return;
            }

            info.ViewTemplateId = RevitCompatibility.GetIdValue(templateId);
            info.ViewTemplateName = SafeGetName(document.GetElement(templateId));
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadDetailAndDisplay(View view, CoreModels.ViewContextInfo info)
    {
        info.DetailLevel = SafeGet(() => view.DetailLevel.ToString());
        info.DisplayStyle = SafeGet(() => view.DisplayStyle.ToString());
    }

    private static void ReadDiscipline(View view, CoreModels.ViewContextInfo info)
    {
        try
        {
            info.Discipline = MapDiscipline(view.Discipline);
        }
        catch
        {
            // Not every view type exposes a discipline.
        }
    }

    private static void ReadAssociatedLevel(View view, CoreModels.ViewContextInfo info)
    {
        try
        {
            var level = view.GenLevel;
            if (level is null)
            {
                return;
            }

            info.AssociatedLevelId = RevitCompatibility.GetIdValue(level.Id);
            info.AssociatedLevelName = SafeGetName(level);
        }
        catch
        {
            // ignore
        }
    }

    private static CoreModels.Discipline MapDiscipline(ViewDiscipline discipline) => discipline switch
    {
        ViewDiscipline.Architectural => CoreModels.Discipline.Architectural,
        ViewDiscipline.Structural => CoreModels.Discipline.Structural,
        ViewDiscipline.Mechanical => CoreModels.Discipline.Mechanical,
        ViewDiscipline.Electrical => CoreModels.Discipline.Electrical,
        ViewDiscipline.Coordination => CoreModels.Discipline.Coordination,
        _ => CoreModels.Discipline.Unknown,
    };

    private static string? SafeGetName(Element? element)
    {
        if (element is null)
        {
            return null;
        }

        try
        {
            return element.Name;
        }
        catch
        {
            return null;
        }
    }

    private static string? SafeGet(Func<string> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    private static CoreModels.Point3D ToPoint3D(XYZ xyz) => new(xyz.X, xyz.Y, xyz.Z);
}
