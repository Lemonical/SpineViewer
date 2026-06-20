using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport bone-guide overlay.
/// </summary>
public sealed class BoneOverlaySource : IViewportOverlaySource
{
    private static readonly Color BoneColor = Color.FromRgb(0xF2, 0xCC, 0x8F);

    /// <summary>
    /// Initializes a new instance of the <see cref="BoneOverlaySource"/> class.
    /// </summary>
    public BoneOverlaySource()
    {
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveSession || !context.ViewportState.ShowBones)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return
        [
            new ViewportOverlayLine(0, -80, 0, 40, BoneColor, 2.4),
            new ViewportOverlayLine(0, -20, -55, 25, BoneColor, 2.0),
            new ViewportOverlayLine(0, -20, 55, 25, BoneColor, 2.0),
            new ViewportOverlayLine(0, 40, -35, 120, BoneColor, 2.0),
            new ViewportOverlayLine(0, 40, 35, 120, BoneColor, 2.0),
        ];
    }
}
