using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces missing-resource and unsupported-feature indicator overlays.
/// </summary>
public sealed class DiagnosticIndicatorOverlaySource : IViewportOverlaySource
{
    private readonly IViewportInspectionOverlayFactory _inspectionOverlayFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticIndicatorOverlaySource"/> class.
    /// </summary>
    public DiagnosticIndicatorOverlaySource(IViewportInspectionOverlayFactory inspectionOverlayFactory)
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
        return _inspectionOverlayFactory.CreateDiagnosticText(context);
    }
}
