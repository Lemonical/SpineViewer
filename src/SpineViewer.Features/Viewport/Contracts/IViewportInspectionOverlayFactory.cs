using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Builds inspection-driven viewport overlay primitives for the current session.
/// </summary>
public interface IViewportInspectionOverlayFactory
{
    /// <summary>
    /// Creates the baseline placeholder or attachment silhouette lines.
    /// </summary>
    IReadOnlyList<ViewportOverlayLine> CreatePlaceholderLines(ViewportOverlayContext context);

    /// <summary>
    /// Creates bone overlay lines.
    /// </summary>
    IReadOnlyList<ViewportOverlayLine> CreateBoneLines(ViewportOverlayContext context);

    /// <summary>
    /// Creates bounds overlay lines.
    /// </summary>
    IReadOnlyList<ViewportOverlayLine> CreateBoundsLines(ViewportOverlayContext context);

    /// <summary>
    /// Creates mesh or wireframe overlay lines.
    /// </summary>
    IReadOnlyList<ViewportOverlayLine> CreateMeshLines(ViewportOverlayContext context);

    /// <summary>
    /// Creates slot-outline overlay lines.
    /// </summary>
    IReadOnlyList<ViewportOverlayLine> CreateSlotOutlineLines(ViewportOverlayContext context);

    /// <summary>
    /// Creates labels overlay text primitives.
    /// </summary>
    IReadOnlyList<ViewportOverlayText> CreateLabelText(ViewportOverlayContext context);

    /// <summary>
    /// Creates missing-resource and unsupported-feature indicator text primitives.
    /// </summary>
    IReadOnlyList<ViewportOverlayText> CreateDiagnosticText(ViewportOverlayContext context);
}
