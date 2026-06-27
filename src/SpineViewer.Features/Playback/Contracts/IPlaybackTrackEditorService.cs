using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Models;

namespace SpineViewer.Features.Playback.Contracts;

/// <summary>
/// Applies immutable multi-track editing operations to playback state snapshots.
/// </summary>
public interface IPlaybackTrackEditorService
{
    /// <summary>
    /// Adds one new playback track using the supplied animation and defaults.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="inspection">The structured project inspection snapshot.</param>
    /// <param name="animationName">The animation to assign to the new track.</param>
    /// <param name="isLooping">The loop setting for the new track.</param>
    /// <param name="timeScale">The time scale for the new track.</param>
    /// <param name="mixDuration">The mix duration for the new track.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState AddTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        string animationName,
        bool isLooping,
        double timeScale,
        TimeSpan mixDuration);

    /// <summary>
    /// Moves one track to a new index in the track stack.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="inspection">The structured project inspection snapshot.</param>
    /// <param name="trackId">The track to move.</param>
    /// <param name="targetIndex">The zero-based destination index.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState MoveTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        int targetIndex);

    /// <summary>
    /// Removes one track from the track stack.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="inspection">The structured project inspection snapshot.</param>
    /// <param name="trackId">The track to remove.</param>
    /// <returns>The updated playback state.</returns>
    PlaybackState RemoveTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId);

    /// <summary>
    /// Updates the animation name assigned to one track.
    /// </summary>
    PlaybackState SetTrackAnimation(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        string animationName);

    /// <summary>
    /// Updates the enabled state of one track.
    /// </summary>
    PlaybackState SetTrackEnabled(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        bool isEnabled);

    /// <summary>
    /// Updates the loop state of one track.
    /// </summary>
    PlaybackState SetTrackLooping(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        bool isLooping);

    /// <summary>
    /// Updates the mix duration of one track.
    /// </summary>
    PlaybackState SetTrackMixDuration(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        TimeSpan mixDuration);

    /// <summary>
    /// Updates the time scale of one track.
    /// </summary>
    PlaybackState SetTrackTimeScale(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        double timeScale);

    /// <summary>
    /// Validates the current track configuration for user-facing editing feedback.
    /// </summary>
    /// <param name="playbackState">The current playback state.</param>
    /// <param name="inspection">The structured project inspection snapshot.</param>
    /// <param name="runtime">The runtime selected for the current session.</param>
    /// <returns>The current validation issues.</returns>
    IReadOnlyList<PlaybackTrackValidationIssue> Validate(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        SpineRuntimeDescriptor runtime);
}
