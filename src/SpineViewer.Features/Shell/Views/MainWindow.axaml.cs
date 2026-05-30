using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpineViewer.Features.Shell.ViewModels;

namespace SpineViewer.Features.Shell.Views;

/// <summary>
/// Hosts the initial shell window.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The shell view model for the window.</param>
    public MainWindow(MainWindowViewModel viewModel)
    {
        DataContext = viewModel;
        AvaloniaXamlLoader.Load(this);
    }
}
