using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Coordinates the application workspace lifecycle for opening, reloading, restoring, and closing Spine sessions.
/// </summary>
public interface IWorkspaceSessionService
{
    /// <summary>
    /// Occurs when the workspace state changes.
    /// </summary>
    event EventHandler? StateChanged;

    /// <summary>
    /// Gets the current application workspace state.
    /// </summary>
    WorkspaceState State { get; }

    /// <summary>
    /// Loads persisted workspace dependencies such as recent files and viewer settings.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels initialization.</param>
    /// <returns>A task that completes when initialization has finished.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Opens a Spine project from one or two user-selected files.
    /// </summary>
    /// <param name="selectedPaths">The selected atlas and/or skeleton paths.</param>
    /// <param name="cancellationToken">A token that cancels the open operation.</param>
    /// <returns>A task that completes when the open flow has finished.</returns>
    Task OpenAsync(IReadOnlyList<string> selectedPaths, CancellationToken cancellationToken);

    /// <summary>
    /// Opens a Spine project from one selected file and an optional explicit companion file.
    /// </summary>
    /// <param name="selectedPath">The selected atlas or skeleton path.</param>
    /// <param name="companionPath">An optional explicit companion path.</param>
    /// <param name="cancellationToken">A token that cancels the open operation.</param>
    /// <returns>A task that completes when the open flow has finished.</returns>
    Task OpenAsync(string selectedPath, string? companionPath, CancellationToken cancellationToken);

    /// <summary>
    /// Opens a previously stored project reference, such as a recent-file or restore target.
    /// </summary>
    /// <param name="projectReference">The project reference to reopen.</param>
    /// <param name="cancellationToken">A token that cancels the open operation.</param>
    /// <returns>A task that completes when the open flow has finished.</returns>
    Task OpenAsync(SpineProjectReference projectReference, CancellationToken cancellationToken);

    /// <summary>
    /// Reloads the currently open session from disk.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the reload operation.</param>
    /// <returns>A task that completes when reload has finished.</returns>
    Task ReloadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the current session and clears any persisted restore target.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the close operation.</param>
    /// <returns>A task that completes when the workspace has been cleared.</returns>
    Task CloseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Restores the persisted last session when viewer settings allow it.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the restore operation.</param>
    /// <returns>A task that completes when restore has finished.</returns>
    Task RestoreLastSessionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates the transient playback state for the current session.
    /// </summary>
    /// <param name="playbackState">The playback state to store.</param>
    void UpdatePlaybackState(PlaybackState playbackState);

    /// <summary>
    /// Updates the transient viewport state for the current session.
    /// </summary>
    /// <param name="viewportState">The viewport state to store.</param>
    void UpdateViewportState(ViewportState viewportState);

    /// <summary>
    /// Updates the selected skin for the current session.
    /// </summary>
    /// <param name="skinName">The selected skin name, or <see langword="null" /> to clear the selection.</param>
    void UpdateSelectedSkin(string? skinName);
}
