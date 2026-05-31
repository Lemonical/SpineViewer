using SpineViewer.Core.Services;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Loads a Spine project through the currently selected runtime path.
/// </summary>
public interface ISpineProjectLoader
{
    /// <summary>
    /// Loads a Spine project and returns structured diagnostics instead of raw runtime failures.
    /// </summary>
    /// <param name="request">The load request to execute.</param>
    /// <param name="cancellationToken">A token that cancels the load operation.</param>
    /// <returns>The structured load result.</returns>
    Task<LoadSpineProjectResult> LoadAsync(
        LoadSpineProjectRequest request,
        CancellationToken cancellationToken);
}
