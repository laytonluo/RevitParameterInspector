using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Dictionary;
using RevitParameterInspector.Revit.Builders;
using RevitParameterInspector.Revit.Selection;
using RevitParameterInspector.UI;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Entry point external command implementing the selection workflow from HANDOFF Section 34:
/// use the current selection if there is exactly one element, otherwise prompt to pick one.
/// Shows the WPF inspector window (Summary/Parameters/Geometry/Location/Relationships/
/// View-Sheet Context/Raw JSON). The Dictionary tab is not included in this step.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
[Regeneration(RegenerationOption.Manual)]
public sealed class InspectSelectedElementCommand : IExternalCommand
{
    // Only the built-in dictionary tier exists today; user-override/community tiers
    // (HANDOFF Section 20.3) can be added to this list later without changing callers.
    private const string DictionaryLocale = "zh-TW";

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
            var dictionary = DictionaryEngine.Load(GetDictionaryDirectories(), DictionaryLocale);
            var snapshot = ElementContextSnapshotBuilder.Build(element, dictionary.Resolver);
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

    /// <summary>The dictionary is shipped next to the add-in assembly under a "dictionary" folder.</summary>
    private static string[] GetDictionaryDirectories()
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(InspectSelectedElementCommand).Assembly.Location);
        return assemblyDirectory is null
            ? Array.Empty<string>()
            : new[] { Path.Combine(assemblyDirectory, "dictionary") };
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
