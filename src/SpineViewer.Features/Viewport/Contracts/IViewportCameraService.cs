using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Owns viewport camera-state mutation and world-to-screen transform calculations.
/// </summary>
public interface IViewportCameraService
{
    /// <summary>
    /// Creates a render transform for the supplied viewport state and host layout.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <param name="hostLayout">The current render-host layout.</param>
    /// <returns>The render transform to apply for the frame.</returns>
    ViewportRenderTransform CreateTransform(
        ViewportState viewportState,
        ViewportHostLayout hostLayout);

    /// <summary>
    /// Applies a screen-space pan delta to the viewport camera state.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <param name="deltaX">The horizontal pan delta.</param>
    /// <param name="deltaY">The vertical pan delta.</param>
    /// <returns>The updated viewport state.</returns>
    ViewportState Pan(
        ViewportState viewportState,
        double deltaX,
        double deltaY);

    /// <summary>
    /// Applies a zoom multiplier to the viewport camera state.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <param name="zoomFactor">The zoom multiplier to apply.</param>
    /// <returns>The updated viewport state.</returns>
    ViewportState Zoom(
        ViewportState viewportState,
        double zoomFactor);
}
