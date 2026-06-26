using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one atlas page found in a Spine atlas text file.
/// </summary>
public sealed record SpineAtlasPageInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineAtlasPageInfo"/> class.
    /// </summary>
    /// <param name="name">The atlas page name.</param>
    /// <param name="width">The page width, if known.</param>
    /// <param name="height">The page height, if known.</param>
    /// <param name="format">The page format, if known.</param>
    /// <param name="filter">The page filter, if known.</param>
    /// <param name="repeat">The page repeat mode, if known.</param>
    /// <param name="regionCount">The number of regions on the page.</param>
    public SpineAtlasPageInfo(
        string name,
        int? width,
        int? height,
        string? format,
        string? filter,
        string? repeat,
        int regionCount)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Width = width;
        Height = height;
        Format = Guard.NullIfWhiteSpace(format);
        Filter = Guard.NullIfWhiteSpace(filter);
        Repeat = Guard.NullIfWhiteSpace(repeat);
        RegionCount = Guard.NonNegative(regionCount, nameof(regionCount));
    }

    /// <summary>
    /// Gets the atlas page name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the page width, if known.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Gets the page height, if known.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Gets the page format, if known.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the page filter, if known.
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    /// Gets the page repeat mode, if known.
    /// </summary>
    public string? Repeat { get; init; }

    /// <summary>
    /// Gets the number of regions on the page.
    /// </summary>
    public int RegionCount { get; init; }
}
