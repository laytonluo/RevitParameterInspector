using System;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitParameterInspector.Dictionary;
using RevitParameterInspector.Revit.Builders;
using RevitParameterInspector.UI;
using CoreModels = RevitParameterInspector.Core.Models;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// Shared "load dictionary, build snapshot, show inspector window" workflow used by both
/// <see cref="InspectActiveViewCommand"/> and <see cref="PickElementCommand"/>. The two
/// commands differ only in how they resolve which element to inspect (HANDOFF Section 34).
/// <see cref="BuildSnapshot"/> is also the rebuild path for the Reselect button
/// (<see cref="ReselectExternalEventHandler"/>), so both flows stay identical.
/// </summary>
internal static class InspectionRunner
{
    // Only the built-in dictionary tier exists today; user-override/community tiers
    // (HANDOFF Section 20.3) can be added to this list later without changing callers.
    private const string DictionaryLocale = "zh-TW";

    public static Result Run(ExternalCommandData commandData, Element element, ref string message)
    {
        try
        {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var snapshot = BuildSnapshot(uiDocument, element);

            // ExternalEvent.Create must run in a valid API context, i.e. here in the command,
            // not later from the modeless window.
            var reselectHandler = RevitReselectRequestHandler.Create();

            InspectorWindowLauncher.Show(snapshot, commandData.Application.MainWindowHandle, reselectHandler);
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            // Inspection failures must never crash Revit (HANDOFF Section 35).
            message = $"Failed to inspect element: {ex.Message}";
            return Result.Failed;
        }
    }

    /// <summary>Loads the dictionary and builds the full snapshot for <paramref name="element"/>.</summary>
    internal static CoreModels.ElementContextSnapshot BuildSnapshot(UIDocument? uiDocument, Element element)
    {
        var dictionary = DictionaryEngine.Load(GetDictionaryDirectories(), DictionaryLocale);
        return ElementContextSnapshotBuilder.Build(element, dictionary.Resolver, uiDocument?.ActiveView);
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
