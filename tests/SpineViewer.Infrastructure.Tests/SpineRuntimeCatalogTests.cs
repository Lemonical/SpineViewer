using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Infrastructure.Spine.Loading;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class SpineRuntimeCatalogTests
{
    [Fact]
    public void TryGetBestAdapter_ReturnsSuggestedAdapterWhenAvailable()
    {
        SpineRuntimeCatalog catalog = new([new FakeRuntimeAdapter("spine-4.1.00")]);
        SpineVersionMatch versionMatch = new(
            "4.1.00",
            "spine-4.1.00",
            ["spine-4.1.00"],
            true,
            Array.Empty<ViewerDiagnostic>());

        bool found = catalog.TryGetBestAdapter(versionMatch, out ISpineRuntimeAdapter? adapter);

        Assert.True(found);
        Assert.NotNull(adapter);
        Assert.Equal("spine-4.1.00", adapter.Descriptor.RuntimeId);
    }

    [Fact]
    public void GetRequiredAdapter_ThrowsForUnknownRuntime()
    {
        SpineRuntimeCatalog catalog = new([new FakeRuntimeAdapter("spine-4.1.00")]);

        Assert.Throws<KeyNotFoundException>(() => catalog.GetRequiredAdapter("spine-3.8.95"));
    }

    private sealed class FakeRuntimeAdapter : ISpineRuntimeAdapter
    {
        public FakeRuntimeAdapter(string runtimeId)
        {
            Descriptor = new SpineRuntimeDescriptor(
                runtimeId,
                runtimeId,
                runtimeId,
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)]));
        }

        public SpineRuntimeDescriptor Descriptor { get; }

        public Task<SpineRuntimeProbeResult> ProbeAsync(
            SpineAssetFileSet assetFileSet,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<SpineLoadResult> LoadAsync(
            SpineLoadRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
