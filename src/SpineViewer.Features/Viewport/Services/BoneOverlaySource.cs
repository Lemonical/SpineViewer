using Avalonia.Media;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Produces the viewport bone-guide overlay.
/// </summary>
public sealed class BoneOverlaySource : IViewportOverlaySource
{
    private readonly IViewportInspectionOverlayFactory _inspectionOverlayFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoneOverlaySource"/> class.
    /// </summary>
    public BoneOverlaySource(IViewportInspectionOverlayFactory inspectionOverlayFactory)
    {
        _inspectionOverlayFactory =
            inspectionOverlayFactory ?? throw new ArgumentNullException(nameof(inspectionOverlayFactory));
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveSession || !context.ViewportState.ShowBones)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return _inspectionOverlayFactory.CreateBoneLines(context);
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateOverlayText(ViewportOverlayContext context)
    {
        return Array.Empty<ViewportOverlayText>();
    }
}
