using SpineViewer.Core.Models;
using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Describes the information required to choose a runtime for a Spine project.
/// </summary>
public sealed record RuntimeSelectionRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeSelectionRequest"/> class.
    /// </summary>
    /// <param name="projectReference">The user-facing project reference.</param>
    /// <param name="assetFileSet">The resolved files to inspect.</param>
    /// <param name="versionMatch">The version-detection result to act on.</param>
    /// <param name="availableRuntimes">The runtimes available for selection.</param>
    /// <param name="preferredRuntimeId">An optional caller preference for the runtime identifier.</param>
    public RuntimeSelectionRequest(
        SpineProjectReference projectReference,
        SpineAssetFileSet assetFileSet,
        SpineVersionMatch versionMatch,
        IEnumerable<SpineRuntimeDescriptor> availableRuntimes,
        string? preferredRuntimeId = null)
    {
        ProjectReference = Guard.NotNull(projectReference, nameof(projectReference));
        AssetFileSet = Guard.NotNull(assetFileSet, nameof(assetFileSet));
        VersionMatch = Guard.NotNull(versionMatch, nameof(versionMatch));
        AvailableRuntimes = Guard.MaterializeReadOnlyList(availableRuntimes, nameof(availableRuntimes));
        PreferredRuntimeId = Guard.NullIfWhiteSpace(preferredRuntimeId);
    }

    /// <summary>
    /// Gets the user-facing project reference.
    /// </summary>
    public SpineProjectReference ProjectReference { get; init; }

    /// <summary>
    /// Gets the resolved files to inspect.
    /// </summary>
    public SpineAssetFileSet AssetFileSet { get; init; }

    /// <summary>
    /// Gets the version-detection result to act on.
    /// </summary>
    public SpineVersionMatch VersionMatch { get; init; }

    /// <summary>
    /// Gets the runtimes available for selection.
    /// </summary>
    public IReadOnlyList<SpineRuntimeDescriptor> AvailableRuntimes { get; init; }

    /// <summary>
    /// Gets an optional caller preference for the runtime identifier.
    /// </summary>
    public string? PreferredRuntimeId { get; init; }
}
