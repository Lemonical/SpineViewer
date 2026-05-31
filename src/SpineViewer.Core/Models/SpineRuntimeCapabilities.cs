namespace SpineViewer.Core.Models;

/// <summary>
/// Describes the major feature areas a runtime adapter can support.
/// </summary>
public sealed record SpineRuntimeCapabilities
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimeCapabilities"/> class.
    /// </summary>
    /// <param name="supportsJsonSkeleton">Indicates whether JSON skeleton files are supported.</param>
    /// <param name="supportsBinarySkeleton">Indicates whether binary skeleton files are supported.</param>
    /// <param name="supportsEvents">Indicates whether Spine events are supported.</param>
    /// <param name="supportsClipping">Indicates whether clipping attachments are supported.</param>
    /// <param name="supportsMeshes">Indicates whether mesh attachments are supported.</param>
    /// <param name="supportsMultipleTracks">Indicates whether multiple animation tracks are supported.</param>
    public SpineRuntimeCapabilities(
        bool supportsJsonSkeleton,
        bool supportsBinarySkeleton,
        bool supportsEvents,
        bool supportsClipping,
        bool supportsMeshes,
        bool supportsMultipleTracks)
    {
        SupportsJsonSkeleton = supportsJsonSkeleton;
        SupportsBinarySkeleton = supportsBinarySkeleton;
        SupportsEvents = supportsEvents;
        SupportsClipping = supportsClipping;
        SupportsMeshes = supportsMeshes;
        SupportsMultipleTracks = supportsMultipleTracks;
    }

    /// <summary>
    /// Gets a value indicating whether JSON skeleton files are supported.
    /// </summary>
    public bool SupportsJsonSkeleton { get; init; }

    /// <summary>
    /// Gets a value indicating whether binary skeleton files are supported.
    /// </summary>
    public bool SupportsBinarySkeleton { get; init; }

    /// <summary>
    /// Gets a value indicating whether Spine events are supported.
    /// </summary>
    public bool SupportsEvents { get; init; }

    /// <summary>
    /// Gets a value indicating whether clipping attachments are supported.
    /// </summary>
    public bool SupportsClipping { get; init; }

    /// <summary>
    /// Gets a value indicating whether mesh attachments are supported.
    /// </summary>
    public bool SupportsMeshes { get; init; }

    /// <summary>
    /// Gets a value indicating whether multiple animation tracks are supported.
    /// </summary>
    public bool SupportsMultipleTracks { get; init; }
}
