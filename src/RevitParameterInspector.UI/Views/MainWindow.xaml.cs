using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Win32;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Export;
using RevitParameterInspector.UI.Reselect;
using RevitParameterInspector.UI.ViewModels;

namespace RevitParameterInspector.UI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IReselectRequestHandler? _reselectHandler;

    public MainWindow(ElementContextSnapshot snapshot, IReselectRequestHandler? reselectHandler = null)
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel(snapshot);
        _reselectHandler = reselectHandler;
        DataContext = _viewModel;
    }

    private void OnReselectClick(object sender, RoutedEventArgs e)
    {
        if (_reselectHandler is null)
        {
            _viewModel.StatusMessage = "Reselect is not available in this session.";
            return;
        }

        _viewModel.StatusMessage = "Reselecting...";
        _reselectHandler.RequestReselect(OnReselectCompleted);
    }

    /// <summary>
    /// Double-click on a blue ID hyperlink (Relationships value cells, View/Sheet ElementId
    /// cells): re-inspect that element and select/zoom to it in Revit.
    /// </summary>
    private void OnElementIdLinkMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 || sender is not System.Windows.Controls.TextBlock textBlock)
        {
            return;
        }

        if (!long.TryParse(textBlock.Text, out var elementId) || elementId <= 0)
        {
            return;
        }

        e.Handled = true;
        if (_reselectHandler is null)
        {
            _viewModel.StatusMessage = "Inspect by ID is not available in this session.";
            return;
        }

        _viewModel.StatusMessage = $"Inspecting element {elementId}...";
        _reselectHandler.RequestInspectById(elementId, OnReselectCompleted);
    }

    /// <summary>
    /// Invoked by the host once the ExternalEvent has rebuilt the snapshot. Revit and this
    /// modeless window share the same STA thread, but marshal through the dispatcher anyway
    /// so a future threading change in the host can't corrupt the UI.
    /// </summary>
    private void OnReselectCompleted(ReselectResult result)
    {
        Dispatcher.Invoke(() =>
        {
            if (result.Snapshot is null)
            {
                _viewModel.StatusMessage = result.ErrorMessage ?? "Reselect failed.";
                return;
            }

            _viewModel.LoadSnapshot(result.Snapshot);
            var name = result.Snapshot.Identity?.Name ?? result.Snapshot.Identity?.ElementIdString ?? "?";
            _viewModel.StatusMessage = result.SourceType switch
            {
                ReselectSourceType.ActiveView => $"Reloaded from active view: {name}",
                ReselectSourceType.ById => $"Inspected element by id: {name}",
                _ => $"Reloaded from selected element: {name}",
            };
        });
    }

    private void OnCopyJsonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(_viewModel.BuildCopyJson());
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy JSON: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnCopyAiContextClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(_viewModel.BuildCopyAiContext());
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy AI context: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnCopyUnresolvedTermsClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(_viewModel.UnresolvedDictionaryTermsText);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy unresolved terms: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OnHyperlinkNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            // UseShellExecute opens the URL in the default system browser (no embedded web view).
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to open link: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        e.Handled = true;
    }

    private void OnExportJsonClick(object sender, RoutedEventArgs e) =>
        RunExport("json", "JSON files (*.json)|*.json", JsonExporter.ExportToFile);

    private void OnExportMarkdownClick(object sender, RoutedEventArgs e) =>
        RunExport("md", "Markdown files (*.md)|*.md", MarkdownExporter.ExportToFile);

    private void OnExportExcelClick(object sender, RoutedEventArgs e) =>
        RunExport("xlsx", "Excel files (*.xlsx)|*.xlsx", ExcelExporter.ExportToFile);

    private void RunExport(string extension, string filter, Func<ElementContextSnapshot, string, string> export)
    {
        var snapshot = _viewModel.Snapshot;
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = ExportFileNaming.BuildFileName(snapshot, extension, DateTimeOffset.UtcNow),
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var savedPath = export(snapshot, dialog.FileName);
            MessageBox.Show(this, $"Exported to:\n{savedPath}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // Export failures must never crash Revit (HANDOFF Section 35).
            MessageBox.Show(this, $"Export failed: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
