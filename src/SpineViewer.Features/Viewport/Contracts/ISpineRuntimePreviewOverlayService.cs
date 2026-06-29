using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Captures runtime-synchronized debug overlay primitives from the active Spine preview implementation.
/// </summary>
public interface ISpineRuntimePreviewOverlayService
{
    /// <summary>
    /// Captures the current overlay snapshot for the supplied session.
    /// </summary>
    /// <param name="session">The session whose runtime preview overlay snapshot should be captured.</param>
    /// <returns>The animated overlay snapshot, or <see langword="null" /> when the preview cannot be inspected.</returns>
    SpineRuntimePreviewOverlaySnapshot? CaptureOverlaySnapshot(SpineProjectSession session);
}
