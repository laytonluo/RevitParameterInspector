using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Builders;
using RevitParameterInspector.UI;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Entry point for the "Selection Ids List Ex" ribbon button: shows the active view id and,
/// when something is selected, the selected element ids grouped by category - a lightweight
/// copy-friendly panel distinct from the full parameter inspector window.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class SelectionIdsListCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDocument = commandData.Application.ActiveUIDocument;
        if (uiDocument is null)
        {
            message = "No active Revit document.";
            return Result.Failed;
        }

        try
        {
            var snapshot = SelectionIdsSnapshotBuilder.Build(uiDocument);
            SelectionIdsListWindowLauncher.Show(snapshot, commandData.Application.MainWindowHandle);
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = $"Failed to build selection ids list: {ex.Message}";
            return Result.Failed;
        }
    }
}
