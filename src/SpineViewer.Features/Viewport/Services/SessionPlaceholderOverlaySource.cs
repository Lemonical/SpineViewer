using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces a neutral loaded-session silhouette until runtime-backed render geometry is introduced.
/// </summary>
public sealed class SessionPlaceholderOverlaySource : IViewportOverlaySource
{
    private static readonly Color PlaceholderColor = Color.FromRgb(0x8B, 0xC1, 0xD6);
    private const int HalfWidth = 90;
    private const int HalfHeight = 130;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionPlaceholderOverlaySource"/> class.
    /// </summary>
    public SessionPlaceholderOverlaySource()
    {
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveSession)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return
        [
            new ViewportOverlayLine(-HalfWidth, -HalfHeight, HalfWidth, -HalfHeight, PlaceholderColor, 2.0),
            new ViewportOverlayLine(HalfWidth, -HalfHeight, HalfWidth, HalfHeight, PlaceholderColor, 2.0),
            new ViewportOverlayLine(HalfWidth, HalfHeight, -HalfWidth, HalfHeight, PlaceholderColor, 2.0),
            new ViewportOverlayLine(-HalfWidth, HalfHeight, -HalfWidth, -HalfHeight, PlaceholderColor, 2.0),
            new ViewportOverlayLine(-HalfWidth, -HalfHeight, HalfWidth, HalfHeight, PlaceholderColor, 1.2),
            new ViewportOverlayLine(HalfWidth, -HalfHeight, -HalfWidth, HalfHeight, PlaceholderColor, 1.2),
        ];
    }
}
