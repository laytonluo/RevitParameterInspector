using System;

namespace RevitParameterInspector.UI.ViewSheetScan;

/// <summary>
/// Bridge the inspector window uses to ask its host to run the deferred project-wide
/// "which views is this element visible in" scan (the View / Sheet Context tab's Scan
/// button). Defined here, Revit-agnostically, so the UI project stays free of Revit API
/// types; the Revit add-in supplies an ExternalEvent-backed implementation because the
/// window is modeless and must not touch the Revit API from its own event handlers.
/// </summary>
public interface IViewSheetScanRequestHandler
{
    /// <summary>
    /// Requests the scan for the element with the given id. Returns immediately;
    /// <paramref name="onComplete"/> is invoked later (on the UI thread) once the host has
    /// run the scan or failed.
    /// </summary>
    void RequestScan(long elementId, Action<ViewSheetScanResult> onComplete);
}
