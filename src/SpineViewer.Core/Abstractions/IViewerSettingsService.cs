using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Owns the in-memory viewer settings snapshot and persists changes for shared callers.
/// </summary>
public interface IViewerSettingsService
{
    /// <summary>
    /// Occurs when the current viewer settings change.
    /// </summary>
    event EventHandler? SettingsChanged;

    /// <summary>
    /// Gets the current viewer settings snapshot.
    /// </summary>
    ViewerSettings CurrentSettings { get; }

    /// <summary>
    /// Loads persisted settings into the current snapshot.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels initialization.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new viewer settings snapshot and broadcasts the change.
    /// </summary>
    /// <param name="settings">The settings snapshot to persist.</param>
    /// <param name="cancellationToken">A token that cancels the save operation.</param>
    /// <returns>A task that completes when the settings are saved.</returns>
    Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the viewer settings to defaults and persists the default snapshot.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the reset operation.</param>
    /// <returns>A task that completes when the reset has finished.</returns>
    Task ResetAsync(CancellationToken cancellationToken);
}
