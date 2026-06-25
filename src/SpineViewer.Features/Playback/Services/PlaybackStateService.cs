using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Contracts;

namespace SpineViewer.Features.Playback.Services;

/// <summary>
/// Applies transport transitions to immutable playback state snapshots.
/// </summary>
public sealed class PlaybackStateService : IPlaybackStateService
{
    private static readonly TimeSpan FrameStepDuration = TimeSpan.FromSeconds(1.0 / 30.0);

    /// <inheritdoc />
    public PlaybackState Advance(PlaybackState playbackState, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        if (!playbackState.IsPlaying || elapsed <= TimeSpan.Zero)
        {
            return playbackState;
        }

        return MoveToTime(playbackState, playbackState.CurrentTime + Scale(elapsed, playbackState.Speed), playbackState.IsPlaying);
    }

    /// <inheritdoc />
    public PlaybackState Pause(PlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        if (playbackState.IsStopped)
        {
            return playbackState;
        }

        return playbackState with
        {
            Status = PlaybackTransportStatus.Paused,
        };
    }

    /// <inheritdoc />
    public PlaybackState Play(PlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        TimeSpan startTime = playbackState.CurrentTime >= playbackState.Duration && !playbackState.IsLooping
            ? TimeSpan.Zero
            : playbackState.CurrentTime;

        return playbackState with
        {
            Status = PlaybackTransportStatus.Playing,
            CurrentTime = startTime,
        };
    }

    /// <inheritdoc />
    public PlaybackState Restart(PlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        return playbackState with
        {
            Status = PlaybackTransportStatus.Playing,
            CurrentTime = TimeSpan.Zero,
        };
    }

    /// <inheritdoc />
    public PlaybackState ScrubTo(PlaybackState playbackState, TimeSpan currentTime)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        return MoveToTime(playbackState, currentTime, playbackState.IsPlaying);
    }

    /// <inheritdoc />
    public PlaybackState SetLooping(PlaybackState playbackState, bool isLooping)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        return playbackState with
        {
            IsLooping = isLooping,
        };
    }

    /// <inheritdoc />
    public PlaybackState SetSpeed(PlaybackState playbackState, double speed)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        return playbackState with
        {
            Speed = speed,
        };
    }

    /// <inheritdoc />
    public PlaybackState StepForward(PlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        PlaybackState advancedState = MoveToTime(playbackState, playbackState.CurrentTime + FrameStepDuration, false);
        return advancedState with
        {
            Status = advancedState.CurrentTime == TimeSpan.Zero
                ? PlaybackTransportStatus.Stopped
                : PlaybackTransportStatus.Paused,
        };
    }

    /// <inheritdoc />
    public PlaybackState Stop(PlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        return playbackState with
        {
            Status = PlaybackTransportStatus.Stopped,
            CurrentTime = TimeSpan.Zero,
        };
    }

    private static PlaybackState MoveToTime(
        PlaybackState playbackState,
        TimeSpan requestedTime,
        bool keepPlaying)
    {
        TimeSpan duration = playbackState.Duration;
        TimeSpan clampedTime;
        PlaybackTransportStatus status;

        if (playbackState.IsLooping && duration > TimeSpan.Zero)
        {
            clampedTime = WrapTime(requestedTime, duration);
            status = keepPlaying
                ? PlaybackTransportStatus.Playing
                : clampedTime == TimeSpan.Zero
                    ? PlaybackTransportStatus.Stopped
                    : PlaybackTransportStatus.Paused;
        }
        else
        {
            clampedTime = requestedTime < TimeSpan.Zero
                ? TimeSpan.Zero
                : requestedTime > duration
                    ? duration
                    : requestedTime;

            if (keepPlaying && clampedTime < duration)
            {
                status = PlaybackTransportStatus.Playing;
            }
            else if (clampedTime == TimeSpan.Zero && !keepPlaying)
            {
                status = PlaybackTransportStatus.Stopped;
            }
            else if (clampedTime >= duration && keepPlaying)
            {
                status = PlaybackTransportStatus.Stopped;
            }
            else
            {
                status = PlaybackTransportStatus.Paused;
            }
        }

        return playbackState with
        {
            Status = status,
            CurrentTime = clampedTime,
        };
    }

    private static TimeSpan Scale(TimeSpan value, double multiplier)
    {
        return TimeSpan.FromTicks((long)Math.Round(value.Ticks * multiplier));
    }

    private static TimeSpan WrapTime(TimeSpan requestedTime, TimeSpan duration)
    {
        if (requestedTime <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        long wrappedTicks = requestedTime.Ticks % duration.Ticks;
        return wrappedTicks == 0 && requestedTime > TimeSpan.Zero
            ? duration
            : TimeSpan.FromTicks(wrappedTicks);
    }
}
