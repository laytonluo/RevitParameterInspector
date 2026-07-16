using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Collects the View / Sheet context rows for the inspected element
/// (HANDOFF_Update_ViewSheetContext_V1). When the element itself is a View/Sheet/Viewport,
/// only the direct relationships are reported. For any other model element, a project-wide
/// visibility scan finds every View3D/ViewPlan/ViewSection (elevations are ViewSections in
/// the API) whose collector contains the element; each such view row's AdditionalInfo shows
/// its Viewport/Sheet placement, or "N/A" when the view is not placed on any sheet.
/// </summary>
public static class ViewSheetContextReader
{
    public static List<CoreModels.ViewSheetContextItem> Read(Document document, View? activeView, Element inspectedElement)
    {
        var items = new List<CoreModels.ViewSheetContextItem>();
        var seen = new HashSet<string>();

        switch (inspectedElement)
        {
            case Viewport viewport:
                AddView(items, seen, document.GetElement(viewport.ViewId) as View);
                AddSheet(items, seen, document.GetElement(viewport.SheetId) as ViewSheet);
                break;

            case ViewSheet sheet:
                AddSheet(items, seen, sheet);
                foreach (var placedViewId in SafeGetPlacedViews(sheet))
                {
                    AddView(items, seen, document.GetElement(placedViewId) as View);
                }

                break;

            case View view when !view.IsTemplate:
                AddView(items, seen, view);
                AddSheetsContainingView(items, seen, document, view);
                break;

            default:
                AddViewsWhereElementVisible(items, seen, document, inspectedElement);
                break;
        }

        // Always add the active view (and its sheet, if placed) for any inspected element.
        // The seen-set keeps the visibility-scan row (with its placement info) when the
        // active view already appeared there.
        if (activeView is not null && !activeView.IsTemplate)
        {
            AddView(items, seen, activeView);
            AddSheetsContainingView(items, seen, document, activeView);
        }

        return items;
    }

    /// <summary>
    /// Project-wide scan: one row per non-template View3D/ViewPlan/ViewSection whose
    /// collector contains the inspected element (the classic "find all views where element
    /// is visible" pattern), plus a Sheet row for each sheet those views are placed on.
    /// </summary>
    private static void AddViewsWhereElementVisible(
        List<CoreModels.ViewSheetContextItem> items,
        HashSet<string> seen,
        Document document,
        Element element)
    {
        // Element types never appear in view collectors; skip the scan entirely.
        if (element is ElementType)
        {
            return;
        }

        try
        {
            var filter = new ElementMulticlassFilter(
                new List<Type> { typeof(View3D), typeof(ViewPlan), typeof(ViewSection) });
            var candidateViews = new FilteredElementCollector(document)
                .WherePasses(filter)
                .Cast<View>()
                .Where(view => !view.IsTemplate);

            foreach (var view in candidateViews)
            {
                if (!IsElementVisibleInView(document, view, element.Id))
                {
                    continue;
                }

                var placements = GetSheetPlacements(document, view);
                var additionalInfo = placements.Count == 0
                    ? "Viewport: N/A | Sheet: N/A"
                    : string.Join(" ; ", placements.Select(FormatPlacement));

                AddView(items, seen, view, additionalInfo);
                foreach (var placement in placements)
                {
                    AddSheet(items, seen, placement.Sheet);
                }
            }
        }
        catch
        {
            // The visibility scan is best-effort; a failure must never break inspection
            // (HANDOFF Section 7).
        }
    }

    private static bool IsElementVisibleInView(Document document, View view, ElementId elementId)
    {
        try
        {
            return new FilteredElementCollector(document, view.Id)
                .WhereElementIsNotElementType()
                .ToElementIds()
                .Contains(elementId);
        }
        catch
        {
            // Some views reject collectors (e.g. system browser-like views); treat as not visible.
            return false;
        }
    }

    private static List<(Viewport Viewport, ViewSheet Sheet)> GetSheetPlacements(Document document, View view)
    {
        var placements = new List<(Viewport, ViewSheet)>();
        try
        {
            var viewports = new FilteredElementCollector(document)
                .OfClass(typeof(Viewport))
                .Cast<Viewport>()
                .Where(viewport => viewport.ViewId == view.Id);

            foreach (var viewport in viewports)
            {
                if (document.GetElement(viewport.SheetId) is ViewSheet sheet)
                {
                    placements.Add((viewport, sheet));
                }
            }
        }
        catch
        {
            // Not finding a sheet is never an error (HANDOFF Section 7).
        }

        return placements;
    }

    private static string FormatPlacement((Viewport Viewport, ViewSheet Sheet) placement)
    {
        var viewportId = RevitCompatibility.GetIdValue(placement.Viewport.Id);
        var sheetId = RevitCompatibility.GetIdValue(placement.Sheet.Id);
        var viewportName = SafeGetName(placement.Viewport) ?? "Viewport";
        return $"Viewport: {viewportName} (ID {viewportId}) | Sheet: {BuildSheetLabel(placement.Sheet)} (ID {sheetId})";
    }

    private static void AddView(
        List<CoreModels.ViewSheetContextItem> items,
        HashSet<string> seen,
        View? view,
        string? additionalInfo = null)
    {
        if (view is null || view is ViewSheet)
        {
            return;
        }

        Add(items, seen, MapViewType(view), SafeGetName(view), view, additionalInfo);
    }

    private static void AddSheet(List<CoreModels.ViewSheetContextItem> items, HashSet<string> seen, ViewSheet? sheet)
    {
        if (sheet is null)
        {
            return;
        }

        Add(items, seen, "Sheet", BuildSheetLabel(sheet), sheet, null);
    }

    private static string? BuildSheetLabel(ViewSheet sheet) =>
        string.IsNullOrEmpty(sheet.SheetNumber)
            ? SafeGetName(sheet)
            : $"{sheet.SheetNumber} - {SafeGetName(sheet)}";

    private static void AddSheetsContainingView(
        List<CoreModels.ViewSheetContextItem> items,
        HashSet<string> seen,
        Document document,
        View view)
    {
        foreach (var placement in GetSheetPlacements(document, view))
        {
            AddSheet(items, seen, placement.Sheet);
        }
    }

    private static void Add(
        List<CoreModels.ViewSheetContextItem> items,
        HashSet<string> seen,
        string contextType,
        string? name,
        Element element,
        string? additionalInfo)
    {
        var elementId = RevitCompatibility.GetIdValue(element.Id).ToString();
        if (!seen.Add($"{contextType}|{elementId}"))
        {
            return;
        }

        items.Add(new CoreModels.ViewSheetContextItem
        {
            ContextType = contextType,
            Name = name,
            ElementId = elementId,
            UniqueId = element.UniqueId,
            AdditionalInfo = additionalInfo,
        });
    }

    private static string MapViewType(View view)
    {
        try
        {
            return view.ViewType switch
            {
                ViewType.FloorPlan => "Plan View",
                ViewType.CeilingPlan => "Plan View",
                ViewType.EngineeringPlan => "Plan View",
                ViewType.AreaPlan => "Plan View",
                ViewType.Section => "Section",
                ViewType.Elevation => "Elevation",
                ViewType.ThreeD => "3D View",
                ViewType.DraftingView => "Drafting View",
                ViewType.Legend => "Legend",
                ViewType.Schedule => "Schedule",
                _ => "View",
            };
        }
        catch
        {
            return "View";
        }
    }

    private static IEnumerable<ElementId> SafeGetPlacedViews(ViewSheet sheet)
    {
        try
        {
            return sheet.GetAllPlacedViews();
        }
        catch
        {
            return Enumerable.Empty<ElementId>();
        }
    }

    private static string? SafeGetName(Element element)
    {
        try
        {
            return element.Name;
        }
        catch
        {
            return null;
        }
    }
}
