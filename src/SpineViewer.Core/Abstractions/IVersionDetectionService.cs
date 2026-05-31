using SpineViewer.Core.Models;

namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Detects the most likely Spine export/runtime match for a resolved asset set.
/// </summary>
public interface IVersionDetectionService
{
    /// <summary>
    /// Detects the most likely export version and runtime family for a Spine asset set.
    /// </summary>
    /// <param name="assetFileSet">The resolved asset set to inspect.</param>
    /// <param name="cancellationToken">A token that cancels the detection operation.</param>
    /// <returns>The structured version-match result.</returns>
    Task<SpineVersionMatch> DetectAsync(
        SpineAssetFileSet assetFileSet,
        CancellationToken cancellationToken);
}
