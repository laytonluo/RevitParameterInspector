using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Compatibility;

namespace RevitParameterInspector.Revit.Selection;

/// <summary>
/// Reads the current Revit selection. Implements the selection workflow from HANDOFF Section 34.
/// </summary>
public sealed class SelectionReader
{
    public IList<Element> GetCurrentSelection(UIDocument uiDocument)
    {
        var document = uiDocument.Document;
        return uiDocument.Selection.GetElementIds()
            .Select(document.GetElement)
            .Where(element => element is not null)
            .ToList()!;
    }

    /// <summary>
    /// Resolves the element to inspect for the Reselect workflow
    /// (HANDOFF_Update_Reload_CurrentContext_V1 Section 7): with a multi-selection, the
    /// element with the lowest ElementId wins (sorted ascending, first taken) so the result
    /// is deterministic regardless of click/selection order; with no selection the active
    /// view is used. Returns (null, false) when neither is available.
    /// </summary>
    public (Element? Element, bool FromActiveView) GetFirstSelectedOrActiveView(UIDocument uiDocument)
    {
        var selected = GetCurrentSelection(uiDocument);
        if (selected.Count > 0)
        {
            var first = selected.OrderBy(element => RevitCompatibility.GetIdValue(element.Id)).First();
            return (first, false);
        }

        return (uiDocument.ActiveView, true);
    }
}
