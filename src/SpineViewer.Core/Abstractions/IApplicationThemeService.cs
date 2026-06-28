using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Applies persisted viewer theme preferences to the active application host.
/// </summary>
public interface IApplicationThemeService
{
    /// <summary>
    /// Applies the supplied viewer theme preference to the active application host.
    /// </summary>
    /// <param name="theme">The theme preference to apply.</param>
    void ApplyTheme(ViewerTheme theme);
}
