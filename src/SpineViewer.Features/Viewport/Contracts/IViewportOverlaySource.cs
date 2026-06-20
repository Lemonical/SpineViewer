using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Produces one dedicated set of viewport overlay primitives for a render frame.
/// </summary>
public interface IViewportOverlaySource
{
    /// <summary>
    /// Creates the world-space overlay lines for the current frame.
    /// </summary>
    /// <param name="context">The current overlay context.</param>
    /// <returns>The world-space lines contributed by this overlay source.</returns>
    IReadOnlyList<ViewportOverlayLine> CreateWorldLines(ViewportOverlayContext context);
}
