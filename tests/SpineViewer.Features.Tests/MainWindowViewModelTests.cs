using Avalonia.Media;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Services;
using SpineViewer.Features.Playback.ViewModels;
using SpineViewer.Features.Shell.ViewModels;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Services;
using SpineViewer.Features.Viewport.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_ReflectsWorkspaceState()
    {
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                CreateSession("Hero"),
                [new SpineProjectReference("Mage", "mage.json", "mage.atlas")],
                Array.Empty<ViewerDiagnostic>(),
                "Opened Hero.",
                false));
        ViewportViewModel viewportViewModel = CreateViewportViewModel(workspaceSessionService);
        PlaybackTransportViewModel playbackViewModel = CreatePlaybackViewModel(workspaceSessionService);
        MainWindowViewModel viewModel = new(workspaceSessionService, viewportViewModel, playbackViewModel);

        Assert.Equal("Hero - SpineViewer", viewModel.Title);
        Assert.Equal("Hero", viewModel.CurrentProjectName);
        Assert.Equal("Spine 4.1.00", viewModel.CurrentRuntimeSummary);
        Assert.Equal("Opened Hero.", viewModel.StatusText);
        Assert.True(viewModel.HasActiveSession);
        Assert.True(viewModel.HasRecentFiles);
        Assert.Equal("1 recent project(s)", viewModel.RecentFilesSummaryText);
        Assert.Contains("Stopped", viewModel.CurrentPlaybackSummary);
        Assert.Contains("Studio", viewModel.CurrentViewportSummary);
        Assert.False(viewModel.ShowLandingState);
        Assert.False(viewModel.IsCompactLayout);
        Assert.Same(viewportViewModel, viewModel.Viewport);
        Assert.Same(playbackViewModel, viewModel.Playback);
    }

    [Fact]
    public void Constructor_WithoutSession_ExposesFirstRunGuidance()
    {
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));

        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        Assert.True(viewModel.ShowLandingState);
        Assert.True(viewModel.ShowFirstRunState);
        Assert.False(viewModel.ShowLoadFailureState);
        Assert.Equal("Open a Spine model.", viewModel.WorkspaceExperienceTitle);
        Assert.Equal("Open a Spine Model", viewModel.CurrentProjectName);
        Assert.Equal("Exact runtime support is available for Spine 3.8.95 and 4.1.00.", viewModel.CurrentRuntimeSummary);
    }

    [Fact]
    public void UpdateLayoutWidth_UsesCompactLayoutBelowThreshold()
    {
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        viewModel.UpdateLayoutWidth(900.0);

        Assert.True(viewModel.IsCompactLayout);
        Assert.Equal("Compact layout", viewModel.LayoutModeLabel);

        viewModel.UpdateLayoutWidth(1400.0);

        Assert.False(viewModel.IsCompactLayout);
        Assert.Equal("Docked layout", viewModel.LayoutModeLabel);
    }

    [Fact]
    public async Task ReloadSessionCommand_RoutesToWorkspaceService()
    {
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                CreateSession("Hero"),
                [new SpineProjectReference("Hero", "hero.json", "hero.atlas")],
                Array.Empty<ViewerDiagnostic>(),
                "Opened Hero.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        await viewModel.ReloadSessionCommand.ExecuteAsync(null);

        Assert.Equal(1, workspaceSessionService.ReloadCount);
    }

    [Fact]
    public async Task OpenSelectedRecentProjectCommand_RoutesToWorkspaceService()
    {
        SpineProjectReference hero = new("Hero", "hero.json", "hero.atlas");
        SpineProjectReference mage = new("Mage", "mage.json", "mage.atlas");
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                [hero, mage],
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);
        viewModel.SelectedRecentProject = mage;

        await viewModel.OpenSelectedRecentProjectCommand.ExecuteAsync(null);

        Assert.Equal(mage, workspaceSessionService.LastOpenedProjectReference);
    }

    [Fact]
    public async Task OpenFilesAsync_RoutesToWorkspaceService()
    {
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        await viewModel.OpenFilesAsync(["hero.atlas", "hero.json"], CancellationToken.None);

        Assert.Equal(["hero.atlas", "hero.json"], workspaceSessionService.LastOpenedSelectedPaths);
    }

    [Fact]
    public void WorkspaceStateChange_RefreshesCurrentSelections()
    {
        SpineProjectReference hero = new("Hero", "hero.json", "hero.atlas");
        ViewerDiagnostic diagnostic = new(
            "hero-warning",
            ViewerDiagnosticSeverity.Warning,
            "Hero uses a compatibility fallback.",
            "Runtime Selection",
            suggestedAction: "Reload with a closer runtime when one is available.");
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        workspaceSessionService.UpdateState(
            new WorkspaceState(
                CreateSession("Hero"),
                [hero],
                [diagnostic],
                "Opened Hero.",
                false));

        Assert.Equal(hero, viewModel.SelectedRecentProject);
        Assert.Equal(diagnostic, viewModel.SelectedDiagnostic);
        Assert.Equal("Warning | hero-warning", viewModel.SelectedDiagnosticTitle);
    }

    [Fact]
    public void WorkspaceStateChange_WithDiagnostics_ExposesRecoveryGuidance()
    {
        ViewerDiagnostic diagnostic = new(
            "atlas-texture-missing",
            ViewerDiagnosticSeverity.Error,
            "A texture referenced by the atlas could not be found.",
            "Resolver",
            suggestedAction: "Restore the missing texture or re-export the atlas.");
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        workspaceSessionService.UpdateState(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                [diagnostic],
                "Failed to open Hero.",
                false));

        Assert.True(viewModel.ShowLandingState);
        Assert.True(viewModel.ShowLoadFailureState);
        Assert.False(viewModel.ShowFirstRunState);
        Assert.Equal("Couldn't open that model.", viewModel.WorkspaceExperienceTitle);
        Assert.Equal(diagnostic.SuggestedAction, viewModel.SelectedDiagnosticSuggestedAction);
    }

    private static MainWindowViewModel CreateShellViewModel(
        IWorkspaceSessionService workspaceSessionService)
    {
        return new MainWindowViewModel(
            workspaceSessionService,
            CreateViewportViewModel(workspaceSessionService),
            CreatePlaybackViewModel(workspaceSessionService));
    }

    private static PlaybackTransportViewModel CreatePlaybackViewModel(
        IWorkspaceSessionService workspaceSessionService)
    {
        return new PlaybackTransportViewModel(
            workspaceSessionService,
            new PlaybackStateService(),
            new StubRenderInvalidationService(),
            new StubViewportFrameScheduler());
    }

    private static SpineProjectSession CreateSession(string displayName)
    {
        return new SpineProjectSession(
            Guid.NewGuid(),
            new SpineProjectReference(displayName, $"{displayName.ToLowerInvariant()}.json", $"{displayName.ToLowerInvariant()}.atlas"),
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
            new PlaybackState(),
            new ViewportState(),
            Array.Empty<ViewerDiagnostic>());
    }

    private sealed class StubWorkspaceSessionService : IWorkspaceSessionService
    {
        public StubWorkspaceSessionService(WorkspaceState state)
        {
            State = state;
        }

        public event EventHandler? StateChanged;

        public WorkspaceState State { get; private set; }

        public SpineProjectReference? LastOpenedProjectReference { get; private set; }

        public IReadOnlyList<string> LastOpenedSelectedPaths { get; private set; } = Array.Empty<string>();

        public int ReloadCount { get; private set; }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task OpenAsync(IReadOnlyList<string> selectedPaths, CancellationToken cancellationToken)
        {
            LastOpenedSelectedPaths = selectedPaths.ToArray();
            return Task.CompletedTask;
        }

        public Task OpenAsync(string selectedPath, string? companionPath, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
        {
            LastOpenedProjectReference = projectReference;
            return Task.CompletedTask;
        }

        public Task ReloadAsync(CancellationToken cancellationToken)
        {
            ReloadCount++;
            return Task.CompletedTask;
        }

        public Task RestoreLastSessionAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void UpdatePlaybackState(PlaybackState playbackState)
        {
        }

        public void UpdateViewportState(ViewportState viewportState)
        {
        }

        public void UpdateState(WorkspaceState state)
        {
            State = state;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static ViewportViewModel CreateViewportViewModel(IWorkspaceSessionService workspaceSessionService)
    {
        return new ViewportViewModel(
            workspaceSessionService,
            new StubRenderInvalidationService(),
            new StubViewportFrameScheduler(),
            new ViewportCameraService(),
            new StubViewportSceneComposer());
    }

    private sealed class StubRenderInvalidationService : IRenderInvalidationService
    {
        public event EventHandler<RenderInvalidatedEventArgs>? RenderInvalidated;

        public void RequestInvalidation(RenderInvalidationReason reason)
        {
            RenderInvalidated?.Invoke(this, new RenderInvalidatedEventArgs(reason));
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

    private sealed class StubViewportSceneComposer : IViewportSceneComposer
    {
        public ViewportRenderScene Compose(
            WorkspaceState workspaceState,
            ViewportHostLayout hostLayout,
            long frameVersion)
        {
            return new ViewportRenderScene(
                Color.FromRgb(0x10, 0x15, 0x1F),
                new ViewportRenderTransform(1.0, 0.0, 0.0),
                Array.Empty<ViewportOverlayLine>(),
                frameVersion,
                workspaceState.CurrentSession is not null,
                workspaceState.CurrentSession?.Project.DisplayName);
        }
    }
}
