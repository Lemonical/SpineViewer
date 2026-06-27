using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport labels overlay.
/// </summary>
public sealed class LabelOverlaySource : IViewportOverlaySource
{
    private readonly IViewportInspectionOverlayFactory _inspectionOverlayFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelOverlaySource"/> class.
    /// </summary>
    public LabelOverlaySource(IViewportInspectionOverlayFactory inspectionOverlayFactory)
    {
        _inspectionOverlayFactory =
            inspectionOverlayFactory ?? throw new ArgumentNullException(nameof(inspectionOverlayFactory));
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayLine>();
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        if (!context.ViewportState.ShowLabels)
        {
            return Array.Empty<ViewportOverlayText>();
        }

        return _inspectionOverlayFactory.CreateLabelText(context);
    }
}
