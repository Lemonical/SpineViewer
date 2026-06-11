using SpineViewer.Core.Models;
using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Represents the structured outcome of resolving a Spine project reference and its dependent files.
/// </summary>
public sealed record ResolveSpineProjectResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveSpineProjectResult"/> class.
    /// </summary>
    /// <param name="isSuccessful">Indicates whether the project resolved successfully.</param>
    /// <param name="projectReference">The resolved project reference, if one could be determined.</param>
    /// <param name="assetFileSet">The resolved asset file set, if one could be determined.</param>
    /// <param name="diagnostics">The diagnostics produced during resolution.</param>
    public ResolveSpineProjectResult(
        bool isSuccessful,
        SpineProjectReference? projectReference,
        SpineAssetFileSet? assetFileSet,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        if (isSuccessful)
        {
            ProjectReference = Guard.NotNull(projectReference, nameof(projectReference));
            AssetFileSet = Guard.NotNull(assetFileSet, nameof(assetFileSet));
        }
        else
        {
            ProjectReference = projectReference;
            AssetFileSet = assetFileSet;
        }

        IsSuccessful = isSuccessful;
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets a value indicating whether the project resolved successfully.
    /// </summary>
    public bool IsSuccessful { get; init; }

    /// <summary>
    /// Gets the resolved project reference, if one could be determined.
    /// </summary>
    public SpineProjectReference? ProjectReference { get; init; }

    /// <summary>
    /// Gets the resolved asset file set, if one could be determined.
    /// </summary>
    public SpineAssetFileSet? AssetFileSet { get; init; }

    /// <summary>
    /// Gets the diagnostics produced during resolution.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
