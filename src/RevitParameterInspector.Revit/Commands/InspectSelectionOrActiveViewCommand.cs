using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Revit.Compatibility;
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
        var stopwatch = Stopwatch.StartNew();
        FileLogger.Log("InspectCommand", "Execute begin");

        var uiDocument = commandData.Application.ActiveUIDocument;
        if (uiDocument is null)
        {
            message = "No active Revit document.";
            FileLogger.Log("InspectCommand", $"Execute end: {message} ({stopwatch.ElapsedMilliseconds} ms)");
            return Result.Failed;
        }

        var (element, fromActiveView) = new SelectionReader().GetFirstSelectedOrActiveView(uiDocument);
        if (element is null)
        {
            message = "No selected element or active view is available.";
            FileLogger.Log("InspectCommand", $"Execute end: {message} ({stopwatch.ElapsedMilliseconds} ms)");
            return Result.Failed;
        }

        FileLogger.Log(
            "InspectCommand",
            $"Selected element Id={RevitCompatibility.GetIdValue(element.Id)}, "
                + $"Category={element.Category?.Name}, FromActiveView={fromActiveView}");

        var result = InspectionRunner.Run(commandData, element, ref message);
        FileLogger.Log(
            "InspectCommand",
            $"Execute end: Result={result}, Message={message} ({stopwatch.ElapsedMilliseconds} ms)");
        return result;
    }
}
