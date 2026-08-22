using System;
using System.Windows;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.UI.ViewModels;

namespace RevitParameterInspector.UI.Views;

public partial class SelectionIdsListWindow : Window
{
    private readonly SelectionIdsListViewModel _viewModel;

    public SelectionIdsListWindow(SelectionIdsSnapshot snapshot)
    {
        InitializeComponent();
        _viewModel = new SelectionIdsListViewModel(snapshot);
        DataContext = _viewModel;

        NoSelectionButtons.Visibility = _viewModel.HasSelection ? Visibility.Collapsed : Visibility.Visible;
        SelectionButtons.Visibility = _viewModel.HasSelection ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnCopyViewIdOnlyClick(object sender, RoutedEventArgs e) =>
        CopyAndClose(_viewModel.BuildIdsOnlyText());

    private void OnCopyIdsOnlyClick(object sender, RoutedEventArgs e) =>
        CopyAndClose(_viewModel.BuildIdsOnlyText());

    private void OnCopyFullTextClick(object sender, RoutedEventArgs e) =>
        CopyAndClose(_viewModel.BuildFullDisplayedText());

    private void OnCopyOstCategoryClick(object sender, RoutedEventArgs e) =>
        CopyAndClose(_viewModel.BuildOstCategoryText());

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>Every copy button copies then closes the panel; a failed copy stays open so the warning is visible.</summary>
    private void CopyAndClose(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to copy: {ex.Message}", "RevitParameterInspector", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Close();
    }
}
