using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class PlaybackStateServiceTests
{
    [Fact]
    public void PlayPauseStop_UsesExplicitVisibleStatuses()
    {
        PlaybackStateService service = new();
        PlaybackState state = new();

        PlaybackState playingState = service.Play(state);
        PlaybackState pausedState = service.Pause(playingState);
        PlaybackState stoppedState = service.Stop(pausedState);

        Assert.Equal(PlaybackTransportStatus.Playing, playingState.Status);
        Assert.Equal(PlaybackTransportStatus.Paused, pausedState.Status);
        Assert.Equal(PlaybackTransportStatus.Stopped, stoppedState.Status);
        Assert.Equal(TimeSpan.Zero, stoppedState.CurrentTime);
    }

    [Fact]
    public void Advance_WithoutLooping_StopsAtTimelineEnd()
    {
        PlaybackStateService service = new();
        PlaybackState state = new(
            PlaybackTransportStatus.Playing,
            false,
            1.0,
            TimeSpan.FromSeconds(4.9),
            TimeSpan.FromSeconds(5),
            Array.Empty<AnimationTrackState>());

        PlaybackState advancedState = service.Advance(state, TimeSpan.FromSeconds(1));

        Assert.Equal(PlaybackTransportStatus.Stopped, advancedState.Status);
        Assert.Equal(TimeSpan.FromSeconds(5), advancedState.CurrentTime);
    }

    [Fact]
    public void StepForward_FromStopped_MovesToPausedPreviewTime()
    {
        PlaybackStateService service = new();
        PlaybackState state = new();

        PlaybackState steppedState = service.StepForward(state);

        Assert.Equal(PlaybackTransportStatus.Paused, steppedState.Status);
        Assert.True(steppedState.CurrentTime > TimeSpan.Zero);
    }
}
