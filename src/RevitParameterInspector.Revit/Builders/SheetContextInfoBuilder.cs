using System;
using System.Linq;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Builds sheet context: populated when the selected element is a ViewSheet, a Viewport, a
/// TitleBlock instance, a ScheduleSheetInstance, or any other element placed on a sheet.
/// Recursive inspection of placed views/viewports is deferred. See HANDOFF Section 18.
/// </summary>
public static class SheetContextInfoBuilder
{
    /// <summary>Returns null when the element has no meaningful sheet context.</summary>
    public static CoreModels.SheetContextInfo? Build(Element element)
    {
        var sheet = ResolveTargetSheet(element, element.Document);
        if (sheet is null)
        {
            return null;
        }

        var info = new CoreModels.SheetContextInfo
        {
            SheetId = RevitCompatibility.GetIdValue(sheet.Id),
            SheetNumber = SafeGet(() => sheet.SheetNumber),
            SheetName = SafeGetName(sheet),
        };

        ReadTitleBlock(sheet, element.Document, info);
        ReadViewportsAndViews(sheet, info);
        ReadScheduleInstances(sheet, element.Document, info);
        ReadRevisions(sheet, info);

        // PaperSizeName/PaperWidth/PaperHeight would require Document.PrintManager (which
        // mutates print settings) or a title-block-specific "Sheet Width/Height" parameter
        // that isn't standardized across families; deferred to a later step.

        return info;
    }

    private static ViewSheet? ResolveTargetSheet(Element element, Document document)
    {
        if (element is ViewSheet sheet)
        {
            return sheet;
        }

        if (element is Viewport viewport)
        {
            return SafeGetSheet(document, viewport.SheetId);
        }

        return SafeGetSheet(document, SafeGetOwnerViewId(element));
    }

    private static ViewSheet? SafeGetSheet(Document document, ElementId? viewId)
    {
        if (viewId is null || viewId == ElementId.InvalidElementId)
        {
            return null;
        }

        try
        {
            return document.GetElement(viewId) as ViewSheet;
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

    private static void ReadTitleBlock(ViewSheet sheet, Document document, CoreModels.SheetContextInfo info)
    {
        try
        {
            var titleBlock = new FilteredElementCollector(document, sheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if (titleBlock is null)
            {
                return;
            }

            info.TitleBlockElementId = RevitCompatibility.GetIdValue(titleBlock.Id);
            info.TitleBlockFamilyName = titleBlock.Symbol?.Family?.Name;
            info.TitleBlockTypeName = titleBlock.Symbol?.Name;
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadViewportsAndViews(ViewSheet sheet, CoreModels.SheetContextInfo info)
    {
        try
        {
            info.ViewportIds.AddRange(sheet.GetAllViewports().Select(RevitCompatibility.GetIdValue));
        }
        catch
        {
            // ignore
        }

        try
        {
            info.PlacedViewIds.AddRange(sheet.GetAllPlacedViews().Select(RevitCompatibility.GetIdValue));
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadScheduleInstances(ViewSheet sheet, Document document, CoreModels.SheetContextInfo info)
    {
        try
        {
            var scheduleIds = new FilteredElementCollector(document, sheet.Id)
                .OfClass(typeof(ScheduleSheetInstance))
                .Select(scheduleInstance => RevitCompatibility.GetIdValue(scheduleInstance.Id));

            info.ScheduleSheetInstanceIds.AddRange(scheduleIds);
        }
        catch
        {
            // ignore
        }
    }

    private static void ReadRevisions(ViewSheet sheet, CoreModels.SheetContextInfo info)
    {
        try
        {
            info.RevisionIds.AddRange(sheet.GetAllRevisionIds().Select(RevitCompatibility.GetIdValue));
        }
        catch
        {
            // ignore
        }
    }

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
}
