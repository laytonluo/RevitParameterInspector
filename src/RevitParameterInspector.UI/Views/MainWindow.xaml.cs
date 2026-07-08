using System.Windows;
using RevitParameterInspector.Core.Models;
using RevitParameterInspector.UI.ViewModels;

namespace RevitParameterInspector.UI.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(ElementContextSnapshot snapshot)
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel(snapshot);
        DataContext = _viewModel;
    }

    private void OnCopyJsonClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_viewModel.RawJson);
    }
}
