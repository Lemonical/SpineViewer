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
    private const int AxisExtent = 700;

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

        return
        [
            new ViewportOverlayLine(-AxisExtent, 0, AxisExtent, 0, HorizontalAxisColor, 1.8, false),
            new ViewportOverlayLine(0, -AxisExtent, 0, AxisExtent, VerticalAxisColor, 1.8, false),
        ];
    }
}
