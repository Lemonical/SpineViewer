using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one inspected skin entry from the exported skeleton data.
/// </summary>
public sealed record SpineSkinInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineSkinInfo"/> class.
    /// </summary>
    /// <param name="name">The skin name.</param>
    /// <param name="slotCount">The number of slots with attachments in the skin.</param>
    /// <param name="attachmentCount">The number of attachments in the skin.</param>
    /// <param name="isDefault">Indicates whether the skin is the default skin selection.</param>
    public SpineSkinInfo(
        string name,
        int slotCount,
        int attachmentCount,
        bool isDefault)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        SlotCount = Guard.NonNegative(slotCount, nameof(slotCount));
        AttachmentCount = Guard.NonNegative(attachmentCount, nameof(attachmentCount));
        IsDefault = isDefault;
    }

    /// <summary>
    /// Gets the skin name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the number of slots with attachments in the skin.
    /// </summary>
    public int SlotCount { get; init; }

    /// <summary>
    /// Gets the number of attachments in the skin.
    /// </summary>
    public int AttachmentCount { get; init; }

    /// <summary>
    /// Gets a value indicating whether this skin is the default selection.
    /// </summary>
    public bool IsDefault { get; init; }
}
