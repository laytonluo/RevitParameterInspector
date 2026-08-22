using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Selection;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// The single ribbon entry point for RevitParameterInspector (v0.3.0 merges what used to be
/// two separate commands/buttons - see <see cref="ReselectExternalEventHandler"/>'s identical
/// logic for the Reselect button): inspects the current selection's first element when
/// something is selected, otherwise falls back to the active view.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class InspectSelectionOrActiveViewCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDocument = commandData.Application.ActiveUIDocument;
        if (uiDocument is null)
        {
            message = "No active Revit document.";
            return Result.Failed;
        }

        var (element, _) = new SelectionReader().GetFirstSelectedOrActiveView(uiDocument);
        if (element is null)
        {
            message = "No selected element or active view is available.";
            return Result.Failed;
        }

        return InspectionRunner.Run(commandData, element, ref message);
    }
}
