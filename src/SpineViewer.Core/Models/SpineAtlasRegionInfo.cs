using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one atlas region found in a Spine atlas text file.
/// </summary>
public sealed record SpineAtlasRegionInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineAtlasRegionInfo"/> class.
    /// </summary>
    /// <param name="pageName">The owning atlas page name.</param>
    /// <param name="name">The region name.</param>
    /// <param name="rotate">Indicates whether the region is rotated.</param>
    /// <param name="width">The packed region width, if known.</param>
    /// <param name="height">The packed region height, if known.</param>
    /// <param name="originalWidth">The original region width, if known.</param>
    /// <param name="originalHeight">The original region height, if known.</param>
    public SpineAtlasRegionInfo(
        string pageName,
        string name,
        bool rotate,
        int? width,
        int? height,
        int? originalWidth,
        int? originalHeight)
    {
        PageName = Guard.NotNullOrWhiteSpace(pageName, nameof(pageName));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Rotate = rotate;
        Width = width;
        Height = height;
        OriginalWidth = originalWidth;
        OriginalHeight = originalHeight;
    }

    /// <summary>
    /// Gets the owning atlas page name.
    /// </summary>
    public string PageName { get; init; }

    /// <summary>
    /// Gets the region name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets a value indicating whether the region is rotated.
    /// </summary>
    public bool Rotate { get; init; }

    /// <summary>
    /// Gets the packed region width, if known.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Gets the packed region height, if known.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Gets the original region width, if known.
    /// </summary>
    public int? OriginalWidth { get; init; }

    /// <summary>
    /// Gets the original region height, if known.
    /// </summary>
    public int? OriginalHeight { get; init; }
}
