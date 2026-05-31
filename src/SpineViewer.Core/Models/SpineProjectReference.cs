using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents a user-facing reference to a Spine project for open and recent-file workflows.
/// </summary>
public sealed record SpineProjectReference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineProjectReference"/> class.
    /// </summary>
    /// <param name="displayName">The user-facing project name.</param>
    /// <param name="skeletonPath">The path to the skeleton file.</param>
    /// <param name="atlasPath">The path to the atlas file.</param>
    public SpineProjectReference(
        string displayName,
        string skeletonPath,
        string atlasPath)
    {
        DisplayName = Guard.NotNullOrWhiteSpace(displayName, nameof(displayName));
        SkeletonPath = Guard.NotNullOrWhiteSpace(skeletonPath, nameof(skeletonPath));
        AtlasPath = Guard.NotNullOrWhiteSpace(atlasPath, nameof(atlasPath));
    }

    /// <summary>
    /// Gets the user-facing project name.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Gets the path to the skeleton file.
    /// </summary>
    public string SkeletonPath { get; init; }

    /// <summary>
    /// Gets the path to the atlas file.
    /// </summary>
    public string AtlasPath { get; init; }
}
