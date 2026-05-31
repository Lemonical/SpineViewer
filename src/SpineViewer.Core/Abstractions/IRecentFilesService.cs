using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Manages the persisted recent-file list for Spine projects.
/// </summary>
public interface IRecentFilesService
{
    /// <summary>
    /// Returns the current recent-file entries in most-recent-first order.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the query.</param>
    /// <returns>The current recent-file entries.</returns>
    Task<IReadOnlyList<SpineProjectReference>> GetRecentFilesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds or promotes one project reference in the recent-file list.
    /// </summary>
    /// <param name="projectReference">The project reference to persist.</param>
    /// <param name="cancellationToken">A token that cancels the update.</param>
    /// <returns>A task that completes when the update is stored.</returns>
    Task AddAsync(SpineProjectReference projectReference, CancellationToken cancellationToken);

    /// <summary>
    /// Removes entries that no longer point at valid project files.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the cleanup operation.</param>
    /// <returns>A task that completes when cleanup has finished.</returns>
    Task RemoveMissingEntriesAsync(CancellationToken cancellationToken);
}
