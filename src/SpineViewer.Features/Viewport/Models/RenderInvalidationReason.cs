namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Identifies why the viewport render scene needs a new frame.
/// </summary>
public enum RenderInvalidationReason
{
    /// <summary>
    /// The open session changed.
    /// </summary>
    SessionChanged,

    /// <summary>
    /// The viewport camera or overlay state changed.
    /// </summary>
    ViewportChanged,

    /// <summary>
    /// The playback state changed.
    /// </summary>
    PlaybackChanged,

    /// <summary>
    /// A scheduled frame tick requested a redraw.
    /// </summary>
    FrameTick,

    /// <summary>
    /// The viewport host size changed.
    /// </summary>
    HostLayoutChanged,
}
