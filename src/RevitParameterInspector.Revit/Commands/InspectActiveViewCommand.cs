using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Entry point external command "RevitParameterInspector - Active View": inspects the
/// current active view directly, without looking at the selection. Use the Reselect button
/// in the inspector window to switch to a selected element afterwards, or
/// <see cref="PickElementCommand"/> to pick one explicitly.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class InspectActiveViewCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDocument = commandData.Application.ActiveUIDocument;
        if (uiDocument is null)
        {
            message = "No active Revit document.";
            return Result.Failed;
        }

        var activeView = uiDocument.ActiveView;
        if (activeView is null)
        {
            message = "No active view is available.";
            return Result.Failed;
        }

        return InspectionRunner.Run(commandData, activeView, ref message);
    }
}
