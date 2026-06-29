using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport origin overlay.
/// </summary>
public sealed class OriginOverlaySource : IViewportOverlaySource
{
    private static readonly Color HorizontalAxisColor = Color.FromRgb(0xE0, 0x7A, 0x5F);
    private static readonly Color VerticalAxisColor = Color.FromRgb(0x5F, 0xC9, 0xD8);
    private const double FallbackAxisExtent = 700.0;
    private const double VisiblePaddingDip = 48.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginOverlaySource"/> class.
    /// </summary>
    public OriginOverlaySource()
    {
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.ViewportState.ShowOrigin)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        double zoom = Math.Max(context.ViewportState.Zoom, double.Epsilon);
        double worldPadding = VisiblePaddingDip / zoom;

        double minimumX = context.HostLayout.HasSurface
            ? ((-context.HostLayout.Width / 2.0) - context.ViewportState.OffsetX) / zoom - worldPadding
            : -FallbackAxisExtent;
        double maximumX = context.HostLayout.HasSurface
            ? ((context.HostLayout.Width / 2.0) - context.ViewportState.OffsetX) / zoom + worldPadding
            : FallbackAxisExtent;
        double minimumY = context.HostLayout.HasSurface
            ? ((-context.HostLayout.Height / 2.0) - context.ViewportState.OffsetY) / zoom - worldPadding
            : -FallbackAxisExtent;
        double maximumY = context.HostLayout.HasSurface
            ? ((context.HostLayout.Height / 2.0) - context.ViewportState.OffsetY) / zoom + worldPadding
            : FallbackAxisExtent;

        return
        [
            new ViewportOverlayLine(minimumX, 0, maximumX, 0, HorizontalAxisColor, 1.8, false),
            new ViewportOverlayLine(0, minimumY, 0, maximumY, VerticalAxisColor, 1.8, false),
        ];
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }
}
