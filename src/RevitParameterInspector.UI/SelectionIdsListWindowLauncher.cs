using System;
using System.Windows.Interop;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.UI.Views;

namespace RevitParameterInspector.UI;

/// <summary>
/// Entry point for hosts (the Revit add-in) to show the "Selection Ids List Ex" panel without
/// needing to reference WPF types themselves. Mirrors <see cref="InspectorWindowLauncher"/>.
/// </summary>
public static class SelectionIdsListWindowLauncher
{
    public static void Show(SelectionIdsSnapshot snapshot, IntPtr ownerHandle = default)
    {
        var window = new SelectionIdsListWindow(snapshot);

        if (ownerHandle != IntPtr.Zero)
        {
            _ = new WindowInteropHelper(window) { Owner = ownerHandle };
        }

        window.Show();
    }
}
