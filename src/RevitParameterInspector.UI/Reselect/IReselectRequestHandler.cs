using System;

namespace RevitParameterInspector.UI.Reselect;

/// <summary>
/// Bridge the inspector window uses to ask its host to re-read the current Revit selection
/// (or active view) and rebuild the snapshot. Defined here, Revit-agnostically, so the UI
/// project stays free of Revit API types; the Revit add-in supplies an ExternalEvent-backed
/// implementation because the window is modeless (HANDOFF_Update_Reload_CurrentContext_V1
/// Section 10).
/// </summary>
public interface IReselectRequestHandler
{
    /// <summary>
    /// Requests a reselect. Returns immediately; <paramref name="onComplete"/> is invoked
    /// later (on the UI thread) once the host has rebuilt the snapshot or failed.
    /// </summary>
    void RequestReselect(Action<ReselectResult> onComplete);

    /// <summary>
    /// Requests an inspect of the element with the given id (the "research by ID" flow
    /// behind the blue ID hyperlinks). The host selects the element in Revit, rebuilds the
    /// snapshot, and invokes <paramref name="onComplete"/> on the UI thread.
    /// </summary>
    void RequestInspectById(long elementId, Action<ReselectResult> onComplete);
}
