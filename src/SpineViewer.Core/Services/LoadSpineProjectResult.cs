using SpineViewer.Core.Models;
using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Represents the structured outcome of a Spine project load operation.
/// </summary>
public sealed record LoadSpineProjectResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSpineProjectResult"/> class.
    /// </summary>
    /// <param name="isSuccessful">Indicates whether the load completed successfully.</param>
    /// <param name="projectReference">The user-facing project reference.</param>
    /// <param name="assetFileSet">The resolved files used by the load.</param>
    /// <param name="selectedRuntime">The runtime used by the load attempt.</param>
    /// <param name="versionMatch">The version-detection result associated with the load.</param>
    /// <param name="diagnostics">The structured diagnostics produced by the load.</param>
    public LoadSpineProjectResult(
        bool isSuccessful,
        SpineProjectReference projectReference,
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor selectedRuntime,
        SpineVersionMatch versionMatch,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        IsSuccessful = isSuccessful;
        ProjectReference = Guard.NotNull(projectReference, nameof(projectReference));
        AssetFileSet = Guard.NotNull(assetFileSet, nameof(assetFileSet));
        SelectedRuntime = Guard.NotNull(selectedRuntime, nameof(selectedRuntime));
        VersionMatch = Guard.NotNull(versionMatch, nameof(versionMatch));
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets a value indicating whether the load completed successfully.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Gets the user-facing project reference.
    /// </summary>
    public SpineProjectReference ProjectReference { get; init; }

    /// <summary>
    /// Gets the resolved files used by the load.
    /// </summary>
    public SpineAssetFileSet AssetFileSet { get; init; }

    /// <summary>
    /// Gets the runtime used by the load attempt.
    /// </summary>
    public SpineRuntimeDescriptor SelectedRuntime { get; init; }

    /// <summary>
    /// Gets the version-detection result associated with the load.
    /// </summary>
    public SpineVersionMatch VersionMatch { get; init; }

    /// <summary>
    /// Gets the structured diagnostics produced by the load.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
