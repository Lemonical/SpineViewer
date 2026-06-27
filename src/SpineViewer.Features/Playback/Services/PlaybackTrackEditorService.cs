using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Contracts;
using SpineViewer.Features.Playback.Models;

namespace SpineViewer.Features.Playback.Services;

/// <summary>
/// Applies immutable multi-track editing operations and duration recalculation to playback state.
/// </summary>
public sealed class PlaybackTrackEditorService : IPlaybackTrackEditorService
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    public PlaybackState AddTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        string animationName,
        bool isLooping,
        double timeScale,
        TimeSpan mixDuration)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentException.ThrowIfNullOrWhiteSpace(animationName);

        List<AnimationTrackState> tracks = playbackState.Tracks.OrderBy(static track => track.TrackIndex).ToList();
        tracks.Add(
            new AnimationTrackState(
                Guid.NewGuid(),
                tracks.Count,
                animationName,
                isLooping,
                timeScale,
                mixDuration,
                true));

        return RebuildPlaybackState(playbackState, inspection, tracks);
    }

    /// <inheritdoc />
    public PlaybackState MoveTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        int targetIndex)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);

        List<AnimationTrackState> tracks = playbackState.Tracks.OrderBy(static track => track.TrackIndex).ToList();
        int currentIndex = tracks.FindIndex(track => track.TrackId == trackId);
        if (currentIndex < 0)
        {
            return playbackState;
        }

        int boundedTargetIndex = Math.Clamp(targetIndex, 0, tracks.Count - 1);
        if (boundedTargetIndex == currentIndex)
        {
            return playbackState;
        }

        AnimationTrackState track = tracks[currentIndex];
        tracks.RemoveAt(currentIndex);
        tracks.Insert(boundedTargetIndex, track);

        return RebuildPlaybackState(playbackState, inspection, tracks);
    }

    /// <inheritdoc />
    public PlaybackState RemoveTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);

        List<AnimationTrackState> tracks = playbackState.Tracks
            .Where(track => track.TrackId != trackId)
            .OrderBy(static track => track.TrackIndex)
            .ToList();

        return RebuildPlaybackState(playbackState, inspection, tracks);
    }

    /// <inheritdoc />
    public PlaybackState SetTrackAnimation(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        string animationName)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentException.ThrowIfNullOrWhiteSpace(animationName);

        return UpdateTrack(
            playbackState,
            inspection,
            trackId,
            track => track with
            {
                AnimationName = animationName,
            });
    }

    /// <inheritdoc />
    public PlaybackState SetTrackEnabled(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);

        return UpdateTrack(
            playbackState,
            inspection,
            trackId,
            track => track with
            {
                IsEnabled = isEnabled,
            });
    }

    /// <inheritdoc />
    public PlaybackState SetTrackLooping(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        bool isLooping)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);

        return UpdateTrack(
            playbackState,
            inspection,
            trackId,
            track => track with
            {
                IsLooping = isLooping,
            });
    }

    /// <inheritdoc />
    public PlaybackState SetTrackMixDuration(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        TimeSpan mixDuration)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);

        return UpdateTrack(
            playbackState,
            inspection,
            trackId,
            track => track with
            {
                MixDuration = mixDuration,
            });
    }

    /// <inheritdoc />
    public PlaybackState SetTrackTimeScale(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        double timeScale)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);

        return UpdateTrack(
            playbackState,
            inspection,
            trackId,
            track => track with
            {
                TimeScale = timeScale,
            });
    }

    /// <inheritdoc />
    public IReadOnlyList<PlaybackTrackValidationIssue> Validate(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        SpineRuntimeDescriptor runtime)
    {
        ArgumentNullException.ThrowIfNull(playbackState);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(runtime);

        List<PlaybackTrackValidationIssue> issues = [];
        HashSet<string> animationNames = inspection.Animations
            .Select(static animation => animation.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (playbackState.Tracks.Count == 0)
        {
            issues.Add(
                new PlaybackTrackValidationIssue(
                    "track-stack-empty",
                    ViewerDiagnosticSeverity.Warning,
                    "Add at least one track to preview an animation."));
        }

        if (inspection.Animations.Count == 0 && playbackState.Tracks.Count > 0)
        {
            issues.Add(
                new PlaybackTrackValidationIssue(
                    "track-stack-no-animations",
                    ViewerDiagnosticSeverity.Error,
                    "This export does not expose inspected animation names for track assignment."));
        }

        foreach (AnimationTrackState track in playbackState.Tracks)
        {
            if (!animationNames.Contains(track.AnimationName))
            {
                issues.Add(
                    new PlaybackTrackValidationIssue(
                        $"track-animation-missing-{track.TrackId:N}",
                        ViewerDiagnosticSeverity.Error,
                        $"Track {track.TrackIndex + 1} references '{track.AnimationName}', which is not available in the inspected export."));
            }
        }

        int enabledTrackCount = playbackState.Tracks.Count(static track => track.IsEnabled);
        if (playbackState.Tracks.Count > 0 && enabledTrackCount == 0)
        {
            issues.Add(
                new PlaybackTrackValidationIssue(
                    "track-stack-all-disabled",
                    ViewerDiagnosticSeverity.Warning,
                    "All tracks are disabled, so playback will not show layered animation changes."));
        }

        if (enabledTrackCount > 1 &&
            !runtime.Capabilities.Supports(SpineRuntimeFeature.MultipleTracks))
        {
            issues.Add(
                new PlaybackTrackValidationIssue(
                    "track-stack-multiple-tracks-unsupported",
                    ViewerDiagnosticSeverity.Error,
                    $"{runtime.DisplayName} does not support multiple active animation tracks."));
        }

        return issues;
    }

    private static TimeSpan DetermineDuration(
        IReadOnlyList<AnimationTrackState> tracks,
        SpineProjectInspection inspection)
    {
        TimeSpan duration = tracks
            .Select(track => inspection.Animations.FirstOrDefault(animation => animation.Name == track.AnimationName)?.Duration ?? TimeSpan.Zero)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

        return duration > TimeSpan.Zero
            ? duration
            : DefaultDuration;
    }

    private static PlaybackState NormalizeState(
        PlaybackState originalState,
        TimeSpan duration,
        IReadOnlyList<AnimationTrackState> tracks)
    {
        TimeSpan currentTime = originalState.CurrentTime > duration
            ? duration
            : originalState.CurrentTime;

        PlaybackTransportStatus status = originalState.Status;
        if (status == PlaybackTransportStatus.Playing &&
            !originalState.IsLooping &&
            currentTime >= duration)
        {
            status = PlaybackTransportStatus.Stopped;
            currentTime = duration;
        }

        if (status == PlaybackTransportStatus.Stopped)
        {
            currentTime = TimeSpan.Zero;
        }

        return originalState with
        {
            Status = status,
            CurrentTime = currentTime,
            Duration = duration,
            Tracks = tracks,
        };
    }

    private static PlaybackState RebuildPlaybackState(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        IEnumerable<AnimationTrackState> tracks)
    {
        IReadOnlyList<AnimationTrackState> normalizedTracks = tracks
            .OrderBy(static track => track.TrackIndex)
            .Select(
                (track, index) => track with
                {
                    TrackIndex = index,
                })
            .ToArray();

        return NormalizeState(
            playbackState,
            DetermineDuration(normalizedTracks, inspection),
            normalizedTracks);
    }

    private static PlaybackState UpdateTrack(
        PlaybackState playbackState,
        SpineProjectInspection inspection,
        Guid trackId,
        Func<AnimationTrackState, AnimationTrackState> update)
    {
        List<AnimationTrackState> tracks = playbackState.Tracks
            .OrderBy(static track => track.TrackIndex)
            .Select(track => track.TrackId == trackId ? update(track) : track)
            .ToList();

        return RebuildPlaybackState(playbackState, inspection, tracks);
    }
}
