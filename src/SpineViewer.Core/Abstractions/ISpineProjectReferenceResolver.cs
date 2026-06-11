using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Resolves user-selected files or stored project references into a concrete Spine asset file set.
/// </summary>
public interface ISpineProjectReferenceResolver
{
    /// <summary>
    /// Resolves a project from one selected file and an optional explicit companion file.
    /// </summary>
    /// <param name="selectedPath">The selected atlas or skeleton path.</param>
    /// <param name="companionPath">An optional explicit companion path supplied by the caller.</param>
    /// <param name="cancellationToken">A token that cancels the resolution operation.</param>
    /// <returns>The structured resolution result.</returns>
    Task<ResolveSpineProjectResult> ResolveFromSelectionAsync(
        string selectedPath,
        string? companionPath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Re-resolves a previously stored project reference for project reopen flows.
    /// </summary>
    /// <param name="projectReference">The stored project reference to reopen.</param>
    /// <param name="cancellationToken">A token that cancels the resolution operation.</param>
    /// <returns>The structured resolution result.</returns>
    Task<ResolveSpineProjectResult> ResolveForReopenAsync(
        SpineProjectReference projectReference,
        CancellationToken cancellationToken);
}
