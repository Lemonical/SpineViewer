using Avalonia;
using Avalonia.Styling;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.App.Startup;

/// <summary>
/// Applies viewer theme preferences to the running Avalonia application instance.
/// </summary>
public sealed class AvaloniaApplicationThemeService : IApplicationThemeService
{
    private readonly IUiDispatcher _uiDispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaApplicationThemeService"/> class.
    /// </summary>
    /// <param name="uiDispatcher">The dispatcher used for UI-thread property updates.</param>
    public AvaloniaApplicationThemeService(IUiDispatcher uiDispatcher)
    {
        _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
    }

    /// <inheritdoc />
    public void ApplyTheme(ViewerTheme theme)
    {
        Application? application = Application.Current;
        if (application is null)
        {
            return;
        }

        ThemeVariant variant = theme switch
        {
            ViewerTheme.Light => ThemeVariant.Light,
            ViewerTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };

        // Theme changes can be requested from settings-save continuations that run off the UI thread.
        _uiDispatcher.Invoke(() => application.RequestedThemeVariant = variant);
    }
}
