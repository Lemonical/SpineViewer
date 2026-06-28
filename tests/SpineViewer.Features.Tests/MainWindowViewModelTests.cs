using Avalonia.Media;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Diagnostics.ViewModels;
using SpineViewer.Features.Inspector.ViewModels;
using SpineViewer.Features.Playback.Services;
using SpineViewer.Features.Playback.ViewModels;
using SpineViewer.Features.Settings.ViewModels;
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
        TestWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                FeatureTestFactory.CreateSession("Hero"),
                [new SpineProjectReference("Mage", "mage.json", "mage.atlas")],
                Array.Empty<ViewerDiagnostic>(),
                "Opened Hero.",
                false));
        ViewportViewModel viewportViewModel = CreateViewportViewModel(workspaceSessionService);
        PlaybackTransportViewModel playbackViewModel = CreatePlaybackViewModel(workspaceSessionService);
        TestViewerSettingsService viewerSettingsService = new(new ViewerSettings());
        MainWindowViewModel viewModel = new(
            workspaceSessionService,
            viewportViewModel,
            playbackViewModel,
            new AnimationTrackEditorViewModel(
                workspaceSessionService,
                new PlaybackTrackEditorService(),
                viewerSettingsService),
            new AssetInspectorViewModel(workspaceSessionService),
            new DiagnosticsPanelViewModel(workspaceSessionService),
            new ViewerSettingsViewModel(
                viewerSettingsService,
                new TestApplicationThemeService()));

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
        TestWorkspaceSessionService workspaceSessionService = new(
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
        TestWorkspaceSessionService workspaceSessionService = new(
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
        TestWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                FeatureTestFactory.CreateSession("Hero"),
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
        TestWorkspaceSessionService workspaceSessionService = new(
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
        TestWorkspaceSessionService workspaceSessionService = new(
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
        TestWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                null,
                Array.Empty<SpineProjectReference>(),
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false));
        MainWindowViewModel viewModel = CreateShellViewModel(workspaceSessionService);

        workspaceSessionService.UpdateState(
            new WorkspaceState(
                FeatureTestFactory.CreateSession("Hero"),
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
        TestWorkspaceSessionService workspaceSessionService = new(
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
        TestViewerSettingsService viewerSettingsService = new(new ViewerSettings());

        return new MainWindowViewModel(
            workspaceSessionService,
            CreateViewportViewModel(workspaceSessionService),
            CreatePlaybackViewModel(workspaceSessionService),
            new AnimationTrackEditorViewModel(
                workspaceSessionService,
                new PlaybackTrackEditorService(),
                viewerSettingsService),
            new AssetInspectorViewModel(workspaceSessionService),
            new DiagnosticsPanelViewModel(workspaceSessionService),
            new ViewerSettingsViewModel(
                viewerSettingsService,
                new TestApplicationThemeService()));
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
