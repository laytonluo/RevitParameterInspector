using System;
using System.Diagnostics;
using Autodesk.Revit.UI;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Revit.Builders;
using RevitParameterInspector.Revit.Compatibility;
using RevitParameterInspector.UI.ViewSheetScan;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// The ExternalEvent side of the on-demand View / Sheet Context project-wide scan (the Scan
/// button). Deferred from <see cref="InspectionRunner"/>/Reselect because
/// <see cref="Builders.ViewSheetContextReader.ScanProjectWide"/> can take minutes on a project
/// with many views - it must only run when the user explicitly asks for it from the tab, and
/// like <see cref="ReselectExternalEventHandler"/>, must run in a valid API context rather
/// than directly from the modeless window.
/// </summary>
public sealed class ViewSheetScanExternalEventHandler : IExternalEventHandler
{
    private Action<ViewSheetScanResult>? _pendingCallback;
    private long _pendingElementId;

    public void SetPendingRequest(long elementId, Action<ViewSheetScanResult> onComplete)
    {
        _pendingElementId = elementId;
        _pendingCallback = onComplete;
    }

    public void Execute(UIApplication app)
    {
        var stopwatch = Stopwatch.StartNew();
        FileLogger.Log("ViewSheetScanHandler", "Execute begin");

        var callback = _pendingCallback;
        var elementId = _pendingElementId;
        _pendingCallback = null;
        if (callback is null)
        {
            FileLogger.Log("ViewSheetScanHandler", "Execute end: no pending callback");
            return;
        }

        var result = new ViewSheetScanResult();
        try
        {
            var document = app.ActiveUIDocument?.Document;
            if (document is null)
            {
                result.ErrorMessage = "No active Revit document.";
            }
            else
            {
                var element = document.GetElement(RevitCompatibility.CreateElementId(elementId));
                if (element is null)
                {
                    result.ErrorMessage = $"Element {elementId} was not found in the active document.";
                }
                else
                {
                    result.Items = ViewSheetContextReader.ScanProjectWide(document, element);
                }
            }
        }
        catch (Exception ex)
        {
            // Scan failures must never crash Revit (HANDOFF Section 35).
            result.ErrorMessage = $"View / Sheet scan failed: {ex.Message}";
            FileLogger.LogException("ViewSheetScanHandler", "Execute", ex);
        }

        FileLogger.Log(
            "ViewSheetScanHandler",
            $"Execute end: ItemCount={result.Items?.Count}, Error={result.ErrorMessage} ({stopwatch.ElapsedMilliseconds} ms)");

        callback(result);
    }

    public string GetName() => "RevitParameterInspector ViewSheetScan";
}
