namespace SpineViewer.Core.Models;

/// <summary>
/// Identifies a viewer-relevant Spine runtime capability.
/// </summary>
public enum SpineRuntimeFeature
{
    /// <summary>
    /// Supports JSON skeleton data files.
    /// </summary>
    JsonSkeleton,

    /// <summary>
    /// Supports binary skeleton data files.
    /// </summary>
    BinarySkeleton,

    /// <summary>
    /// Supports Spine events.
    /// </summary>
    Events,

    /// <summary>
    /// Supports clipping attachments.
    /// </summary>
    Clipping,

    /// <summary>
    /// Supports mesh attachments.
    /// </summary>
    Meshes,

    /// <summary>
    /// Supports multiple animation tracks.
    /// </summary>
    MultipleTracks,
}
