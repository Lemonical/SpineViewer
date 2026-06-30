using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Builds structured inspection data for one resolved Spine project asset set.
/// </summary>
public interface ISpineProjectInspector
{
    /// <summary>
    /// Inspects the supplied Spine asset set and extracts viewer-facing metadata.
    /// </summary>
    /// <param name="assetFileSet">The resolved asset set to inspect.</param>
    /// <param name="selectedRuntime">The runtime that successfully loaded the asset set.</param>
    /// <param name="cancellationToken">A token that cancels the inspection operation.</param>
    /// <returns>The structured inspection snapshot for the supplied assets.</returns>
    Task<SpineProjectInspection> InspectAsync(
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor selectedRuntime,
        CancellationToken cancellationToken);
}
