using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one inspected bone entry from the exported skeleton data.
/// </summary>
public sealed record SpineBoneInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineBoneInfo"/> class.
    /// </summary>
    /// <param name="name">The bone name.</param>
    /// <param name="parentName">The parent bone name, if any.</param>
    /// <param name="length">The local bone length.</param>
    /// <param name="x">The local X offset.</param>
    /// <param name="y">The local Y offset.</param>
    /// <param name="rotation">The local rotation in degrees.</param>
    /// <param name="scaleX">The local X scale.</param>
    /// <param name="scaleY">The local Y scale.</param>
    /// <param name="depth">The derived hierarchy depth.</param>
    public SpineBoneInfo(
        string name,
        string? parentName,
        double length,
        double x,
        double y,
        double rotation,
        double scaleX,
        double scaleY,
        int depth)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        ParentName = Guard.NullIfWhiteSpace(parentName);
        Length = length < 0
            ? throw new ArgumentOutOfRangeException(nameof(length), "Value cannot be negative.")
            : Guard.Finite(length, nameof(length));
        X = Guard.Finite(x, nameof(x));
        Y = Guard.Finite(y, nameof(y));
        Rotation = Guard.Finite(rotation, nameof(rotation));
        ScaleX = Guard.Finite(scaleX, nameof(scaleX));
        ScaleY = Guard.Finite(scaleY, nameof(scaleY));
        Depth = Guard.NonNegative(depth, nameof(depth));
    }

    /// <summary>
    /// Gets the bone name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the parent bone name, if any.
    /// </summary>
    public string? ParentName { get; init; }

    /// <summary>
    /// Gets the local bone length.
    /// </summary>
    public double Length { get; init; }

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
    /// Gets the derived hierarchy depth.
    /// </summary>
    public int Depth { get; init; }
}
