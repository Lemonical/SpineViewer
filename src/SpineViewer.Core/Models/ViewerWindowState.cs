using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the persisted window placement used to restore the main shell.
/// </summary>
public sealed record ViewerWindowState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerWindowState"/> class.
    /// </summary>
    /// <param name="width">The restored window width.</param>
    /// <param name="height">The restored window height.</param>
    /// <param name="positionX">The restored window X position, if known.</param>
    /// <param name="positionY">The restored window Y position, if known.</param>
    /// <param name="isMaximized">Indicates whether the window should restore maximized.</param>
    public ViewerWindowState(
        double width,
        double height,
        int? positionX,
        int? positionY,
        bool isMaximized)
    {
        Width = Guard.PositiveFinite(width, nameof(width));
        Height = Guard.PositiveFinite(height, nameof(height));
        PositionX = positionX;
        PositionY = positionY;
        IsMaximized = isMaximized;

        if ((PositionX is null) != (PositionY is null))
        {
            throw new ArgumentException(
                "Window position coordinates must both be provided or both be omitted.",
                nameof(positionX));
        }
    }

    /// <summary>
    /// Gets the restored window width.
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Gets the restored window height.
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Gets the restored window X position, if known.
    /// </summary>
    public int? PositionX { get; init; }

    /// <summary>
    /// Gets the restored window Y position, if known.
    /// </summary>
    public int? PositionY { get; init; }

    /// <summary>
    /// Gets a value indicating whether the window should restore maximized.
    /// </summary>
    public bool IsMaximized { get; init; }
}
