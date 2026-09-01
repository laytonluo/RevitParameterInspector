using System;
using System.Windows.Interop;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.UI.Reselect;
using RevitParameterInspector.UI.Views;
using RevitParameterInspector.UI.ViewSheetScan;

namespace RevitParameterInspector.UI;

/// <summary>
/// Entry point for hosts (the Revit add-in) to show the inspector window without needing
/// to reference WPF types themselves.
/// </summary>
public static class InspectorWindowLauncher
{
    /// <param name="reselectHandler">
    /// Optional host bridge for the Reselect button; when null the button reports that
    /// reselect is unavailable instead of crashing.
    /// </param>
    /// <param name="viewSheetScanHandler">
    /// Optional host bridge for the View / Sheet Context tab's Scan button; when null the
    /// button reports that the scan is unavailable instead of crashing.
    /// </param>
    public static void Show(
        ElementContextSnapshot snapshot,
        IntPtr ownerHandle = default,
        IReselectRequestHandler? reselectHandler = null,
        IViewSheetScanRequestHandler? viewSheetScanHandler = null)
    {
        FileLogger.Log("InspectorWindowLauncher", "Show begin");
        var window = new MainWindow(snapshot, reselectHandler, viewSheetScanHandler);

        if (ownerHandle != IntPtr.Zero)
        {
            _ = new WindowInteropHelper(window) { Owner = ownerHandle };
        }

        window.Show();
        FileLogger.Log("InspectorWindowLauncher", "Show end");
    }
}
