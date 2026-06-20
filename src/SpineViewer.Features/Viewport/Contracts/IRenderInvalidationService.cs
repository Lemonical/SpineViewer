using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Coordinates viewport redraw requests across workspace, camera, and frame-scheduling boundaries.
/// </summary>
public interface IRenderInvalidationService
{
    /// <summary>
    /// Occurs when a viewport redraw has been requested.
    /// </summary>
    event EventHandler<RenderInvalidatedEventArgs>? RenderInvalidated;

    /// <summary>
    /// Requests a new viewport frame for the given reason.
    /// </summary>
    /// <param name="reason">The reason a new frame is required.</param>
    void RequestInvalidation(RenderInvalidationReason reason);
}
