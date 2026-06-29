using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Measures world-space content bounds for the active Spine runtime preview.
/// </summary>
public interface ISpineRuntimePreviewBoundsService
{
    /// <summary>
    /// Measures the current preview content bounds for the supplied session.
    /// </summary>
    /// <param name="session">The session whose runtime preview bounds should be measured.</param>
    /// <returns>The measured bounds, or <see langword="null" /> when the preview cannot be measured.</returns>
    ViewportContentBounds? MeasureContentBounds(SpineProjectSession session);
}
