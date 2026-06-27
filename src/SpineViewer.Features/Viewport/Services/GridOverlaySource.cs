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
    private const int GridExtent = 600;
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

        List<ViewportOverlayLine> lines = [];

        for (int position = -GridExtent; position <= GridExtent; position += GridStep)
        {
            lines.Add(new ViewportOverlayLine(position, -GridExtent, position, GridExtent, GridColor, 1.0, false));
            lines.Add(new ViewportOverlayLine(-GridExtent, position, GridExtent, position, GridColor, 1.0, false));
        }

        return lines;
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }
}
