using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Selection;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Entry point external command for HANDOFF Section 34.2's dedicated "Pick Element" workflow:
/// always prompts the user to pick a single element, ignoring the current selection (unlike
/// <see cref="InspectSelectedElementCommand"/>, which prefers the current selection when there
/// is exactly one element).
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class PickElementCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDocument = commandData.Application.ActiveUIDocument;
        if (uiDocument is null)
        {
            message = "No active Revit document.";
            return Result.Failed;
        }

        var element = new SelectionReader().PickSingleElement(uiDocument);
        if (element is null)
        {
            message = "No element was picked.";
            return Result.Cancelled;
        }

        return InspectionRunner.Run(commandData, element, ref message);
    }
}
