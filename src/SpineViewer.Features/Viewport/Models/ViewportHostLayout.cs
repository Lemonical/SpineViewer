namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Represents the current on-screen size of the viewport render host.
/// </summary>
public sealed record ViewportHostLayout
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportHostLayout"/> class.
    /// </summary>
    /// <param name="width">The current host width in device-independent pixels.</param>
    /// <param name="height">The current host height in device-independent pixels.</param>
    public ViewportHostLayout(double width, double height)
    {
        if (double.IsNaN(width) || double.IsInfinity(width) || width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be a finite non-negative number.");
        }

        if (double.IsNaN(height) || double.IsInfinity(height) || height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be a finite non-negative number.");
        }

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the current host width in device-independent pixels.
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Gets the current host height in device-independent pixels.
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Gets a value indicating whether the host currently exposes a drawable surface.
    /// </summary>
    public bool HasSurface => Width > 0 && Height > 0;
}
