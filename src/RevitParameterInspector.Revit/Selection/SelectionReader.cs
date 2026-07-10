using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitParameterInspector.Revit.Selection;

/// <summary>
/// Reads the current Revit selection or prompts the user to pick a single element.
/// Implements the selection workflow from HANDOFF Section 34.
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
    /// (HANDOFF_Update_Reload_CurrentContext_V1 Section 7): the first currently selected
    /// element wins; with no selection the active view is used. Returns (null, false) when
    /// neither is available.
    /// </summary>
    public (Element? Element, bool FromActiveView) GetFirstSelectedOrActiveView(UIDocument uiDocument)
    {
        var selected = GetCurrentSelection(uiDocument);
        if (selected.Count > 0)
        {
            return (selected[0], false);
        }

        return (uiDocument.ActiveView, true);
    }

    /// <summary>Prompts the user to pick one element. Returns null if the user cancels.</summary>
    public Element? PickSingleElement(UIDocument uiDocument)
    {
        try
        {
            var reference = uiDocument.Selection.PickObject(ObjectType.Element, "Select an element to inspect");
            return reference is null ? null : uiDocument.Document.GetElement(reference);
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return null;
        }
    }
}
