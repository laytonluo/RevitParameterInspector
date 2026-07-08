using System;
using System.Windows.Interop;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.UI.Views;

namespace RevitParameterInspector.UI;

/// <summary>
/// Entry point for hosts (the Revit add-in) to show the inspector window without needing
/// to reference WPF types themselves.
/// </summary>
public static class InspectorWindowLauncher
{
    public static void Show(ElementContextSnapshot snapshot, IntPtr ownerHandle = default)
    {
        var window = new MainWindow(snapshot);

        if (ownerHandle != IntPtr.Zero)
        {
            _ = new WindowInteropHelper(window) { Owner = ownerHandle };
        }

        window.Show();
    }
}
