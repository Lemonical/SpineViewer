using SkiaSharp;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Rendering;

/// <summary>
/// Defines the runtime-specific preview driver used by the viewport render host.
/// </summary>
internal interface ISpineRuntimePreviewDriver : IDisposable
{
    /// <summary>
    /// Draws the current synchronized skeleton pose to the supplied canvas.
    /// </summary>
    /// <param name="canvas">The Skia canvas for the active viewport frame.</param>
    void Draw(SKCanvas canvas);

    /// <summary>
    /// Synchronizes the runtime state to the supplied playback and skin selection.
    /// </summary>
    /// <param name="playbackState">The playback state to apply.</param>
    /// <param name="selectedSkinName">The selected skin name, if any.</param>
    void Synchronize(PlaybackState playbackState, string? selectedSkinName);

    /// <summary>
    /// Measures the current synchronized skeleton bounds in world space.
    /// </summary>
    /// <returns>The synchronized world-space bounds, or <see langword="null" /> when the preview is empty.</returns>
    ViewportContentBounds? GetContentBounds();

    /// <summary>
    /// Captures the current synchronized overlay snapshot from the animated runtime pose.
    /// </summary>
    /// <returns>The synchronized overlay snapshot.</returns>
    SpineRuntimePreviewOverlaySnapshot GetOverlaySnapshot();
}
