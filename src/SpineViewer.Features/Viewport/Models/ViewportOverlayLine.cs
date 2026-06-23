using Avalonia.Media;

namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Represents one world-space line segment drawn by a viewport overlay component.
/// </summary>
public sealed record ViewportOverlayLine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportOverlayLine"/> class.
    /// </summary>
    /// <param name="startX">The world-space start X coordinate.</param>
    /// <param name="startY">The world-space start Y coordinate.</param>
    /// <param name="endX">The world-space end X coordinate.</param>
    /// <param name="endY">The world-space end Y coordinate.</param>
    /// <param name="color">The display color for the line.</param>
    /// <param name="thickness">The line thickness in device-independent pixels.</param>
    /// <param name="includeInContentBounds">Indicates whether the line should contribute to fit-to-view bounds.</param>
    public ViewportOverlayLine(
        double startX,
        double startY,
        double endX,
        double endY,
        Color color,
        double thickness,
        bool includeInContentBounds = true)
    {
        if (double.IsNaN(startX) || double.IsInfinity(startX))
        {
            throw new ArgumentOutOfRangeException(nameof(startX), "StartX must be a finite number.");
        }

        if (double.IsNaN(startY) || double.IsInfinity(startY))
        {
            throw new ArgumentOutOfRangeException(nameof(startY), "StartY must be a finite number.");
        }

        if (double.IsNaN(endX) || double.IsInfinity(endX))
        {
            throw new ArgumentOutOfRangeException(nameof(endX), "EndX must be a finite number.");
        }

        if (double.IsNaN(endY) || double.IsInfinity(endY))
        {
            throw new ArgumentOutOfRangeException(nameof(endY), "EndY must be a finite number.");
        }

        if (double.IsNaN(thickness) || double.IsInfinity(thickness) || thickness <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(thickness), "Thickness must be a positive finite number.");
        }

        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        Color = color;
        Thickness = thickness;
        IncludeInContentBounds = includeInContentBounds;
    }

    /// <summary>
    /// Gets the world-space start X coordinate.
    /// </summary>
    public double StartX { get; init; }

    /// <summary>
    /// Gets the world-space start Y coordinate.
    /// </summary>
    public double StartY { get; init; }

    /// <summary>
    /// Gets the world-space end X coordinate.
    /// </summary>
    public double EndX { get; init; }

    /// <summary>
    /// Gets the world-space end Y coordinate.
    /// </summary>
    public double EndY { get; init; }

    /// <summary>
    /// Gets the display color for the line.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// Gets the line thickness in device-independent pixels.
    /// </summary>
    public double Thickness { get; init; }

    /// <summary>
    /// Gets a value indicating whether the line should contribute to fit-to-view bounds.
    /// </summary>
    public bool IncludeInContentBounds { get; init; }
}
