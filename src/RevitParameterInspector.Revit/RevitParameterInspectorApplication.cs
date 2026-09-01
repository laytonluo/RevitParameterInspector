using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Revit.Commands;

namespace RevitParameterInspector.Revit;

/// <summary>
/// Registers the "ParameterInspector" ribbon panel (v0.3.0: replaces the old Add-Ins &gt;
/// External Tools command entries) with its two independent buttons - RevitParameterInspector
/// and Selection Ids List Ex.
/// </summary>
public sealed class RevitParameterInspectorApplication : IExternalApplication
{
    private const string PanelName = "ParameterInspector";

    public Result OnStartup(UIControlledApplication application)
    {
        FileLogger.Log("Application", "OnStartup begin");
        RibbonPanel panel;
        try
        {
            panel = application.CreateRibbonPanel(PanelName);
        }
        catch (Exception ex)
        {
            // Ribbon setup failures must never block Revit from starting, but they also must
            // not vanish silently - a panel with no buttons and no explanation is worse than a
            // one-time dialog (v0.3.0 shipped with a shared try/catch around both AddItem calls,
            // so one button's failure silently took the other down with it; each button now
            // fails independently and reports why).
            FileLogger.LogException("Application", "CreateRibbonPanel", ex);
            ShowStartupError("Failed to create the ParameterInspector ribbon panel", ex);
            return Result.Failed;
        }

        AddButtonSafely(panel, "RevitParameterInspector", BuildInspectButtonData);
        AddButtonSafely(panel, "Selection Ids List Ex", BuildSelectionIdsListButtonData);

        FileLogger.Log("Application", "OnStartup end");
        return Result.Succeeded;
    }

    private static void AddButtonSafely(RibbonPanel panel, string buttonLabel, Func<PushButtonData> build)
    {
        try
        {
            panel.AddItem(build());
        }
        catch (Exception ex)
        {
            FileLogger.LogException("Application", $"AddButton({buttonLabel})", ex);
            ShowStartupError($"Failed to add the \"{buttonLabel}\" button", ex);
        }
    }

    private static void ShowStartupError(string context, Exception ex) =>
        TaskDialog.Show("RevitParameterInspector", $"{context}:\n{ex}");

    public Result OnShutdown(UIControlledApplication application)
    {
        FileLogger.Log("Application", "OnShutdown");
        return Result.Succeeded;
    }

    private static PushButtonData BuildInspectButtonData()
    {
        var data = new PushButtonData(
            "RevitParameterInspector.Inspect",
            "Inspector",
            Assembly.GetExecutingAssembly().Location,
            typeof(InspectSelectionOrActiveViewCommand).FullName)
        {
            ToolTip = "RevitParameterInspector: inspect the current selection, "
                + "or the active view when nothing is selected.",
            LargeImage = LoadIcon("PARAINS.png"),
        };

        return data;
    }

    private static PushButtonData BuildSelectionIdsListButtonData()
    {
        var data = new PushButtonData(
            "RevitParameterInspector.SelectionIdsListEx",
            "IDs List Ex",
            Assembly.GetExecutingAssembly().Location,
            typeof(SelectionIdsListCommand).FullName)
        {
            ToolTip = "Selection Ids List Ex: show the active view id and selected element ids, "
                + "grouped by category, ready to copy.",
            LargeImage = LoadIcon("IDSLIST.png"),
        };

        return data;
    }

    /// <summary>Icons ship next to the add-in assembly under an "icons" folder (same convention as the dictionary).</summary>
    private static BitmapImage LoadIcon(string fileName)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = string.IsNullOrEmpty(assemblyLocation) ? null : Path.GetDirectoryName(assemblyLocation);
        if (string.IsNullOrEmpty(assemblyDirectory))
        {
            throw new InvalidOperationException(
                $"Could not resolve the add-in assembly's directory (Assembly.Location was \"{assemblyLocation}\").");
        }

        var path = Path.Combine(assemblyDirectory, "icons", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Icon file not found at \"{path}\".", path);
        }

        // Decode from bytes rather than UriSource: avoids keeping the file handle open and
        // sidesteps any Uri-kind/DPI-timing quirks with loading directly from a file path.
        var bytes = File.ReadAllBytes(path);
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
