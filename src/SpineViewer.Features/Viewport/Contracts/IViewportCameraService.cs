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
    /// Resets the camera transform while preserving overlay and background selections.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <returns>The reset viewport state.</returns>
    ViewportState Reset(ViewportState viewportState);

    /// <summary>
    /// Fits the supplied content bounds into the current render host.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <param name="hostLayout">The current render-host layout.</param>
    /// <param name="contentBounds">The content bounds to fit.</param>
    /// <returns>The updated viewport state.</returns>
    ViewportState FitToView(
        ViewportState viewportState,
        ViewportHostLayout hostLayout,
        ViewportContentBounds contentBounds);

    /// <summary>
    /// Applies a zoom multiplier to the viewport camera state.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <param name="zoomFactor">The zoom multiplier to apply.</param>
    /// <returns>The updated viewport state.</returns>
    ViewportState Zoom(
        ViewportState viewportState,
        double zoomFactor);

    /// <summary>
    /// Applies a zoom multiplier while keeping the supplied screen point anchored.
    /// </summary>
    /// <param name="viewportState">The current viewport state.</param>
    /// <param name="hostLayout">The current render-host layout.</param>
    /// <param name="screenX">The anchored screen-space X coordinate.</param>
    /// <param name="screenY">The anchored screen-space Y coordinate.</param>
    /// <param name="zoomFactor">The zoom multiplier to apply.</param>
    /// <returns>The updated viewport state.</returns>
    ViewportState ZoomAtPoint(
        ViewportState viewportState,
        ViewportHostLayout hostLayout,
        double screenX,
        double screenY,
        double zoomFactor);
}
