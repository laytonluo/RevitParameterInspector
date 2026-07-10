using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Collects the simple, direct View / Sheet context rows for the inspected element
/// (HANDOFF_Update_ViewSheetContext_V1). Deliberately avoids any project-wide visibility
/// analysis: only the active view, the element itself when it is a View/Sheet/Viewport,
/// and direct viewport/sheet relationships are reported.
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
        }

        // Always add the active view (and its sheet, if placed) for any inspected element.
        if (activeView is not null && !activeView.IsTemplate)
        {
            AddView(items, seen, activeView);
            AddSheetsContainingView(items, seen, document, activeView);
        }

        return items;
    }

    private static void AddView(List<CoreModels.ViewSheetContextItem> items, HashSet<string> seen, View? view)
    {
        if (view is null || view is ViewSheet)
        {
            return;
        }

        Add(items, seen, MapViewType(view), SafeGetName(view), view);
    }

    private static void AddSheet(List<CoreModels.ViewSheetContextItem> items, HashSet<string> seen, ViewSheet? sheet)
    {
        if (sheet is null)
        {
            return;
        }

        var name = string.IsNullOrEmpty(sheet.SheetNumber)
            ? SafeGetName(sheet)
            : $"{sheet.SheetNumber} - {SafeGetName(sheet)}";
        Add(items, seen, "Sheet", name, sheet);
    }

    private static void AddSheetsContainingView(
        List<CoreModels.ViewSheetContextItem> items,
        HashSet<string> seen,
        Document document,
        View view)
    {
        try
        {
            var sheets = new FilteredElementCollector(document)
                .OfClass(typeof(Viewport))
                .Cast<Viewport>()
                .Where(viewport => viewport.ViewId == view.Id)
                .Select(viewport => document.GetElement(viewport.SheetId) as ViewSheet);

            foreach (var sheet in sheets)
            {
                AddSheet(items, seen, sheet);
            }
        }
        catch
        {
            // Not finding a sheet is never an error (HANDOFF Section 7).
        }
    }

    private static void Add(
        List<CoreModels.ViewSheetContextItem> items,
        HashSet<string> seen,
        string contextType,
        string? name,
        Element element)
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
