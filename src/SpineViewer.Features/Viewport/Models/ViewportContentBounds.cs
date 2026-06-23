namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Represents the world-space content bounds used for fit-to-view calculations.
/// </summary>
public sealed record ViewportContentBounds
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportContentBounds"/> class.
    /// </summary>
    /// <param name="minimumX">The minimum world-space X coordinate.</param>
    /// <param name="minimumY">The minimum world-space Y coordinate.</param>
    /// <param name="maximumX">The maximum world-space X coordinate.</param>
    /// <param name="maximumY">The maximum world-space Y coordinate.</param>
    public ViewportContentBounds(
        double minimumX,
        double minimumY,
        double maximumX,
        double maximumY)
    {
        if (double.IsNaN(minimumX) || double.IsInfinity(minimumX))
        {
            throw new ArgumentOutOfRangeException(nameof(minimumX), "MinimumX must be a finite number.");
        }

        if (double.IsNaN(minimumY) || double.IsInfinity(minimumY))
        {
            throw new ArgumentOutOfRangeException(nameof(minimumY), "MinimumY must be a finite number.");
        }

        if (double.IsNaN(maximumX) || double.IsInfinity(maximumX))
        {
            throw new ArgumentOutOfRangeException(nameof(maximumX), "MaximumX must be a finite number.");
        }

        if (double.IsNaN(maximumY) || double.IsInfinity(maximumY))
        {
            throw new ArgumentOutOfRangeException(nameof(maximumY), "MaximumY must be a finite number.");
        }

        if (maximumX < minimumX)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumX), "MaximumX cannot be less than MinimumX.");
        }

        if (maximumY < minimumY)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumY), "MaximumY cannot be less than MinimumY.");
        }

        MinimumX = minimumX;
        MinimumY = minimumY;
        MaximumX = maximumX;
        MaximumY = maximumY;
    }

    /// <summary>
    /// Gets the minimum world-space X coordinate.
    /// </summary>
    public double MinimumX { get; init; }

    /// <summary>
    /// Gets the minimum world-space Y coordinate.
    /// </summary>
    public double MinimumY { get; init; }

    /// <summary>
    /// Gets the maximum world-space X coordinate.
    /// </summary>
    public double MaximumX { get; init; }

    /// <summary>
    /// Gets the maximum world-space Y coordinate.
    /// </summary>
    public double MaximumY { get; init; }

    /// <summary>
    /// Gets the width of the content bounds.
    /// </summary>
    public double Width => MaximumX - MinimumX;

    /// <summary>
    /// Gets the height of the content bounds.
    /// </summary>
    public double Height => MaximumY - MinimumY;

    /// <summary>
    /// Gets the horizontal center of the content bounds.
    /// </summary>
    public double CenterX => MinimumX + (Width / 2.0);

    /// <summary>
    /// Gets the vertical center of the content bounds.
    /// </summary>
    public double CenterY => MinimumY + (Height / 2.0);
}
