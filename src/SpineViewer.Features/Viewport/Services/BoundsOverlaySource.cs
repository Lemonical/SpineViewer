using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport bounds overlay.
/// </summary>
public sealed class BoundsOverlaySource : IViewportOverlaySource
{
    private static readonly Color BoundsColor = Color.FromRgb(0x9A, 0xD1, 0x8B);
    private const int HalfWidth = 130;
    private const int HalfHeight = 170;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundsOverlaySource"/> class.
    /// </summary>
    public BoundsOverlaySource()
    {
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveSession || !context.ViewportState.ShowBounds)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return
        [
            new ViewportOverlayLine(-HalfWidth, -HalfHeight, HalfWidth, -HalfHeight, BoundsColor, 2.4),
            new ViewportOverlayLine(HalfWidth, -HalfHeight, HalfWidth, HalfHeight, BoundsColor, 2.4),
            new ViewportOverlayLine(HalfWidth, HalfHeight, -HalfWidth, HalfHeight, BoundsColor, 2.4),
            new ViewportOverlayLine(-HalfWidth, HalfHeight, -HalfWidth, -HalfHeight, BoundsColor, 2.4),
        ];
    }
}
