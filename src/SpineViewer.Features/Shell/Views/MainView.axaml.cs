using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SpineViewer.Features.Shell.ViewModels;

namespace SpineViewer.Features.Shell.Views;

/// <summary>
/// Hosts the browser single-view shell surface.
/// </summary>
public partial class MainView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class.
    /// </summary>
    public MainView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainView"/> class with its shell view model.
    /// </summary>
    /// <param name="viewModel">The shell view model for the browser surface.</param>
    public MainView(MainWindowViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }
}
