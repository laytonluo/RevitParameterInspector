using System;
using Autodesk.Revit.UI;
using RevitParameterInspector.UI.ViewSheetScan;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// The <see cref="IViewSheetScanRequestHandler"/> implementation handed to the inspector
/// window: wraps an <see cref="ExternalEvent"/> around
/// <see cref="ViewSheetScanExternalEventHandler"/>. Must be created via <see cref="Create"/>
/// inside a valid Revit API context (an external command's Execute), because
/// ExternalEvent.Create requires one.
/// </summary>
public sealed class RevitViewSheetScanRequestHandler : IViewSheetScanRequestHandler
{
    private readonly ViewSheetScanExternalEventHandler _handler;
    private readonly ExternalEvent _externalEvent;

    private RevitViewSheetScanRequestHandler(ViewSheetScanExternalEventHandler handler, ExternalEvent externalEvent)
    {
        _handler = handler;
        _externalEvent = externalEvent;
    }

    public static RevitViewSheetScanRequestHandler Create()
    {
        var handler = new ViewSheetScanExternalEventHandler();
        return new RevitViewSheetScanRequestHandler(handler, ExternalEvent.Create(handler));
    }

    public void RequestScan(long elementId, Action<ViewSheetScanResult> onComplete)
    {
        _handler.SetPendingRequest(elementId, onComplete);
        _externalEvent.Raise();
    }
}
