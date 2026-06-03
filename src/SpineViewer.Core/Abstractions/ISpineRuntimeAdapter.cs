using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Probes and loads Spine assets for one concrete runtime target.
/// </summary>
public interface ISpineRuntimeAdapter
{
    /// <summary>
    /// Gets the descriptor that identifies this runtime adapter.
    /// </summary>
    SpineRuntimeDescriptor Descriptor { get; }

    /// <summary>
    /// Probes a resolved asset set to determine whether this runtime can load it.
    /// </summary>
    /// <param name="assetFileSet">The resolved asset set to inspect.</param>
    /// <param name="cancellationToken">A token that cancels the probe operation.</param>
    /// <returns>The runtime-specific probe result.</returns>
    Task<SpineRuntimeProbeResult> ProbeAsync(
        SpineAssetFileSet assetFileSet,
        CancellationToken cancellationToken);

    /// <summary>
    /// Loads a Spine project through this runtime adapter.
    /// </summary>
    /// <param name="request">The adapter-scoped load request.</param>
    /// <param name="cancellationToken">A token that cancels the load operation.</param>
    /// <returns>The structured runtime load result.</returns>
    Task<SpineLoadResult> LoadAsync(
        SpineLoadRequest request,
        CancellationToken cancellationToken);
}
