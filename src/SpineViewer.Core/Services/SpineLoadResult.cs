using SpineViewer.Core.Models;
using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Represents the structured outcome of loading a Spine project through one runtime adapter.
/// </summary>
public sealed record SpineLoadResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineLoadResult"/> class.
    /// </summary>
    /// <param name="isSuccessful">Indicates whether the runtime load completed successfully.</param>
    /// <param name="projectReference">The user-facing project reference.</param>
    /// <param name="assetFileSet">The resolved files used by the load.</param>
    /// <param name="runtime">The runtime used by the load.</param>
    /// <param name="versionMatch">The version-detection result associated with the load.</param>
    /// <param name="unsupportedFeatures">The unsupported features reported by the runtime.</param>
    /// <param name="diagnostics">The structured diagnostics produced by the runtime load.</param>
    public SpineLoadResult(
        bool isSuccessful,
        SpineProjectReference projectReference,
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor runtime,
        SpineVersionMatch versionMatch,
        IEnumerable<UnsupportedSpineFeature> unsupportedFeatures,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        IsSuccessful = isSuccessful;
        ProjectReference = Guard.NotNull(projectReference, nameof(projectReference));
        AssetFileSet = Guard.NotNull(assetFileSet, nameof(assetFileSet));
        Runtime = Guard.NotNull(runtime, nameof(runtime));
        VersionMatch = Guard.NotNull(versionMatch, nameof(versionMatch));
        UnsupportedFeatures = Guard.MaterializeReadOnlyList(
            unsupportedFeatures,
            nameof(unsupportedFeatures));
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets a value indicating whether the runtime load completed successfully.
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
    /// Gets the runtime used by the load.
    /// </summary>
    public SpineRuntimeDescriptor Runtime { get; init; }

    /// <summary>
    /// Gets the version-detection result associated with the load.
    /// </summary>
    public SpineVersionMatch VersionMatch { get; init; }

    /// <summary>
    /// Gets the unsupported features reported by the runtime.
    /// </summary>
    public IReadOnlyList<UnsupportedSpineFeature> UnsupportedFeatures { get; init; }

    /// <summary>
    /// Gets the structured diagnostics produced by the runtime load.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
