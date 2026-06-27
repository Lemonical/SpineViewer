using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport mesh or wireframe overlay.
/// </summary>
public sealed class MeshOverlaySource : IViewportOverlaySource
{
    private readonly IViewportInspectionOverlayFactory _inspectionOverlayFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MeshOverlaySource"/> class.
    /// </summary>
    public MeshOverlaySource(IViewportInspectionOverlayFactory inspectionOverlayFactory)
    {
        _inspectionOverlayFactory =
            inspectionOverlayFactory ?? throw new ArgumentNullException(nameof(inspectionOverlayFactory));
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        if (!context.ViewportState.ShowMeshWireframe)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return _inspectionOverlayFactory.CreateMeshLines(context);
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }
}
