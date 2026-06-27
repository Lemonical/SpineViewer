using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces a neutral loaded-session silhouette until runtime-backed render geometry is introduced.
/// </summary>
public sealed class SessionPlaceholderOverlaySource : IViewportOverlaySource
{
    private readonly IViewportInspectionOverlayFactory _inspectionOverlayFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionPlaceholderOverlaySource"/> class.
    /// </summary>
    public SessionPlaceholderOverlaySource(IViewportInspectionOverlayFactory inspectionOverlayFactory)
    {
        _inspectionOverlayFactory =
            inspectionOverlayFactory ?? throw new ArgumentNullException(nameof(inspectionOverlayFactory));
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveSession)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return _inspectionOverlayFactory.CreatePlaceholderLines(context);
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }
}
