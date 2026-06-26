using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one inspected slot entry from the exported skeleton data.
/// </summary>
public sealed record SpineSlotInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineSlotInfo"/> class.
    /// </summary>
    /// <param name="name">The slot name.</param>
    /// <param name="boneName">The owning bone name.</param>
    /// <param name="attachmentName">The default attachment name, if any.</param>
    /// <param name="blendMode">The blend mode, if any.</param>
    public SpineSlotInfo(
        string name,
        string boneName,
        string? attachmentName,
        string? blendMode)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        BoneName = Guard.NotNullOrWhiteSpace(boneName, nameof(boneName));
        AttachmentName = Guard.NullIfWhiteSpace(attachmentName);
        BlendMode = Guard.NullIfWhiteSpace(blendMode);
    }

    /// <summary>
    /// Gets the slot name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the owning bone name.
    /// </summary>
    public string BoneName { get; init; }

    /// <summary>
    /// Gets the default attachment name, if any.
    /// </summary>
    public string? AttachmentName { get; init; }

    /// <summary>
    /// Gets the blend mode, if any.
    /// </summary>
    public string? BlendMode { get; init; }
}
