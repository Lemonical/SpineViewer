using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the resolved group of files required to load a Spine project.
/// </summary>
public sealed record SpineAssetFileSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineAssetFileSet"/> class.
    /// </summary>
    /// <param name="skeletonPath">The path to the skeleton file.</param>
    /// <param name="atlasPath">The path to the atlas file.</param>
    /// <param name="dependentTexturePaths">The dependent texture files referenced by the atlas.</param>
    public SpineAssetFileSet(
        string skeletonPath,
        string atlasPath,
        IEnumerable<string> dependentTexturePaths)
    {
        SkeletonPath = Guard.NotNullOrWhiteSpace(skeletonPath, nameof(skeletonPath));
        AtlasPath = Guard.NotNullOrWhiteSpace(atlasPath, nameof(atlasPath));
        DependentTexturePaths = Guard.MaterializeNonEmptyStrings(
            dependentTexturePaths,
            nameof(dependentTexturePaths));
    }

    /// <summary>
    /// Gets the path to the skeleton file.
    /// </summary>
    public string SkeletonPath { get; init; }

    /// <summary>
    /// Gets the path to the atlas file.
    /// </summary>
    public string AtlasPath { get; init; }

    /// <summary>
    /// Gets the dependent texture files referenced by the atlas.
    /// </summary>
    public IReadOnlyList<string> DependentTexturePaths { get; init; }
}
