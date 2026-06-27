using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Services;
using SpineViewer.Features.Playback.ViewModels;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class PlaybackTransportViewModelTests
{
    [Fact]
    public void PlayCommand_UpdatesWorkspaceState()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState());
        StubRenderInvalidationService renderInvalidationService = new();
        PlaybackTransportViewModel viewModel = CreateViewModel(workspaceSessionService, renderInvalidationService);

        viewModel.PlayCommand.Execute(null);

        Assert.True(Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Playback.IsPlaying);
        Assert.Equal("Playing", viewModel.StatusText);
    }

    [Fact]
    public void FrameTick_WhenPlaying_AdvancesPlaybackTime()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState());
        StubRenderInvalidationService renderInvalidationService = new();
        PlaybackTransportViewModel viewModel = CreateViewModel(workspaceSessionService, renderInvalidationService);

        viewModel.PlayCommand.Execute(null);
        renderInvalidationService.Raise(RenderInvalidationReason.FrameTick);

        Assert.True(
            Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Playback.CurrentTime >
            TimeSpan.Zero);
    }

    [Fact]
    public void ToggleLoopCommand_UpdatesWorkspaceState()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState());
        StubRenderInvalidationService renderInvalidationService = new();
        PlaybackTransportViewModel viewModel = CreateViewModel(workspaceSessionService, renderInvalidationService);

        viewModel.ToggleLoopCommand.Execute(null);

        Assert.False(Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Playback.IsLooping);
    }

    private static PlaybackTransportViewModel CreateViewModel(
        StubWorkspaceSessionService workspaceSessionService,
        StubRenderInvalidationService renderInvalidationService)
    {
        return new PlaybackTransportViewModel(
            workspaceSessionService,
            new PlaybackStateService(),
            renderInvalidationService,
            new StubViewportFrameScheduler());
    }

    private static WorkspaceState CreateWorkspaceState()
    {
        return new WorkspaceState(
            new SpineProjectSession(
                Guid.NewGuid(),
                new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
                new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
                new SpineRuntimeDescriptor(
                    "spine-4.1.00",
                    "Spine 4.1.00",
                    "4.1.x",
                    new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)])),
                new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
                new PlaybackState(),
                new ViewportState(),
                Array.Empty<ViewerDiagnostic>()),
            Array.Empty<SpineProjectReference>(),
            Array.Empty<ViewerDiagnostic>(),
            "Opened Hero.",
            false);
    }

    private sealed class StubRenderInvalidationService : IRenderInvalidationService
    {
        public event EventHandler<RenderInvalidatedEventArgs>? RenderInvalidated;

        public void Raise(RenderInvalidationReason reason)
        {
            RenderInvalidated?.Invoke(this, new RenderInvalidatedEventArgs(reason));
        }

        public void RequestInvalidation(RenderInvalidationReason reason)
        {
            Raise(reason);
        }
    }

    private sealed class StubViewportFrameScheduler : IViewportFrameScheduler
    {
        public TimeSpan FrameInterval => TimeSpan.FromSeconds(1.0 / 60.0);

        public bool IsRunning => false;

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }

    private sealed class StubWorkspaceSessionService : IWorkspaceSessionService
    {
        public StubWorkspaceSessionService(WorkspaceState state)
        {
            State = state;
        }

        public event EventHandler? StateChanged;

        public WorkspaceState State { get; private set; }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(IReadOnlyList<string> selectedPaths, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(string selectedPath, string? companionPath, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task ReloadAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RestoreLastSessionAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void UpdatePlaybackState(PlaybackState playbackState)
        {
            if (State.CurrentSession is null)
            {
                return;
            }

            State = State with
            {
                CurrentSession = State.CurrentSession with
                {
                    Playback = playbackState,
                },
            };

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateViewportState(ViewportState viewportState)
        {
        }

        public void UpdateSelectedSkin(string? skinName)
        {
        }
    }
}
