using System;
using System.Windows;
using Microsoft.Win32;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.Export;
using RevitParameterInspector.UI.ViewModels;

namespace RevitParameterInspector.UI.Views;

public partial class MainWindow : Window
{
    private readonly ElementContextSnapshot _snapshot;
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(ElementContextSnapshot snapshot)
    {
        InitializeComponent();
        _snapshot = snapshot;
        _viewModel = new MainWindowViewModel(snapshot);
        DataContext = _viewModel;
    }

    private void OnCopyJsonClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_viewModel.RawJson);
    }

    private void OnExportJsonClick(object sender, RoutedEventArgs e) =>
        RunExport("json", "JSON files (*.json)|*.json", JsonExporter.ExportToFile);

    private void OnExportMarkdownClick(object sender, RoutedEventArgs e) =>
        RunExport("md", "Markdown files (*.md)|*.md", MarkdownExporter.ExportToFile);

    private void OnExportExcelClick(object sender, RoutedEventArgs e) =>
        RunExport("xlsx", "Excel files (*.xlsx)|*.xlsx", ExcelExporter.ExportToFile);

    private void OnCopyAiContextClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(AiContextComposer.Build(_snapshot));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy AI context: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RunExport(string extension, string filter, Func<ElementContextSnapshot, string, string> export)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = ExportFileNaming.BuildFileName(_snapshot, extension, DateTimeOffset.UtcNow),
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var savedPath = export(_snapshot, dialog.FileName);
            MessageBox.Show(this, $"Exported to:\n{savedPath}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // Export failures must never crash Revit (HANDOFF Section 35).
            MessageBox.Show(this, $"Export failed: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
