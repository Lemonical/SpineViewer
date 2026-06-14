using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Infrastructure.Spine.Adapters;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class RuntimeAdapterLoadTests : IDisposable
{
    private readonly string _temporaryDirectory;

    public RuntimeAdapterLoadTests()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpineViewer.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task Spine41RuntimeAdapter_LoadAsync_ReturnsSuccessForValidJsonSkeletonAsync()
    {
        string skeletonPath = WriteFile("hero.json", """{"skeleton":{"spine":"4.1.00"}}""");
        string atlasPath = WriteFile("hero.atlas", "hero.png\n");
        string texturePath = WriteFile("hero.png", "png");
        Spine41RuntimeAdapter adapter = new();
        SpineAssetFileSet assetFileSet = new(skeletonPath, atlasPath, [texturePath]);
        SpineRuntimeProbeResult probeResult = await adapter.ProbeAsync(assetFileSet, CancellationToken.None);

        SpineLoadResult result = await adapter.LoadAsync(
            new SpineLoadRequest(
                new SpineProjectReference("Hero", skeletonPath, atlasPath),
                assetFileSet,
                new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
                probeResult),
            CancellationToken.None);

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public async Task Spine38RuntimeAdapter_LoadAsync_ReturnsErrorForInvalidJsonSkeletonAsync()
    {
        string skeletonPath = WriteFile("hero.json", "{ bad json");
        string atlasPath = WriteFile("hero.atlas", "hero.png\n");
        string texturePath = WriteFile("hero.png", "png");
        Spine38RuntimeAdapter adapter = new();
        SpineAssetFileSet assetFileSet = new(skeletonPath, atlasPath, [texturePath]);
        SpineRuntimeProbeResult probeResult = await adapter.ProbeAsync(assetFileSet, CancellationToken.None);

        SpineLoadResult result = await adapter.LoadAsync(
            new SpineLoadRequest(
                new SpineProjectReference("Hero", skeletonPath, atlasPath),
                assetFileSet,
                new SpineVersionMatch("3.8.95", "spine-3.8.95", true, Array.Empty<ViewerDiagnostic>()),
                probeResult),
            CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "runtime-load-invalid-json-skeleton");
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, true);
        }
    }

    private string WriteFile(string relativePath, string content)
    {
        string fullPath = Path.Combine(_temporaryDirectory, relativePath);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }
}
