using SpineViewer.Core.Models;
using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Describes the resolved input needed to load a Spine project.
/// </summary>
public sealed record LoadSpineProjectRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSpineProjectRequest"/> class.
    /// </summary>
    /// <param name="projectReference">The user-facing project reference.</param>
    /// <param name="assetFileSet">The resolved files to load.</param>
    /// <param name="selectedRuntime">The runtime selected for the load.</param>
    /// <param name="versionMatch">The version-detection result that informed the selection.</param>
    public LoadSpineProjectRequest(
        SpineProjectReference projectReference,
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor selectedRuntime,
        SpineVersionMatch versionMatch)
    {
        ProjectReference = Guard.NotNull(projectReference, nameof(projectReference));
        AssetFileSet = Guard.NotNull(assetFileSet, nameof(assetFileSet));
        SelectedRuntime = Guard.NotNull(selectedRuntime, nameof(selectedRuntime));
        VersionMatch = Guard.NotNull(versionMatch, nameof(versionMatch));
    }

    /// <summary>
    /// Gets the user-facing project reference.
    /// </summary>
    public SpineProjectReference ProjectReference { get; init; }

    /// <summary>
    /// Gets the resolved files to load.
    /// </summary>
    public SpineAssetFileSet AssetFileSet { get; init; }

    /// <summary>
    /// Gets the runtime selected for the load.
    /// </summary>
    public SpineRuntimeDescriptor SelectedRuntime { get; init; }

    /// <summary>
    /// Gets the version-detection result that informed the selection.
    /// </summary>
    public SpineVersionMatch VersionMatch { get; init; }
}
