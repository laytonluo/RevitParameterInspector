using System;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Selection;
using RevitParameterInspector.UI.Reselect;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// The ExternalEvent side of the Reselect workflow. The modeless inspector window must not
/// touch the Revit API from its own event handlers, so <see cref="RevitReselectRequestHandler"/>
/// parks the request here and raises the event; Revit then calls <see cref="Execute"/> in a
/// valid API context on its next idle cycle (HANDOFF_Update_Reload_CurrentContext_V1 Section 10).
/// </summary>
public sealed class ReselectExternalEventHandler : IExternalEventHandler
{
    private Action<ReselectResult>? _pendingCallback;

    public void SetPendingCallback(Action<ReselectResult> onComplete) => _pendingCallback = onComplete;

    public void Execute(UIApplication app)
    {
        var callback = _pendingCallback;
        _pendingCallback = null;
        if (callback is null)
        {
            return;
        }

        var result = new ReselectResult();
        try
        {
            var uiDocument = app.ActiveUIDocument;
            if (uiDocument is null)
            {
                result.ErrorMessage = "No active Revit document.";
            }
            else
            {
                var (element, fromActiveView) = new SelectionReader().GetFirstSelectedOrActiveView(uiDocument);
                if (element is null)
                {
                    result.ErrorMessage = "No selected element or active view is available.";
                }
                else
                {
                    result.Snapshot = InspectionRunner.BuildSnapshot(uiDocument, element);
                    result.SourceType = fromActiveView
                        ? ReselectSourceType.ActiveView
                        : ReselectSourceType.SelectedElement;
                }
            }
        }
        catch (Exception ex)
        {
            // Reselect failures must never crash Revit (HANDOFF Section 35).
            result.Snapshot = null;
            result.SourceType = ReselectSourceType.None;
            result.ErrorMessage = $"Reselect failed: {ex.Message}";
        }

        // Revit and the modeless WPF window share the same STA thread, so the callback may
        // touch the UI directly here.
        callback(result);
    }

    public string GetName() => "RevitParameterInspector Reselect";
}
