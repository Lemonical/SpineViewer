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
    /// <inheritdoc />
    public void ApplyTheme(ViewerTheme theme)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = theme switch
        {
            ViewerTheme.Light => ThemeVariant.Light,
            ViewerTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }
}
