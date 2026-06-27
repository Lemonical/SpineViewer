using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class PlaybackTrackEditorServiceTests
{
    [Fact]
    public void AddTrack_RecalculatesDurationAndUsesRequestedDefaults()
    {
        PlaybackTrackEditorService service = new();
        SpineProjectInspection inspection = FeatureTestFactory.CreateInspection(
            animations:
            [
                new SpineAnimationInfo("idle", TimeSpan.FromSeconds(1.25), 2, 4),
                new SpineAnimationInfo("run", TimeSpan.FromSeconds(2.5), 3, 6),
            ]);
        PlaybackState playbackState = new(
            PlaybackTransportStatus.Stopped,
            true,
            1.0,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(1.25),
            [
                new AnimationTrackState(Guid.NewGuid(), 0, "idle", true, 1.0, TimeSpan.Zero, true),
            ]);

        PlaybackState updatedState = service.AddTrack(
            playbackState,
            inspection,
            "run",
            false,
            1.5,
            TimeSpan.FromSeconds(0.2));

        Assert.Equal(TimeSpan.FromSeconds(2.5), updatedState.Duration);
        AnimationTrackState addedTrack = Assert.Single(updatedState.Tracks, static track => track.TrackIndex == 1);
        Assert.Equal("run", addedTrack.AnimationName);
        Assert.False(addedTrack.IsLooping);
        Assert.Equal(1.5, addedTrack.TimeScale);
        Assert.Equal(TimeSpan.FromSeconds(0.2), addedTrack.MixDuration);
    }

    [Fact]
    public void Validate_WhenRuntimeLacksMultipleTrackSupport_ReturnsError()
    {
        PlaybackTrackEditorService service = new();
        SpineProjectInspection inspection = FeatureTestFactory.CreateInspection(
            animations:
            [
                new SpineAnimationInfo("idle", TimeSpan.FromSeconds(1), 2, 4),
                new SpineAnimationInfo("run", TimeSpan.FromSeconds(2), 2, 4),
            ]);
        PlaybackState playbackState = new(
            PlaybackTransportStatus.Playing,
            true,
            1.0,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            [
                new AnimationTrackState(Guid.NewGuid(), 0, "idle", true, 1.0, TimeSpan.Zero, true),
                new AnimationTrackState(Guid.NewGuid(), 1, "run", true, 1.0, TimeSpan.Zero, true),
            ]);

        IReadOnlyList<SpineViewer.Features.Playback.Models.PlaybackTrackValidationIssue> issues = service.Validate(
            playbackState,
            inspection,
            FeatureTestFactory.CreateRuntime(supportsMultipleTracks: false));

        Assert.Contains(
            issues,
            static issue => issue.Code == "track-stack-multiple-tracks-unsupported");
    }
}
