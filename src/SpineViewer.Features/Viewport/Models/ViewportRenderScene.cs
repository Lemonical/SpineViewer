using Avalonia.Media;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Represents the immutable render scene consumed by the view-only viewport host control.
/// </summary>
public sealed record ViewportRenderScene
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportRenderScene"/> class.
    /// </summary>
    /// <param name="backgroundColor">The scene background color.</param>
    /// <param name="transform">The camera transform that projects world-space geometry into screen space.</param>
    /// <param name="worldLines">The world-space overlay lines to draw for this frame.</param>
    /// <param name="frameVersion">The monotonically increasing frame version.</param>
    /// <param name="hasActiveSession">Indicates whether the scene represents an active session.</param>
    /// <param name="sessionName">The active session display name, if any.</param>
    public ViewportRenderScene(
        Color backgroundColor,
        ViewportRenderTransform transform,
        IEnumerable<ViewportOverlayLine> worldLines,
        long frameVersion,
        bool hasActiveSession,
        string? sessionName)
        : this(
            backgroundColor,
            transform,
            worldLines,
            Array.Empty<ViewportOverlayText>(),
            frameVersion,
            hasActiveSession,
            sessionName,
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportRenderScene"/> class.
    /// </summary>
    /// <param name="backgroundColor">The scene background color.</param>
    /// <param name="transform">The camera transform that projects world-space geometry into screen space.</param>
    /// <param name="worldLines">The world-space overlay lines to draw for this frame.</param>
    /// <param name="frameVersion">The monotonically increasing frame version.</param>
    /// <param name="hasActiveSession">Indicates whether the scene represents an active session.</param>
    /// <param name="sessionName">The active session display name, if any.</param>
    /// <param name="previewSession">The active session used for real Spine runtime rendering, if any.</param>
    public ViewportRenderScene(
        Color backgroundColor,
        ViewportRenderTransform transform,
        IEnumerable<ViewportOverlayLine> worldLines,
        long frameVersion,
        bool hasActiveSession,
        string? sessionName,
        SpineProjectSession? previewSession)
        : this(
            backgroundColor,
            transform,
            worldLines,
            Array.Empty<ViewportOverlayText>(),
            frameVersion,
            hasActiveSession,
            sessionName,
            previewSession)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportRenderScene"/> class.
    /// </summary>
    /// <param name="backgroundColor">The scene background color.</param>
    /// <param name="transform">The camera transform that projects world-space geometry into screen space.</param>
    /// <param name="worldLines">The world-space overlay lines to draw for this frame.</param>
    /// <param name="overlayText">The text overlays to draw for this frame.</param>
    /// <param name="frameVersion">The monotonically increasing frame version.</param>
    /// <param name="hasActiveSession">Indicates whether the scene represents an active session.</param>
    /// <param name="sessionName">The active session display name, if any.</param>
    public ViewportRenderScene(
        Color backgroundColor,
        ViewportRenderTransform transform,
        IEnumerable<ViewportOverlayLine> worldLines,
        IEnumerable<ViewportOverlayText> overlayText,
        long frameVersion,
        bool hasActiveSession,
        string? sessionName)
        : this(
            backgroundColor,
            transform,
            worldLines,
            overlayText,
            frameVersion,
            hasActiveSession,
            sessionName,
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportRenderScene"/> class.
    /// </summary>
    /// <param name="backgroundColor">The scene background color.</param>
    /// <param name="transform">The camera transform that projects world-space geometry into screen space.</param>
    /// <param name="worldLines">The world-space overlay lines to draw for this frame.</param>
    /// <param name="overlayText">The text overlays to draw for this frame.</param>
    /// <param name="frameVersion">The monotonically increasing frame version.</param>
    /// <param name="hasActiveSession">Indicates whether the scene represents an active session.</param>
    /// <param name="sessionName">The active session display name, if any.</param>
    /// <param name="previewSession">The active session used for real Spine runtime rendering, if any.</param>
    public ViewportRenderScene(
        Color backgroundColor,
        ViewportRenderTransform transform,
        IEnumerable<ViewportOverlayLine> worldLines,
        IEnumerable<ViewportOverlayText> overlayText,
        long frameVersion,
        bool hasActiveSession,
        string? sessionName,
        SpineProjectSession? previewSession)
    {
        if (frameVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameVersion), "Frame version cannot be negative.");
        }

        BackgroundColor = backgroundColor;
        Transform = transform ?? throw new ArgumentNullException(nameof(transform));
        WorldLines = worldLines?.ToArray() ?? throw new ArgumentNullException(nameof(worldLines));
        OverlayText = overlayText?.ToArray() ?? throw new ArgumentNullException(nameof(overlayText));
        FrameVersion = frameVersion;
        HasActiveSession = hasActiveSession;
        SessionName = sessionName ?? string.Empty;
        PreviewSession = previewSession;
        ContentBounds = TryCreateContentBounds(WorldLines);
    }

    /// <summary>
    /// Gets the default empty render scene.
    /// </summary>
    public static ViewportRenderScene Empty { get; } = new(
        Color.FromRgb(0x0D, 0x11, 0x18),
        new ViewportRenderTransform(1.0, 0.0, 0.0),
        Array.Empty<ViewportOverlayLine>(),
        Array.Empty<ViewportOverlayText>(),
        0,
        false,
        string.Empty);

    /// <summary>
    /// Gets the scene background color.
    /// </summary>
    public Color BackgroundColor { get; init; }

    /// <summary>
    /// Gets the camera transform that projects world-space geometry into screen space.
    /// </summary>
    public ViewportRenderTransform Transform { get; init; }

    /// <summary>
    /// Gets the world-space overlay lines to draw for this frame.
    /// </summary>
    public IReadOnlyList<ViewportOverlayLine> WorldLines { get; init; }

    /// <summary>
    /// Gets the text overlays to draw for this frame.
    /// </summary>
    public IReadOnlyList<ViewportOverlayText> OverlayText { get; init; }

    /// <summary>
    /// Gets the fit-to-view bounds derived from the scene's content lines, if any.
    /// </summary>
    public ViewportContentBounds? ContentBounds { get; init; }

    /// <summary>
    /// Gets the monotonically increasing frame version.
    /// </summary>
    public long FrameVersion { get; init; }

    /// <summary>
    /// Gets a value indicating whether the scene represents an active session.
    /// </summary>
    public bool HasActiveSession { get; init; }

    /// <summary>
    /// Gets the active session display name, if any.
    /// </summary>
    public string SessionName { get; init; }

    /// <summary>
    /// Gets the active session used for real Spine runtime rendering, if any.
    /// </summary>
    public SpineProjectSession? PreviewSession { get; init; }

    private static ViewportContentBounds? TryCreateContentBounds(
        IReadOnlyList<ViewportOverlayLine> worldLines)
    {
        IEnumerable<ViewportOverlayLine> contentLines = worldLines
            .Where(static line => line.IncludeInContentBounds);

        if (!contentLines.Any())
        {
            return null;
        }

        double minimumX = double.PositiveInfinity;
        double minimumY = double.PositiveInfinity;
        double maximumX = double.NegativeInfinity;
        double maximumY = double.NegativeInfinity;

        foreach (ViewportOverlayLine line in contentLines)
        {
            minimumX = Math.Min(minimumX, Math.Min(line.StartX, line.EndX));
            minimumY = Math.Min(minimumY, Math.Min(line.StartY, line.EndY));
            maximumX = Math.Max(maximumX, Math.Max(line.StartX, line.EndX));
            maximumY = Math.Max(maximumY, Math.Max(line.StartY, line.EndY));
        }

        return new ViewportContentBounds(minimumX, minimumY, maximumX, maximumY);
    }
}
