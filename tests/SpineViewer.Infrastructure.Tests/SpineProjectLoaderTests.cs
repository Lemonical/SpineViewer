using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Infrastructure.Spine.Loading;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class SpineProjectLoaderTests
{
    [Fact]
    public async Task LoadAsync_UsesAdapterAndReportsCompletionProgressAsync()
    {
        RecordingRuntimeAdapter adapter = new(true, Array.Empty<ViewerDiagnostic>());
        SpineProjectInspection inspection = new(
            new SpineExportMetadata("4.1.00", "./images", "./audio", 512, 256, 30, 0, 0, 0, 1, 0, 0),
            [new SpineAnimationInfo("idle", TimeSpan.FromSeconds(1.5), 2, 4)],
            Array.Empty<SpineSkinInfo>(),
            Array.Empty<SpineBoneInfo>(),
            Array.Empty<SpineSlotInfo>(),
            Array.Empty<SpineAttachmentInfo>(),
            Array.Empty<SpineAtlasPageInfo>(),
            Array.Empty<SpineAtlasRegionInfo>(),
            Array.Empty<ViewerDiagnostic>());
        SpineProjectLoader loader = new(
            new SpineRuntimeCatalog([adapter]),
            new TestProjectInspector(inspection),
            new TestAppLogger<SpineProjectLoader>());

        List<SpineLoadProgress> progressUpdates = [];
        LoadSpineProjectRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            adapter.Descriptor,
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            new Progress<SpineLoadProgress>(progressUpdates.Add));

        LoadSpineProjectResult result = await loader.LoadAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccessful);
        Assert.Equal("idle", Assert.Single(result.Inspection.Animations).Name);
        Assert.Equal(
            [SpineLoadStage.LoadingProject, SpineLoadStage.Completed],
            progressUpdates.Select(static update => update.Stage).ToArray());
    }

    [Fact]
    public async Task LoadAsync_ReturnsStructuredDiagnosticWhenAdapterThrowsAsync()
    {
        ThrowingRuntimeAdapter adapter = new();
        SpineProjectLoader loader = new(
            new SpineRuntimeCatalog([adapter]),
            new TestProjectInspector(SpineProjectInspection.Empty),
            new TestAppLogger<SpineProjectLoader>());

        LoadSpineProjectRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            adapter.Descriptor,
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()));

        LoadSpineProjectResult result = await loader.LoadAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "project-load-unhandled-exception");
    }

    private sealed class RecordingRuntimeAdapter : ISpineRuntimeAdapter
    {
        public RecordingRuntimeAdapter(bool isSuccessful, IReadOnlyList<ViewerDiagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
            _isSuccessful = isSuccessful;
            Descriptor = new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)]));
        }

        private readonly IReadOnlyList<ViewerDiagnostic> _diagnostics;
        private readonly bool _isSuccessful;

        public SpineRuntimeDescriptor Descriptor { get; }

        public Task<SpineRuntimeProbeResult> ProbeAsync(
            SpineAssetFileSet assetFileSet,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new SpineRuntimeProbeResult(
                    Descriptor,
                    SpineRuntimeSupportStatus.Supported,
                    Array.Empty<UnsupportedSpineFeature>(),
                    Array.Empty<ViewerDiagnostic>()));
        }

        public Task<SpineLoadResult> LoadAsync(
            SpineLoadRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new SpineLoadResult(
                    _isSuccessful,
                    request.ProjectReference,
                    request.AssetFileSet,
                    Descriptor,
                    request.VersionMatch,
                    request.ProbeResult.UnsupportedFeatures,
                    _diagnostics));
        }
    }

    private sealed class ThrowingRuntimeAdapter : ISpineRuntimeAdapter
    {
        public ThrowingRuntimeAdapter()
        {
            Descriptor = new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)]));
        }

        public SpineRuntimeDescriptor Descriptor { get; }

        public Task<SpineRuntimeProbeResult> ProbeAsync(
            SpineAssetFileSet assetFileSet,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new SpineRuntimeProbeResult(
                    Descriptor,
                    SpineRuntimeSupportStatus.Supported,
                    Array.Empty<UnsupportedSpineFeature>(),
                    Array.Empty<ViewerDiagnostic>()));
        }

        public Task<SpineLoadResult> LoadAsync(
            SpineLoadRequest request,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("boom");
        }
    }

    private sealed class TestProjectInspector : ISpineProjectInspector
    {
        private readonly SpineProjectInspection _inspection;

        public TestProjectInspector(SpineProjectInspection inspection)
        {
            _inspection = inspection;
        }

        public Task<SpineProjectInspection> InspectAsync(
            SpineAssetFileSet assetFileSet,
            SpineRuntimeDescriptor selectedRuntime,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_inspection);
        }
    }

    private sealed class TestAppLogger<TCategory> : IAppLogger<TCategory>
    {
        public void LogError(string message, Exception exception)
        {
        }

        public void LogInformation(string message)
        {
        }

        public void LogWarning(string message)
        {
        }
    }
}
