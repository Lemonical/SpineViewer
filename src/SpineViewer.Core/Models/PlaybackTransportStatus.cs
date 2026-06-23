namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the visible transport mode for the active playback timeline.
/// </summary>
public enum PlaybackTransportStatus
{
    /// <summary>
    /// Playback is stopped at the beginning of the timeline.
    /// </summary>
    Stopped = 0,

    /// <summary>
    /// Playback is actively advancing.
    /// </summary>
    Playing = 1,

    /// <summary>
    /// Playback is paused at a specific time.
    /// </summary>
    Paused = 2,
}
