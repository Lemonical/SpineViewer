using Avalonia.Media;

namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Represents one text overlay drawn either in world space or screen space.
/// </summary>
public sealed record ViewportOverlayText
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportOverlayText"/> class.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <param name="x">The world-space or screen-space X coordinate.</param>
    /// <param name="y">The world-space or screen-space Y coordinate.</param>
    /// <param name="color">The text color.</param>
    /// <param name="fontSize">The text font size.</param>
    /// <param name="useWorldCoordinates">Indicates whether the coordinates should be transformed through the viewport camera.</param>
    public ViewportOverlayText(
        string text,
        double x,
        double y,
        Color color,
        double fontSize,
        bool useWorldCoordinates)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or whitespace.", nameof(text));
        }

        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "X must be a finite number.");
        }

        if (double.IsNaN(y) || double.IsInfinity(y))
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Y must be a finite number.");
        }

        if (double.IsNaN(fontSize) || double.IsInfinity(fontSize) || fontSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSize), "Font size must be a positive finite number.");
        }

        Text = text;
        X = x;
        Y = y;
        Color = color;
        FontSize = fontSize;
        UseWorldCoordinates = useWorldCoordinates;
    }

    /// <summary>
    /// Gets the text content.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Gets the world-space or screen-space X coordinate.
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Gets the world-space or screen-space Y coordinate.
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// Gets the text color.
    /// </summary>
    public Color Color { get; init; }

    /// <summary>
    /// Gets the text font size.
    /// </summary>
    public double FontSize { get; init; }

    /// <summary>
    /// Gets a value indicating whether the coordinates should be transformed through the viewport camera.
    /// </summary>
    public bool UseWorldCoordinates { get; init; }
}
