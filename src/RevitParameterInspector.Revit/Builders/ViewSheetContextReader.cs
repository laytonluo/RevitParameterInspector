using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Revit.Compatibility;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Builders;

/// <summary>
/// Collects the View / Sheet context rows for the inspected element
/// (HANDOFF_Update_ViewSheetContext_V1). When the element itself is a View/Sheet/Viewport, or
/// has a single owner view (2D annotation elements - tags, dimensions, detail items, etc. -
/// only ever appear on the one view that owns them), only that direct relationship is
/// reported and <see cref="Read"/> resolves it immediately. Any other element (ordinary model
/// elements like walls/columns/beams, which can appear in many views) has no single owner
/// view to look up, so <see cref="Read"/> sets <paramref name="scanPending"/> instead of
/// running the expensive project-wide scan itself: finding every View3D/ViewPlan/ViewSection
/// (elevations are ViewSections in the API) whose collector contains the element requires
/// constructing a view-scoped <see cref="FilteredElementCollector"/> per view, which forces
/// Revit to regenerate each view that hasn't been opened yet - on a project with many views
/// this can take minutes and looks like Revit is stuck repainting. That scan is
/// <see cref="ScanProjectWide"/>, run only on demand (the View / Sheet Context tab's Scan
/// button), never automatically on every inspect/reselect.
/// </summary>
public static class ViewSheetContextReader
{
    public static List<CoreModels.ViewSheetContextItem> Read(
        Document document, View? activeView, Element inspectedElement, out bool scanPending)
    {
        var items = new List<CoreModels.ViewSheetContextItem>();
        var seen = new HashSet<string>();
        scanPending = false;

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
                if (TryGetOwnerView(document, inspectedElement) is { } ownerView)
                {
                    AddView(items, seen, ownerView, "Owner View");
                    AddSheetsContainingView(items, seen, document, ownerView);
                }
                else if (inspectedElement is not ElementType)
                {
                    // Element types never appear in view collectors - no scan needed for them.
                    scanPending = true;
                }

                break;
        }

        // Always add the active view (and its sheet, if placed) for any inspected element.
        // The seen-set keeps the owner-view row (with its placement info) when the active
        // view already appeared there.
        if (activeView is not null && !activeView.IsTemplate)
        {
            AddView(items, seen, activeView);
            AddSheetsContainingView(items, seen, document, activeView);
        }

        return items;
    }

    /// <summary>
    /// The expensive project-wide "which views is this element visible in" scan, deferred
    /// from <see cref="Read"/> (see the class summary). Call this only on demand - e.g. from
    /// the View / Sheet Context tab's Scan button via an ExternalEvent - never automatically
    /// from the inspect/reselect flow.
    /// </summary>
    public static List<CoreModels.ViewSheetContextItem> ScanProjectWide(Document document, Element element)
    {
        var items = new List<CoreModels.ViewSheetContextItem>();
        var seen = new HashSet<string>();
        AddViewsWhereElementVisible(items, seen, document, element);
        return items;
    }

    /// <summary>
    /// 2D annotation elements (tags, dimensions, detail items, filled regions, text notes...)
    /// only ever appear on the one view that owns them - <see cref="Element.OwnerViewId"/>
    /// resolves that directly without any project-wide scan. Ordinary model elements (walls,
    /// columns, beams...) have no owner view (<see cref="ElementId.InvalidElementId"/>) and
    /// fall through to the deferred <see cref="ScanProjectWide"/> path instead.
    /// </summary>
    private static View? TryGetOwnerView(Document document, Element element)
    {
        try
        {
            var ownerViewId = element.OwnerViewId;
            if (ownerViewId == ElementId.InvalidElementId)
            {
                return null;
            }

            return document.GetElement(ownerViewId) is View view && !view.IsTemplate ? view : null;
        }
        catch
        {
            return null;
        }
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

        var totalStopwatch = Stopwatch.StartNew();
        var msByViewType = new Dictionary<string, long>();
        var countByViewType = new Dictionary<string, int>();
        var visibleCount = 0;

        try
        {
            var filter = new ElementMulticlassFilter(
                new List<Type> { typeof(View3D), typeof(ViewPlan), typeof(ViewSection) });
            var candidateViews = new FilteredElementCollector(document)
                .WherePasses(filter)
                .Cast<View>()
                .Where(view => !view.IsTemplate)
                .ToList();

            FileLogger.Log("ViewSheetContextReader", $"AddViewsWhereElementVisible: candidate view count={candidateViews.Count}");

            foreach (var view in candidateViews)
            {
                var typeKey = view.GetType().Name;
                var viewStopwatch = Stopwatch.StartNew();
                var isVisible = IsElementVisibleInView(document, view, element.Id);
                viewStopwatch.Stop();

                msByViewType.TryGetValue(typeKey, out var existingMs);
                msByViewType[typeKey] = existingMs + viewStopwatch.ElapsedMilliseconds;
                countByViewType.TryGetValue(typeKey, out var existingCount);
                countByViewType[typeKey] = existingCount + 1;
                if (viewStopwatch.ElapsedMilliseconds > 200)
                {
                    FileLogger.Log(
                        "ViewSheetContextReader",
                        $"Slow view check: Type={typeKey}, Name={SafeGetName(view)}, "
                            + $"Id={RevitCompatibility.GetIdValue(view.Id)}, Elapsed={viewStopwatch.ElapsedMilliseconds} ms");
                }

                if (!isVisible)
                {
                    continue;
                }

                visibleCount++;
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
        catch (Exception ex)
        {
            // The visibility scan is best-effort; a failure must never break inspection
            // (HANDOFF Section 7).
            FileLogger.LogException("ViewSheetContextReader", "AddViewsWhereElementVisible", ex);
        }
        finally
        {
            var breakdown = string.Join(
                ", ",
                msByViewType.Select(kvp => $"{kvp.Key}: count={countByViewType[kvp.Key]}, totalMs={kvp.Value}"));
            FileLogger.Log(
                "ViewSheetContextReader",
                $"AddViewsWhereElementVisible done: visibleCount={visibleCount}, "
                    + $"totalElapsed={totalStopwatch.ElapsedMilliseconds} ms, breakdown=[{breakdown}]");
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
