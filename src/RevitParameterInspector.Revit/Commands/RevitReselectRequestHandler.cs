using System;
using Autodesk.Revit.UI;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.UI.Reselect;

namespace RevitParameterInspector.Revit.Commands;

/// <summary>
/// The <see cref="IReselectRequestHandler"/> implementation handed to the inspector window:
/// wraps an <see cref="ExternalEvent"/> around <see cref="ReselectExternalEventHandler"/>.
/// Must be created via <see cref="Create"/> inside a valid Revit API context (an external
/// command's Execute), because ExternalEvent.Create requires one.
/// </summary>
public sealed class RevitReselectRequestHandler : IReselectRequestHandler
{
    private readonly ReselectExternalEventHandler _handler;
    private readonly ExternalEvent _externalEvent;

    private RevitReselectRequestHandler(ReselectExternalEventHandler handler, ExternalEvent externalEvent)
    {
        _handler = handler;
        _externalEvent = externalEvent;
    }

    public static RevitReselectRequestHandler Create()
    {
        var handler = new ReselectExternalEventHandler();
        return new RevitReselectRequestHandler(handler, ExternalEvent.Create(handler));
    }

    public void RequestReselect(Action<ReselectResult> onComplete)
    {
        FileLogger.Log("ReselectRequest", "RequestReselect: raising external event");
        _handler.SetPendingCallback(onComplete);
        _externalEvent.Raise();
    }

    public void RequestInspectById(long elementId, Action<ReselectResult> onComplete)
    {
        FileLogger.Log("ReselectRequest", $"RequestInspectById({elementId}): raising external event");
        _handler.SetPendingInspectById(elementId, onComplete);
        _externalEvent.Raise();
    }
}
