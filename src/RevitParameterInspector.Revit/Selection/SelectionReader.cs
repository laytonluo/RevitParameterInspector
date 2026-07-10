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
