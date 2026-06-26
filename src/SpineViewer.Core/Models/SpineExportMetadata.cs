using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Summarizes high-level export metadata for one inspected Spine project.
/// </summary>
public sealed record SpineExportMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineExportMetadata"/> class.
    /// </summary>
    /// <param name="exportVersion">The exported Spine version, if known.</param>
    /// <param name="imagesPath">The exported images path, if any.</param>
    /// <param name="audioPath">The exported audio path, if any.</param>
    /// <param name="width">The exported setup-pose width, if known.</param>
    /// <param name="height">The exported setup-pose height, if known.</param>
    /// <param name="fps">The exported frame rate, if known.</param>
    /// <param name="boneCount">The inspected bone count.</param>
    /// <param name="slotCount">The inspected slot count.</param>
    /// <param name="skinCount">The inspected skin count.</param>
    /// <param name="animationCount">The inspected animation count.</param>
    /// <param name="atlasPageCount">The inspected atlas page count.</param>
    /// <param name="atlasRegionCount">The inspected atlas region count.</param>
    public SpineExportMetadata(
        string? exportVersion,
        string? imagesPath,
        string? audioPath,
        double? width,
        double? height,
        double? fps,
        int boneCount,
        int slotCount,
        int skinCount,
        int animationCount,
        int atlasPageCount,
        int atlasRegionCount)
    {
        ExportVersion = Guard.NullIfWhiteSpace(exportVersion);
        ImagesPath = Guard.NullIfWhiteSpace(imagesPath);
        AudioPath = Guard.NullIfWhiteSpace(audioPath);
        Width = width;
        Height = height;
        FramesPerSecond = fps;
        BoneCount = Guard.NonNegative(boneCount, nameof(boneCount));
        SlotCount = Guard.NonNegative(slotCount, nameof(slotCount));
        SkinCount = Guard.NonNegative(skinCount, nameof(skinCount));
        AnimationCount = Guard.NonNegative(animationCount, nameof(animationCount));
        AtlasPageCount = Guard.NonNegative(atlasPageCount, nameof(atlasPageCount));
        AtlasRegionCount = Guard.NonNegative(atlasRegionCount, nameof(atlasRegionCount));
    }

    /// <summary>
    /// Gets the exported Spine version, if known.
    /// </summary>
    public string? ExportVersion { get; init; }

    /// <summary>
    /// Gets the exported images path, if any.
    /// </summary>
    public string? ImagesPath { get; init; }

    /// <summary>
    /// Gets the exported audio path, if any.
    /// </summary>
    public string? AudioPath { get; init; }

    /// <summary>
    /// Gets the exported setup-pose width, if known.
    /// </summary>
    public double? Width { get; init; }

    /// <summary>
    /// Gets the exported setup-pose height, if known.
    /// </summary>
    public double? Height { get; init; }

    /// <summary>
    /// Gets the exported frame rate, if known.
    /// </summary>
    public double? FramesPerSecond { get; init; }

    /// <summary>
    /// Gets the inspected bone count.
    /// </summary>
    public int BoneCount { get; init; }

    /// <summary>
    /// Gets the inspected slot count.
    /// </summary>
    public int SlotCount { get; init; }

    /// <summary>
    /// Gets the inspected skin count.
    /// </summary>
    public int SkinCount { get; init; }

    /// <summary>
    /// Gets the inspected animation count.
    /// </summary>
    public int AnimationCount { get; init; }

    /// <summary>
    /// Gets the inspected atlas page count.
    /// </summary>
    public int AtlasPageCount { get; init; }

    /// <summary>
    /// Gets the inspected atlas region count.
    /// </summary>
    public int AtlasRegionCount { get; init; }
}
