using CommunityToolkit.Mvvm.ComponentModel;

namespace SpineViewer.Features.Shell.ViewModels;

/// <summary>
/// Presents the initial shell state while deeper features are still being built.
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    private string _title = "SpineViewer";
    private string _welcomeMessage = "Welcome to SpineViewer.";
    private string _statusText = "Ready.";

    /// <summary>
    /// Gets or sets the shell window title.
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets the primary empty-state message shown in the shell.
    /// </summary>
    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    /// <summary>
    /// Gets or sets the status line shown at the bottom of the shell.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
}
