using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Win32;
using RevitParameterInspector.Core.Logging;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Export;
using RevitParameterInspector.UI.Reselect;
using RevitParameterInspector.UI.ViewModels;
using RevitParameterInspector.UI.ViewSheetScan;

namespace RevitParameterInspector.UI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IReselectRequestHandler? _reselectHandler;
    private readonly IViewSheetScanRequestHandler? _viewSheetScanHandler;

    public MainWindow(
        ElementContextSnapshot snapshot,
        IReselectRequestHandler? reselectHandler = null,
        IViewSheetScanRequestHandler? viewSheetScanHandler = null)
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel(snapshot);
        _reselectHandler = reselectHandler;
        _viewSheetScanHandler = viewSheetScanHandler;
        DataContext = _viewModel;

        Loaded += (_, _) => FileLogger.Log("MainWindow", "Loaded");
        Closed += (_, _) => FileLogger.Log("MainWindow", "Closed");
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

    /// <summary>
    /// The View / Sheet Context tab's Scan button: runs the deferred project-wide "which
    /// views is this element visible in" scan on demand, since it can take minutes on a
    /// project with many views and must never run automatically (see
    /// RevitParameterInspector.Revit's ViewSheetContextReader class summary).
    /// </summary>
    private void OnScanViewSheetClick(object sender, RoutedEventArgs e)
    {
        if (_viewSheetScanHandler is null)
        {
            _viewModel.CompleteViewSheetScan(null, "View / Sheet scan is not available in this session.");
            return;
        }

        var elementId = _viewModel.Snapshot.Identity?.ElementId ?? 0;
        if (elementId <= 0)
        {
            _viewModel.CompleteViewSheetScan(null, "No element id available to scan.");
            return;
        }

        _viewModel.BeginViewSheetScan();
        _viewSheetScanHandler.RequestScan(elementId, OnViewSheetScanCompleted);
    }

    private void OnViewSheetScanCompleted(ViewSheetScanResult result)
    {
        Dispatcher.Invoke(() => _viewModel.CompleteViewSheetScan(result.Items, result.ErrorMessage));
    }

    /// <summary>
    /// Tracks whether the View / Sheet Context tab is currently selected, so the Scan button
    /// (placed next to Reselect, not inside the tab) only enables while that tab is showing.
    /// TabControl.SelectionChanged bubbles up from any nested Selector (e.g. the Parameters
    /// or Dictionary DataGrid's own row-selection events use the same routed event), so
    /// e.Source must be checked against the TabControl itself, not just any sender.
    /// </summary>
    private void OnMainTabControlSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (e.Source is not System.Windows.Controls.TabControl tabControl)
        {
            return;
        }

        var isViewSheetContextTab = tabControl.SelectedItem is System.Windows.Controls.TabItem tabItem
            && Equals(tabItem.Header, "View / Sheet Context");
        _viewModel.SetViewSheetContextTabActive(isViewSheetContextTab);
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

    /// <summary>
    /// Refreshes the Dictionary tab's selection preview. Uses DictionaryTerms (grid display
    /// order) filtered by SelectedItems rather than SelectedItems directly, since
    /// DataGrid.SelectedItems does not preserve display order.
    /// </summary>
    private void OnDictionaryTermsSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var selected = new HashSet<DictionaryTermInfo>(DictionaryTermsGrid.SelectedItems.Cast<DictionaryTermInfo>());
        var orderedSelection = _viewModel.DictionaryTerms.Where(selected.Contains);
        _viewModel.UpdateSelectedDictionaryTerms(orderedSelection);
    }

    private void OnCopyIdAndApiNameClick(object sender, RoutedEventArgs e) =>
        CopySelectedDictionaryNames("API Name", _viewModel.SelectedDictionaryApiNames);

    private void OnCopyIdAndLocalizedNameClick(object sender, RoutedEventArgs e) =>
        CopySelectedDictionaryNames("Localized Name", _viewModel.SelectedDictionaryLocalizedNames);

    private void CopySelectedDictionaryNames(string label, IReadOnlyCollection<string> names)
    {
        if (names.Count == 0)
        {
            _viewModel.StatusMessage = "No dictionary rows selected.";
            return;
        }

        try
        {
            Clipboard.SetText(_viewModel.BuildDictionaryCopyText(label, names));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy {label}: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
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
