using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using RevitParameterInspector.Revit.Compatibility;
using RevitParameterInspector.Revit.Selection;
using RevitParameterInspector.UI.Reselect;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// The ExternalEvent side of the Reselect workflow. The modeless inspector window must not
/// touch the Revit API from its own event handlers, so <see cref="RevitReselectRequestHandler"/>
/// parks the request here and raises the event; Revit then calls <see cref="Execute"/> in a
/// valid API context on its next idle cycle (HANDOFF_Update_Reload_CurrentContext_V1 Section 10).
/// Also carries the "inspect by ElementId" requests behind the blue ID hyperlinks: those
/// additionally select the target element in Revit before rebuilding the snapshot.
/// </summary>
public sealed class ReselectExternalEventHandler : IExternalEventHandler
{
    private Action<ReselectResult>? _pendingCallback;
    private long? _pendingElementId;

    public void SetPendingCallback(Action<ReselectResult> onComplete)
    {
        _pendingCallback = onComplete;
        _pendingElementId = null;
    }

    public void SetPendingInspectById(long elementId, Action<ReselectResult> onComplete)
    {
        _pendingCallback = onComplete;
        _pendingElementId = elementId;
    }

    public void Execute(UIApplication app)
    {
        var callback = _pendingCallback;
        var elementId = _pendingElementId;
        _pendingCallback = null;
        _pendingElementId = null;
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
            else if (elementId is long id)
            {
                InspectById(uiDocument, id, result);
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

    private static void InspectById(UIDocument uiDocument, long id, ReselectResult result)
    {
        var revitId = RevitCompatibility.CreateElementId(id);
        var element = uiDocument.Document.GetElement(revitId);
        if (element is null)
        {
            result.ErrorMessage = $"Element {id} was not found in the active document.";
            return;
        }

        // Select only, no ShowElements zoom: linked ids are mostly views/types/other
        // non-physical elements, where zooming is meaningless or throws. Best-effort,
        // must not block the inspect.
        try
        {
            uiDocument.Selection.SetElementIds(new List<Autodesk.Revit.DB.ElementId> { revitId });
        }
        catch
        {
            // Selection failures are cosmetic only.
        }

        result.Snapshot = InspectionRunner.BuildSnapshot(uiDocument, element);
        result.SourceType = ReselectSourceType.ById;
    }

    public string GetName() => "RevitParameterInspector Reselect";
}
