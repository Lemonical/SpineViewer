using SpineViewer.Core.Models;

namespace SpineViewer.Features.Playback.Contracts;

/// <summary>
/// Applies playback transport transitions to immutable playback state snapshots.
/// </summary>
public interface IPlaybackStateService
{
    /// <summary>
    /// Advances playback by the supplied elapsed time.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="elapsed">The elapsed time to apply.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState Advance(PlaybackState playbackState, TimeSpan elapsed);

    /// <summary>
    /// Pauses playback at the current time.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState Pause(PlaybackState playbackState);

    /// <summary>
    /// Starts playback from the current timeline position.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState Play(PlaybackState playbackState);

    /// <summary>
    /// Restarts playback from the beginning of the timeline.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState Restart(PlaybackState playbackState);

    /// <summary>
    /// Scrubs to a new timeline position.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="currentTime">The new timeline position.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState ScrubTo(PlaybackState playbackState, TimeSpan currentTime);

    /// <summary>
    /// Sets the global playback speed multiplier.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="speed">The new speed multiplier.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState SetSpeed(PlaybackState playbackState, double speed);

    /// <summary>
    /// Sets the loop toggle.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="isLooping">The new loop value.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState SetLooping(PlaybackState playbackState, bool isLooping);

    /// <summary>
    /// Advances the timeline by one visible frame step.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState StepForward(PlaybackState playbackState);

    /// <summary>
    /// Stops playback and returns the timeline to the beginning.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState Stop(PlaybackState playbackState);
}
