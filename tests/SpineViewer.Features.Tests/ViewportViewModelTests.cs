using Avalonia.Media;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Services;
using SpineViewer.Features.Viewport.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class ViewportViewModelTests
{
    [Fact]
    public void Activate_WithPlayingSessionAndHostSurface_StartsSchedulerAndAdvancesFrames()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState(isPlaying: true));
        StubRenderInvalidationService renderInvalidationService = new();
        StubViewportFrameScheduler frameScheduler = new();
        ViewportViewModel viewModel = CreateViewModel(
            workspaceSessionService,
            renderInvalidationService,
            frameScheduler);

        viewModel.UpdateHostLayout(800.0, 600.0);
        viewModel.Activate();
        long initialFrameVersion = viewModel.RenderScene.FrameVersion;

        renderInvalidationService.Raise(RenderInvalidationReason.FrameTick);

        Assert.True(frameScheduler.IsRunning);
        Assert.Equal("Hero", viewModel.RenderScene.SessionName);
        Assert.Equal(initialFrameVersion + 1, viewModel.RenderScene.FrameVersion);

        viewModel.Deactivate();

        Assert.False(frameScheduler.IsRunning);
    }

    [Fact]
    public void ToggleGridCommand_UpdatesWorkspaceState()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState(isPlaying: false));
        ViewportViewModel viewModel = CreateViewModel(
            workspaceSessionService,
            new StubRenderInvalidationService(),
            new StubViewportFrameScheduler());

        viewModel.ToggleGridCommand.Execute(null);

        Assert.False(Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Viewport.ShowGrid);
    }

    [Fact]
    public void UpdateHostLayout_WithDefaultViewport_AutoFitsActiveSession()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState(isPlaying: false));
        ViewportViewModel viewModel = CreateViewModel(
            workspaceSessionService,
            new StubRenderInvalidationService(),
            new StubViewportFrameScheduler());

        viewModel.UpdateHostLayout(800.0, 600.0);

        ViewportState viewportState = Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession).Viewport;
        Assert.True(viewportState.Zoom > 1.0);
        Assert.Equal(0.0, viewportState.OffsetX);
        Assert.Equal(0.0, viewportState.OffsetY);
    }

    private static ViewportViewModel CreateViewModel(
        StubWorkspaceSessionService workspaceSessionService,
        StubRenderInvalidationService renderInvalidationService,
        StubViewportFrameScheduler frameScheduler)
    {
        return new ViewportViewModel(
            workspaceSessionService,
            renderInvalidationService,
            frameScheduler,
            new ViewportCameraService(),
            CreateSceneComposer());
    }

    private static IViewportSceneComposer CreateSceneComposer()
    {
        ViewportInspectionOverlayFactory inspectionOverlayFactory = new();

        return new ViewportSceneComposer(
            new ViewportCameraService(),
            [
                new GridOverlaySource(),
                new SessionPlaceholderOverlaySource(inspectionOverlayFactory),
                new BoneOverlaySource(inspectionOverlayFactory),
                new BoundsOverlaySource(inspectionOverlayFactory),
                new OriginOverlaySource(),
            ]);
    }

    private static WorkspaceState CreateWorkspaceState(bool isPlaying)
    {
        return new WorkspaceState(
            CreateSession(
                "Hero",
                new PlaybackState(isPlaying, true, 1.0, TimeSpan.Zero, Array.Empty<AnimationTrackState>()),
                new ViewportState()),
            Array.Empty<SpineProjectReference>(),
            Array.Empty<ViewerDiagnostic>(),
            "Opened Hero.",
            false);
    }

    private static SpineProjectSession CreateSession(
        string displayName,
        PlaybackState playbackState,
        ViewportState viewportState)
    {
        return new SpineProjectSession(
            Guid.NewGuid(),
            new SpineProjectReference(
                displayName,
                $"{displayName.ToLowerInvariant()}.json",
                $"{displayName.ToLowerInvariant()}.atlas"),
            new SpineAssetFileSet(
                $"{displayName.ToLowerInvariant()}.json",
                $"{displayName.ToLowerInvariant()}.atlas",
                [$"{displayName.ToLowerInvariant()}.png"]),
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)])),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            playbackState,
            viewportState,
            Array.Empty<ViewerDiagnostic>());
    }

    private sealed class StubRenderInvalidationService : IRenderInvalidationService
    {
        public event EventHandler<RenderInvalidatedEventArgs>? RenderInvalidated;

        public List<RenderInvalidationReason> RequestedReasons { get; } = [];

        public void Raise(RenderInvalidationReason reason)
        {
            RenderInvalidated?.Invoke(this, new RenderInvalidatedEventArgs(reason));
        }

        public void RequestInvalidation(RenderInvalidationReason reason)
        {
            RequestedReasons.Add(reason);
            Raise(reason);
        }
    }

    private sealed class StubViewportFrameScheduler : IViewportFrameScheduler
    {
        public TimeSpan FrameInterval => TimeSpan.FromSeconds(1.0 / 60.0);

        public bool IsRunning { get; private set; }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
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

            SetState(
                State with
                {
                    CurrentSession = State.CurrentSession with
                    {
                        Playback = playbackState,
                    },
                });
        }

        public void UpdateViewportState(ViewportState viewportState)
        {
            if (State.CurrentSession is null)
            {
                return;
            }

            SetState(
                State with
                {
                    CurrentSession = State.CurrentSession with
                    {
                        Viewport = viewportState,
                    },
                });
        }

        public void UpdateSelectedSkin(string? skinName)
        {
            if (State.CurrentSession is null)
            {
                return;
            }

            SetState(
                State with
                {
                    CurrentSession = State.CurrentSession with
                    {
                        SelectedSkinName = string.IsNullOrWhiteSpace(skinName)
                            ? null
                            : skinName,
                    },
                });
        }

        private void SetState(WorkspaceState state)
        {
            State = state;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
