using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one inspected attachment entry within a Spine skin.
/// </summary>
public sealed record SpineAttachmentInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineAttachmentInfo"/> class.
    /// </summary>
    /// <param name="skinName">The owning skin name.</param>
    /// <param name="slotName">The owning slot name.</param>
    /// <param name="name">The attachment name.</param>
    /// <param name="attachmentType">The attachment type.</param>
    /// <param name="path">The optional attachment path.</param>
    /// <param name="x">The local X offset.</param>
    /// <param name="y">The local Y offset.</param>
    /// <param name="rotation">The local rotation in degrees.</param>
    /// <param name="scaleX">The local X scale.</param>
    /// <param name="scaleY">The local Y scale.</param>
    /// <param name="width">The attachment width, if known.</param>
    /// <param name="height">The attachment height, if known.</param>
    /// <param name="vertexCount">The inspected vertex count.</param>
    /// <param name="triangleCount">The inspected triangle count.</param>
    public SpineAttachmentInfo(
        string skinName,
        string slotName,
        string name,
        string attachmentType,
        string? path,
        double x,
        double y,
        double rotation,
        double scaleX,
        double scaleY,
        double? width,
        double? height,
        int vertexCount,
        int triangleCount)
    {
        SkinName = Guard.NotNullOrWhiteSpace(skinName, nameof(skinName));
        SlotName = Guard.NotNullOrWhiteSpace(slotName, nameof(slotName));
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        AttachmentType = Guard.NotNullOrWhiteSpace(attachmentType, nameof(attachmentType));
        Path = Guard.NullIfWhiteSpace(path);
        X = Guard.Finite(x, nameof(x));
        Y = Guard.Finite(y, nameof(y));
        Rotation = Guard.Finite(rotation, nameof(rotation));
        ScaleX = Guard.Finite(scaleX, nameof(scaleX));
        ScaleY = Guard.Finite(scaleY, nameof(scaleY));
        Width = width;
        Height = height;
        VertexCount = Guard.NonNegative(vertexCount, nameof(vertexCount));
        TriangleCount = Guard.NonNegative(triangleCount, nameof(triangleCount));
    }

    /// <summary>
    /// Gets the owning skin name.
    /// </summary>
    public string SkinName { get; init; }

    /// <summary>
    /// Gets the owning slot name.
    /// </summary>
    public string SlotName { get; init; }

    /// <summary>
    /// Gets the attachment name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the attachment type.
    /// </summary>
    public string AttachmentType { get; init; }

    /// <summary>
    /// Gets the optional attachment path.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets the local X offset.
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Gets the local Y offset.
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// Gets the local rotation in degrees.
    /// </summary>
    public double Rotation { get; init; }

    /// <summary>
    /// Gets the local X scale.
    /// </summary>
    public double ScaleX { get; init; }

    /// <summary>
    /// Gets the local Y scale.
    /// </summary>
    public double ScaleY { get; init; }

    /// <summary>
    /// Gets the inspected attachment width, if known.
    /// </summary>
    public double? Width { get; init; }

    /// <summary>
    /// Gets the inspected attachment height, if known.
    /// </summary>
    public double? Height { get; init; }

    /// <summary>
    /// Gets the inspected vertex count.
    /// </summary>
    public int VertexCount { get; init; }

    /// <summary>
    /// Gets the inspected triangle count.
    /// </summary>
    public int TriangleCount { get; init; }
}
