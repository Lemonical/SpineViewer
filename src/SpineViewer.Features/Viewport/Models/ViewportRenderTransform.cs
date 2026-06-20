namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Represents the camera transform used to project world-space viewport geometry onto the render host.
/// </summary>
public sealed record ViewportRenderTransform
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportRenderTransform"/> class.
    /// </summary>
    /// <param name="scale">The current world-to-screen scale factor.</param>
    /// <param name="translateX">The current horizontal translation in device-independent pixels.</param>
    /// <param name="translateY">The current vertical translation in device-independent pixels.</param>
    public ViewportRenderTransform(
        double scale,
        double translateX,
        double translateY)
    {
        if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be a positive finite number.");
        }

        if (double.IsNaN(translateX) || double.IsInfinity(translateX))
        {
            throw new ArgumentOutOfRangeException(nameof(translateX), "TranslateX must be a finite number.");
        }

        if (double.IsNaN(translateY) || double.IsInfinity(translateY))
        {
            throw new ArgumentOutOfRangeException(nameof(translateY), "TranslateY must be a finite number.");
        }

        Scale = scale;
        TranslateX = translateX;
        TranslateY = translateY;
    }

    /// <summary>
    /// Gets the current world-to-screen scale factor.
    /// </summary>
    public double Scale { get; init; }

    /// <summary>
    /// Gets the current horizontal translation in device-independent pixels.
    /// </summary>
    public double TranslateX { get; init; }

    /// <summary>
    /// Gets the current vertical translation in device-independent pixels.
    /// </summary>
    public double TranslateY { get; init; }
}
