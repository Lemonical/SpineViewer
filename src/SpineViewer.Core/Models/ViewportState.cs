using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the current camera and overlay state for the viewport.
/// </summary>
public sealed record ViewportState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportState"/> class with viewer-friendly defaults.
    /// </summary>
    public ViewportState()
        : this(1.0, 0.0, 0.0, true, true, false, false, ViewportBackgroundStyle.Studio)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportState"/> class.
    /// </summary>
    /// <param name="zoom">The current zoom factor.</param>
    /// <param name="offsetX">The horizontal camera offset.</param>
    /// <param name="offsetY">The vertical camera offset.</param>
    /// <param name="showGrid">Indicates whether the grid overlay is visible.</param>
    /// <param name="showOrigin">Indicates whether the origin overlay is visible.</param>
    /// <param name="showBones">Indicates whether the bones overlay is visible.</param>
    /// <param name="showBounds">Indicates whether the bounds overlay is visible.</param>
    public ViewportState(
        double zoom,
        double offsetX,
        double offsetY,
        bool showGrid,
        bool showOrigin,
        bool showBones,
        bool showBounds)
        : this(
            zoom,
            offsetX,
            offsetY,
            showGrid,
            showOrigin,
            showBones,
            showBounds,
            ViewportBackgroundStyle.Studio)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportState"/> class.
    /// </summary>
    /// <param name="zoom">The current zoom factor.</param>
    /// <param name="offsetX">The horizontal camera offset.</param>
    /// <param name="offsetY">The vertical camera offset.</param>
    /// <param name="showGrid">Indicates whether the grid overlay is visible.</param>
    /// <param name="showOrigin">Indicates whether the origin overlay is visible.</param>
    /// <param name="showBones">Indicates whether the bones overlay is visible.</param>
    /// <param name="showBounds">Indicates whether the bounds overlay is visible.</param>
    /// <param name="backgroundStyle">The selected viewport background treatment.</param>
    public ViewportState(
        double zoom,
        double offsetX,
        double offsetY,
        bool showGrid,
        bool showOrigin,
        bool showBones,
        bool showBounds,
        ViewportBackgroundStyle backgroundStyle)
    {
        Zoom = Guard.PositiveFinite(zoom, nameof(zoom));
        OffsetX = Guard.Finite(offsetX, nameof(offsetX));
        OffsetY = Guard.Finite(offsetY, nameof(offsetY));
        ShowGrid = showGrid;
        ShowOrigin = showOrigin;
        ShowBones = showBones;
        ShowBounds = showBounds;
        BackgroundStyle = backgroundStyle;
    }

    /// <summary>
    /// Gets the current zoom factor.
    /// </summary>
    public double Zoom { get; init; }

    /// <summary>
    /// Gets the horizontal camera offset.
    /// </summary>
    public double OffsetX { get; init; }

    /// <summary>
    /// Gets the vertical camera offset.
    /// </summary>
    public double OffsetY { get; init; }

    /// <summary>
    /// Gets a value indicating whether the grid overlay is visible.
    /// </summary>
    public bool ShowGrid { get; init; }

    /// <summary>
    /// Gets a value indicating whether the origin overlay is visible.
    /// </summary>
    public bool ShowOrigin { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bones overlay is visible.
    /// </summary>
    public bool ShowBones { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bounds overlay is visible.
    /// </summary>
    public bool ShowBounds { get; init; }

    /// <summary>
    /// Gets the selected viewport background treatment.
    /// </summary>
    public ViewportBackgroundStyle BackgroundStyle { get; init; }
}
