using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Loads and saves persisted viewer-level settings.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Loads the persisted viewer settings.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the load operation.</param>
    /// <returns>The persisted settings, or defaults when nothing has been saved yet.</returns>
    Task<ViewerSettings> LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the viewer settings.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    /// <param name="cancellationToken">A token that cancels the save operation.</param>
    /// <returns>A task that completes when the settings are saved.</returns>
    Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken);
}
