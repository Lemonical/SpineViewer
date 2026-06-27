using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Services;
using SpineViewer.Features.Playback.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class AnimationTrackEditorViewModelTests
{
    [Fact]
    public void AddTrackCommand_UsesViewerDefaultsForNewTrack()
    {
        SpineProjectInspection inspection = FeatureTestFactory.CreateInspection(
            animations:
            [
                new SpineAnimationInfo("idle", TimeSpan.FromSeconds(1), 2, 4),
                new SpineAnimationInfo("run", TimeSpan.FromSeconds(2), 2, 4),
            ]);
        TestWorkspaceSessionService workspaceSessionService = new(
            FeatureTestFactory.CreateWorkspaceState(
                FeatureTestFactory.CreateSession(
                    inspection: inspection,
                    playbackState: new PlaybackState())));
        TestViewerSettingsService viewerSettingsService = new(
            new ViewerSettings
            {
                LoopPlaybackByDefault = false,
                DefaultTrackTimeScale = 1.75,
                DefaultTrackMixDuration = TimeSpan.FromSeconds(0.35),
            });
        AnimationTrackEditorViewModel viewModel = new(
            workspaceSessionService,
            new PlaybackTrackEditorService(),
            viewerSettingsService);

        viewModel.SelectedNewTrackAnimationName = "run";
        viewModel.AddTrackCommand.Execute(null);

        AnimationTrackState track = Assert.Single(
            Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Playback.Tracks);
        Assert.Equal("run", track.AnimationName);
        Assert.False(track.IsLooping);
        Assert.Equal(1.75, track.TimeScale);
        Assert.Equal(TimeSpan.FromSeconds(0.35), track.MixDuration);
    }

    [Fact]
    public void EditingSelectedTrack_UpdatesWorkspacePlaybackState()
    {
        AnimationTrackState originalTrack = new(Guid.NewGuid(), 0, "idle", true, 1.0, TimeSpan.Zero, true);
        SpineProjectInspection inspection = FeatureTestFactory.CreateInspection(
            animations:
            [
                new SpineAnimationInfo("idle", TimeSpan.FromSeconds(1), 2, 4),
                new SpineAnimationInfo("run", TimeSpan.FromSeconds(2), 2, 4),
            ]);
        TestWorkspaceSessionService workspaceSessionService = new(
            FeatureTestFactory.CreateWorkspaceState(
                FeatureTestFactory.CreateSession(
                    inspection: inspection,
                    playbackState: new PlaybackState(
                        PlaybackTransportStatus.Paused,
                        true,
                        1.0,
                        TimeSpan.FromSeconds(0.25),
                        TimeSpan.FromSeconds(2),
                        [originalTrack]))));
        AnimationTrackEditorViewModel viewModel = new(
            workspaceSessionService,
            new PlaybackTrackEditorService(),
            new TestViewerSettingsService(new ViewerSettings()));

        viewModel.SelectedTrackAnimationName = "run";
        viewModel.SelectedTrackIsLooping = false;
        viewModel.SelectedTrackTimeScale = 1.5;
        viewModel.SelectedTrackMixDurationSeconds = 0.4;
        viewModel.SelectedTrackIsEnabled = false;

        AnimationTrackState updatedTrack = Assert.Single(
            Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Playback.Tracks);
        Assert.Equal("run", updatedTrack.AnimationName);
        Assert.False(updatedTrack.IsLooping);
        Assert.Equal(1.5, updatedTrack.TimeScale);
        Assert.Equal(TimeSpan.FromSeconds(0.4), updatedTrack.MixDuration);
        Assert.False(updatedTrack.IsEnabled);
    }
}
