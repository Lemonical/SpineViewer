using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Features.Workspace.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class WorkspaceSessionServiceTests
{
    [Fact]
    public async Task OpenAsync_AfterAnotherProject_ResetsTransientSessionStateAsync()
    {
        SpineProjectReference heroProject = CreateProjectReference("Hero");
        SpineProjectReference mageProject = CreateProjectReference("Mage");
        TestProjectReferenceResolver resolver = new();
        resolver.SetSelectionResult("hero.json", CreateSuccessfulResolveResult(heroProject));
        resolver.SetSelectionResult("mage.json", CreateSuccessfulResolveResult(mageProject));

        TestSettingsRepository settingsRepository = new(
            new ViewerSettings(
                ViewerTheme.FollowSystem,
                false,
                false,
                true,
                true,
                false,
                1.5,
                10,
                true,
                null));
        WorkspaceSessionService service = CreateService(resolver, settingsRepository, new TestRecentFilesService());

        await service.OpenAsync("hero.json", null, CancellationToken.None);

        service.UpdatePlaybackState(
            new PlaybackState(
                true,
                true,
                3.0,
                TimeSpan.FromSeconds(5),
                [new AnimationTrackState(Guid.NewGuid(), 0, "walk", true, 1.0, true)]));
        service.UpdateViewportState(new ViewportState(2.0, 100.0, -50.0, true, true, false, false));

        await service.OpenAsync("mage.json", null, CancellationToken.None);

        SpineProjectSession session = Assert.IsType<SpineProjectSession>(service.State.CurrentSession);
        Assert.Equal("Mage", session.Project.DisplayName);
        Assert.False(session.Playback.IsPlaying);
        Assert.False(session.Playback.IsLooping);
        Assert.Equal(1.5, session.Playback.Speed);
        Assert.Equal(TimeSpan.Zero, session.Playback.CurrentTime);
        Assert.Empty(session.Playback.Tracks);
        Assert.Equal(1.0, session.Viewport.Zoom);
        Assert.Equal(0.0, session.Viewport.OffsetX);
        Assert.Equal(0.0, session.Viewport.OffsetY);
        Assert.False(session.Viewport.ShowGrid);
        Assert.False(session.Viewport.ShowOrigin);
        Assert.True(session.Viewport.ShowBones);
        Assert.True(session.Viewport.ShowBounds);
    }

    [Fact]
    public async Task ReloadAsync_ReplacesSessionIdentityAndClearsMutatedTransientStateAsync()
    {
        SpineProjectReference heroProject = CreateProjectReference("Hero");
        TestProjectReferenceResolver resolver = new();
        resolver.SetSelectionResult("hero.json", CreateSuccessfulResolveResult(heroProject));
        resolver.SetReopenResult(heroProject, CreateSuccessfulResolveResult(heroProject));

        WorkspaceSessionService service = CreateService(
            resolver,
            new TestSettingsRepository(new ViewerSettings()),
            new TestRecentFilesService());

        await service.OpenAsync("hero.json", null, CancellationToken.None);
        Guid firstSessionId = Assert.IsType<SpineProjectSession>(service.State.CurrentSession).SessionId;

        service.UpdatePlaybackState(
            new PlaybackState(true, false, 2.0, TimeSpan.FromSeconds(2), Array.Empty<AnimationTrackState>()));
        service.UpdateViewportState(new ViewportState(1.25, 50.0, 25.0, false, false, true, true));

        await service.ReloadAsync(CancellationToken.None);

        SpineProjectSession reloadedSession = Assert.IsType<SpineProjectSession>(service.State.CurrentSession);
        Assert.NotEqual(firstSessionId, reloadedSession.SessionId);
        Assert.False(reloadedSession.Playback.IsPlaying);
        Assert.Equal(TimeSpan.Zero, reloadedSession.Playback.CurrentTime);
        Assert.Equal(1.0, reloadedSession.Viewport.Zoom);
        Assert.Equal(0.0, reloadedSession.Viewport.OffsetX);
    }

    [Fact]
    public async Task OpenAsync_WhenResolutionFails_ClearsStaleSessionAsync()
    {
        SpineProjectReference heroProject = CreateProjectReference("Hero");
        TestProjectReferenceResolver resolver = new();
        resolver.SetSelectionResult("hero.json", CreateSuccessfulResolveResult(heroProject));
        resolver.SetSelectionResult(
            "broken.json",
            new ResolveSpineProjectResult(
                false,
                null,
                null,
                [
                    new ViewerDiagnostic(
                        "resolve-failed",
                        ViewerDiagnosticSeverity.Error,
                        "The project could not be resolved.",
                        "Test"),
                ]));

        WorkspaceSessionService service = CreateService(
            resolver,
            new TestSettingsRepository(new ViewerSettings()),
            new TestRecentFilesService());

        await service.OpenAsync("hero.json", null, CancellationToken.None);
        Assert.NotNull(service.State.CurrentSession);

        await service.OpenAsync("broken.json", null, CancellationToken.None);

        Assert.Null(service.State.CurrentSession);
        Assert.Contains(service.State.Diagnostics, static diagnostic => diagnostic.Code == "resolve-failed");
        Assert.StartsWith("Failed to open", service.State.StatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenAsync_WithSelectedAtlasAndSkeleton_UsesExplicitCompanionAsync()
    {
        SpineProjectReference heroProject = CreateProjectReference("Hero");
        TestProjectReferenceResolver resolver = new();
        resolver.SetSelectionResult("hero.atlas", CreateSuccessfulResolveResult(heroProject));

        WorkspaceSessionService service = CreateService(
            resolver,
            new TestSettingsRepository(new ViewerSettings()),
            new TestRecentFilesService());

        await service.OpenAsync(["hero.atlas", "hero.json"], CancellationToken.None);

        Assert.Equal("hero.atlas", resolver.LastSelectedPath);
        Assert.Equal("hero.json", resolver.LastCompanionPath);
        Assert.Equal("Hero", Assert.IsType<SpineProjectSession>(service.State.CurrentSession).Project.DisplayName);
    }

    [Fact]
    public async Task OpenAsync_WithTooManySelectedFiles_PreservesCurrentSessionAndReportsGuidanceAsync()
    {
        SpineProjectReference heroProject = CreateProjectReference("Hero");
        TestProjectReferenceResolver resolver = new();
        resolver.SetSelectionResult("hero.json", CreateSuccessfulResolveResult(heroProject));

        WorkspaceSessionService service = CreateService(
            resolver,
            new TestSettingsRepository(new ViewerSettings()),
            new TestRecentFilesService());

        await service.OpenAsync("hero.json", null, CancellationToken.None);
        Guid initialSessionId = Assert.IsType<SpineProjectSession>(service.State.CurrentSession).SessionId;

        await service.OpenAsync(["hero.json", "hero.atlas", "hero.png"], CancellationToken.None);

        SpineProjectSession session = Assert.IsType<SpineProjectSession>(service.State.CurrentSession);
        Assert.Equal(initialSessionId, session.SessionId);
        Assert.Contains(
            service.State.Diagnostics,
            static diagnostic => diagnostic.Code == "project-selection-too-many-files");
        Assert.Equal("Select one skeleton file and one atlas file.", service.State.StatusText);
    }

    [Fact]
    public async Task RestoreLastSessionAsync_OpensPersistedProjectAndCloseClearsRestoreTargetAsync()
    {
        SpineProjectReference heroProject = CreateProjectReference("Hero");
        TestProjectReferenceResolver resolver = new();
        resolver.SetReopenResult(heroProject, CreateSuccessfulResolveResult(heroProject));
        TestSettingsRepository settingsRepository = new(
            new ViewerSettings(
                ViewerTheme.FollowSystem,
                true,
                true,
                false,
                false,
                true,
                1.0,
                10,
                true,
                heroProject));
        TestRecentFilesService recentFilesService = new([CreateProjectReference("Mage")]);
        WorkspaceSessionService service = CreateService(resolver, settingsRepository, recentFilesService);

        await service.InitializeAsync(CancellationToken.None);
        await service.RestoreLastSessionAsync(CancellationToken.None);

        SpineProjectSession session = Assert.IsType<SpineProjectSession>(service.State.CurrentSession);
        Assert.Equal("Hero", session.Project.DisplayName);
        Assert.Equal("Hero", service.State.RecentFiles[0].DisplayName);

        await service.CloseAsync(CancellationToken.None);

        Assert.Null(service.State.CurrentSession);
        Assert.Null(settingsRepository.CurrentSettings.LastProjectReference);
    }

    private static ResolveSpineProjectResult CreateSuccessfulResolveResult(SpineProjectReference projectReference)
    {
        return new ResolveSpineProjectResult(
            true,
            projectReference,
            new SpineAssetFileSet(
                projectReference.SkeletonPath,
                projectReference.AtlasPath,
                [$"{Path.GetFileNameWithoutExtension(projectReference.AtlasPath)}.png"]),
            Array.Empty<ViewerDiagnostic>());
    }

    private static SpineProjectReference CreateProjectReference(string displayName)
    {
        string lowerDisplayName = displayName.ToLowerInvariant();
        return new SpineProjectReference(displayName, $"{lowerDisplayName}.json", $"{lowerDisplayName}.atlas");
    }

    private static WorkspaceSessionService CreateService(
        TestProjectReferenceResolver resolver,
        TestSettingsRepository settingsRepository,
        TestRecentFilesService recentFilesService)
    {
        return new WorkspaceSessionService(
            resolver,
            new TestVersionDetectionService(),
            new TestRuntimeSelectionService(),
            new TestProjectLoader(),
            new TestSpineSessionFactory(),
            recentFilesService,
            settingsRepository);
    }

    private sealed class TestProjectLoader : ISpineProjectLoader
    {
        public Task<LoadSpineProjectResult> LoadAsync(
            LoadSpineProjectRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new LoadSpineProjectResult(
                    true,
                    request.ProjectReference,
                    request.AssetFileSet,
                    request.SelectedRuntime,
                    request.VersionMatch,
                    Array.Empty<ViewerDiagnostic>()));
        }
    }

    private sealed class TestProjectReferenceResolver : ISpineProjectReferenceResolver
    {
        private readonly Dictionary<string, ResolveSpineProjectResult> _reopenResults =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ResolveSpineProjectResult> _selectionResults =
            new(StringComparer.OrdinalIgnoreCase);

        public string? LastCompanionPath { get; private set; }

        public string? LastSelectedPath { get; private set; }

        public Task<ResolveSpineProjectResult> ResolveForReopenAsync(
            SpineProjectReference projectReference,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_reopenResults[projectReference.DisplayName]);
        }

        public Task<ResolveSpineProjectResult> ResolveFromSelectionAsync(
            string selectedPath,
            string? companionPath,
            CancellationToken cancellationToken)
        {
            LastSelectedPath = selectedPath;
            LastCompanionPath = companionPath;
            return Task.FromResult(_selectionResults[selectedPath]);
        }

        public void SetReopenResult(SpineProjectReference projectReference, ResolveSpineProjectResult result)
        {
            _reopenResults[projectReference.DisplayName] = result;
        }

        public void SetSelectionResult(string selectedPath, ResolveSpineProjectResult result)
        {
            _selectionResults[selectedPath] = result;
        }
    }

    private sealed class TestRecentFilesService : IRecentFilesService
    {
        private readonly List<SpineProjectReference> _entries;

        public TestRecentFilesService(IEnumerable<SpineProjectReference>? entries = null)
        {
            _entries = entries?.ToList() ?? [];
        }

        public Task AddAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
        {
            _entries.RemoveAll(
                existingReference =>
                    string.Equals(existingReference.SkeletonPath, projectReference.SkeletonPath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existingReference.AtlasPath, projectReference.AtlasPath, StringComparison.OrdinalIgnoreCase));
            _entries.Insert(0, projectReference);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SpineProjectReference>> GetRecentFilesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<SpineProjectReference>>(_entries.ToArray());
        }

        public Task RemoveMissingEntriesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestRuntimeSelectionService : IRuntimeSelectionService
    {
        public Task<RuntimeSelectionResult> SelectAsync(
            RuntimeSelectionRequest request,
            CancellationToken cancellationToken)
        {
            SpineRuntimeDescriptor runtimeDescriptor = new(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)]));

            return Task.FromResult(
                new RuntimeSelectionResult(
                    new SpineRuntimeProbeResult(
                        runtimeDescriptor,
                        SpineRuntimeSupportStatus.Supported,
                        Array.Empty<UnsupportedSpineFeature>(),
                        Array.Empty<ViewerDiagnostic>()),
                    true,
                    false,
                    Array.Empty<ViewerDiagnostic>()));
        }
    }

    private sealed class TestSettingsRepository : ISettingsRepository
    {
        public TestSettingsRepository(ViewerSettings currentSettings)
        {
            CurrentSettings = currentSettings;
        }

        public ViewerSettings CurrentSettings { get; private set; }

        public Task<ViewerSettings> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(CurrentSettings);
        }

        public Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken)
        {
            CurrentSettings = settings;
            return Task.CompletedTask;
        }
    }

    private sealed class TestSpineSessionFactory : ISpineSessionFactory
    {
        public SpineProjectSession Create(LoadSpineProjectResult loadResult, ViewerSettings viewerSettings)
        {
            return new SpineProjectSession(
                Guid.NewGuid(),
                loadResult.ProjectReference,
                loadResult.AssetFileSet,
                loadResult.SelectedRuntime,
                loadResult.VersionMatch,
                new PlaybackState(
                    false,
                    viewerSettings.LoopPlaybackByDefault,
                    viewerSettings.DefaultPlaybackSpeed,
                    TimeSpan.Zero,
                    Array.Empty<AnimationTrackState>()),
                new ViewportState(
                    1.0,
                    0.0,
                    0.0,
                    viewerSettings.ShowGridByDefault,
                    viewerSettings.ShowOriginByDefault,
                    viewerSettings.ShowBonesByDefault,
                    viewerSettings.ShowBoundsByDefault),
                loadResult.Diagnostics);
        }
    }

    private sealed class TestVersionDetectionService : IVersionDetectionService
    {
        public Task<SpineVersionMatch> DetectAsync(
            SpineAssetFileSet assetFileSet,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new SpineVersionMatch(
                    "4.1.00",
                    "spine-4.1.00",
                    true,
                    Array.Empty<ViewerDiagnostic>()));
        }
    }
}
