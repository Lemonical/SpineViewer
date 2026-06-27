using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport slot-outline overlay.
/// </summary>
public sealed class SlotOutlineOverlaySource : IViewportOverlaySource
{
    private readonly IViewportInspectionOverlayFactory _inspectionOverlayFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotOutlineOverlaySource"/> class.
    /// </summary>
    public SlotOutlineOverlaySource(IViewportInspectionOverlayFactory inspectionOverlayFactory)
    {
        _inspectionOverlayFactory =
            inspectionOverlayFactory ?? throw new ArgumentNullException(nameof(inspectionOverlayFactory));
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        if (!context.ViewportState.ShowSlotOutlines)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return _inspectionOverlayFactory.CreateSlotOutlineLines(context);
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }
}
