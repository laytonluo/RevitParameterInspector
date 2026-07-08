using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Builders;
using RevitParameterInspector.Revit.Selection;
using RevitParameterInspector.UI;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Entry point external command implementing the selection workflow from HANDOFF Section 34:
/// use the current selection if there is exactly one element, otherwise prompt to pick one.
/// Shows the WPF inspector window (Summary/Parameters/Geometry/Location/Relationships/Raw
/// JSON). Dictionary and View/Sheet Context tabs are not included in this step.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class InspectSelectedElementCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDocument = commandData.Application.ActiveUIDocument;
        if (uiDocument is null)
        {
            message = "No active Revit document.";
            return Result.Failed;
        }

        var element = ResolveElementToInspect(uiDocument);
        if (element is null)
        {
            message = "No element was selected or picked.";
            return Result.Cancelled;
        }

        try
        {
            var snapshot = ElementContextSnapshotBuilder.Build(element);
            InspectorWindowLauncher.Show(snapshot, commandData.Application.MainWindowHandle);
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            // Inspection failures must never crash Revit (HANDOFF Section 35).
            message = $"Failed to inspect element: {ex.Message}";
            return Result.Failed;
        }
    }

    private static Element? ResolveElementToInspect(UIDocument uiDocument)
    {
        var selectionReader = new SelectionReader();
        var selected = selectionReader.GetCurrentSelection(uiDocument);

        if (selected.Count == 1)
        {
            return selected[0];
        }

        if (selected.Count > 1)
        {
            // A picker for multi-selection lists arrives with the full WPF UI (Section 34.1).
            TaskDialog.Show(
                "RevitParameterInspector",
                "Multiple elements are selected. Inspecting the first selected element for now.");
            return selected[0];
        }

        return selectionReader.PickSingleElement(uiDocument);
    }
}
