using System;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Dictionary;
using RevitParameterInspector.Revit.Builders;
using RevitParameterInspector.UI;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Shared "load dictionary, build snapshot, show inspector window" workflow used by
/// <see cref="InspectSelectionOrActiveViewCommand"/>. <see cref="BuildSnapshot"/> is also the
/// rebuild path for the Reselect button (<see cref="ReselectExternalEventHandler"/>), so both
/// flows stay identical.
/// </summary>
internal static class InspectionRunner
{
    // Only the built-in dictionary tier exists today; user-override/community tiers
    // (HANDOFF Section 20.3) can be added to this list later without changing callers.
    private const string DictionaryLocale = "zh-TW";

    public static Result Run(ExternalCommandData commandData, Element element, ref string message)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var snapshot = BuildSnapshot(uiDocument, element);
            FileLogger.Log("InspectionRunner", $"BuildSnapshot done ({stopwatch.ElapsedMilliseconds} ms)");

            // ExternalEvent.Create must run in a valid API context, i.e. here in the command,
            // not later from the modeless window.
            var reselectHandler = RevitReselectRequestHandler.Create();
            var viewSheetScanHandler = RevitViewSheetScanRequestHandler.Create();

            InspectorWindowLauncher.Show(
                snapshot, commandData.Application.MainWindowHandle, reselectHandler, viewSheetScanHandler);
            FileLogger.Log("InspectionRunner", $"Run end: Succeeded ({stopwatch.ElapsedMilliseconds} ms)");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            // Inspection failures must never crash Revit (HANDOFF Section 35).
            message = $"Failed to inspect element: {ex.Message}";
            FileLogger.LogException("InspectionRunner", "Run", ex);
            return Result.Failed;
        }
    }

    /// <summary>Loads the dictionary and builds the full snapshot for <paramref name="element"/>.</summary>
    internal static CoreModels.ElementContextSnapshot BuildSnapshot(UIDocument? uiDocument, Element element)
    {
        var stopwatch = Stopwatch.StartNew();
        var dictionary = DictionaryEngine.Load(GetDictionaryDirectories(), DictionaryLocale);
        FileLogger.Log("InspectionRunner", $"DictionaryEngine.Load done ({stopwatch.ElapsedMilliseconds} ms)");
        var snapshot = ElementContextSnapshotBuilder.Build(element, dictionary.Resolver, uiDocument?.ActiveView);
        FileLogger.Log("InspectionRunner", $"ElementContextSnapshotBuilder.Build done ({stopwatch.ElapsedMilliseconds} ms)");
        return snapshot;
    }

    /// <summary>The dictionary is shipped next to the add-in assembly under a "dictionary" folder.</summary>
    private static string[] GetDictionaryDirectories()
    {
        var assemblyDirectory = Path.GetDirectoryName(typeof(InspectionRunner).Assembly.Location);
        return assemblyDirectory is null
            ? Array.Empty<string>()
            : new[] { Path.Combine(assemblyDirectory, "dictionary") };
    }
}
