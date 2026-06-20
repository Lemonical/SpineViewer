using Avalonia.Media;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;
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
        ViewportViewModel viewModel = new(
            workspaceSessionService,
            renderInvalidationService,
            frameScheduler,
            new StubViewportSceneComposer());

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
    public void WorkspaceStateChange_WhenViewportChanges_RequestsViewportInvalidation()
    {
        StubWorkspaceSessionService workspaceSessionService = new(CreateWorkspaceState(isPlaying: false));
        SpineProjectSession currentSession = Assert.IsType<SpineProjectSession>(workspaceSessionService.State.CurrentSession);
        StubRenderInvalidationService renderInvalidationService = new();
        ViewportViewModel viewModel = new(
            workspaceSessionService,
            renderInvalidationService,
            new StubViewportFrameScheduler(),
            new StubViewportSceneComposer());

        workspaceSessionService.SetState(
            new WorkspaceState(
                currentSession with
                {
                    Viewport = new ViewportState(2.0, 40.0, 15.0, true, true, false, false),
                },
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Updated viewport.",
                false));

        Assert.Contains(RenderInvalidationReason.ViewportChanged, renderInvalidationService.RequestedReasons);
        Assert.Equal(1, viewModel.RenderScene.FrameVersion);
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

    private sealed class StubViewportSceneComposer : IViewportSceneComposer
    {
        public ViewportRenderScene Compose(
            WorkspaceState workspaceState,
            ViewportHostLayout hostLayout,
            long frameVersion)
        {
            return new ViewportRenderScene(
                Color.FromRgb(0x10, 0x15, 0x1F),
                new ViewportRenderTransform(1.0, hostLayout.Width / 2.0, hostLayout.Height / 2.0),
                Array.Empty<ViewportOverlayLine>(),
                frameVersion,
                workspaceState.CurrentSession is not null,
                workspaceState.CurrentSession?.Project.DisplayName);
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

        public void SetState(WorkspaceState state)
        {
            State = state;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePlaybackState(PlaybackState playbackState)
        {
        }

        public void UpdateViewportState(ViewportState viewportState)
        {
        }
    }
}
