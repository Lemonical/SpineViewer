using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport grid overlay.
/// </summary>
public sealed class GridOverlaySource : IViewportOverlaySource
{
    private static readonly Color GridColor = Color.FromArgb(0x44, 0x6B, 0x78, 0x8C);
    private const int DefaultGridExtent = 600;
    private const int OverscanSteps = 2;
    private const int GridStep = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="GridOverlaySource"/> class.
    /// </summary>
    public GridOverlaySource()
    {
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.ViewportState.ShowGrid)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        (int minX, int maxX, int minY, int maxY) = ResolveVisibleWorldBounds(context);
        List<ViewportOverlayLine> lines = [];

        for (int position = minX; position <= maxX; position += GridStep)
        {
            lines.Add(new ViewportOverlayLine(position, minY, position, maxY, GridColor, 1.0, false));
        }

        for (int position = minY; position <= maxY; position += GridStep)
        {
            lines.Add(new ViewportOverlayLine(minX, position, maxX, position, GridColor, 1.0, false));
        }

        return lines;
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }

    private static (int MinX, int MaxX, int MinY, int MaxY) ResolveVisibleWorldBounds(ViewportOverlayContext context)
    {
        if (!context.HostLayout.HasSurface)
        {
            return (-DefaultGridExtent, DefaultGridExtent, -DefaultGridExtent, DefaultGridExtent);
        }

        double viewportCenterX = (context.HostLayout.Width / 2.0) + context.ViewportState.OffsetX;
        double viewportCenterY = (context.HostLayout.Height / 2.0) + context.ViewportState.OffsetY;
        double worldMinX = (0.0 - viewportCenterX) / context.ViewportState.Zoom;
        double worldMaxX = (context.HostLayout.Width - viewportCenterX) / context.ViewportState.Zoom;
        double worldMinY = (0.0 - viewportCenterY) / context.ViewportState.Zoom;
        double worldMaxY = (context.HostLayout.Height - viewportCenterY) / context.ViewportState.Zoom;
        int overscanDistance = GridStep * OverscanSteps;

        return
        (
            SnapDown(Math.Min(worldMinX, worldMaxX) - overscanDistance),
            SnapUp(Math.Max(worldMinX, worldMaxX) + overscanDistance),
            SnapDown(Math.Min(worldMinY, worldMaxY) - overscanDistance),
            SnapUp(Math.Max(worldMinY, worldMaxY) + overscanDistance)
        );
    }

    private static int SnapDown(double value)
    {
        return (int)Math.Floor(value / GridStep) * GridStep;
    }

    private static int SnapUp(double value)
    {
        return (int)Math.Ceiling(value / GridStep) * GridStep;
    }
}
