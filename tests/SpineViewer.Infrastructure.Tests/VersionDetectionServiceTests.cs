using SpineViewer.Core.Models;
using SpineViewer.Infrastructure.Spine.Adapters;
using SpineViewer.Infrastructure.Spine.Loading;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class VersionDetectionServiceTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly VersionDetectionService _service = new(
        new SpineRuntimeCatalog(
            [
                new Spine38RuntimeAdapter(),
                new Spine41RuntimeAdapter(),
            ]));

    public VersionDetectionServiceTests()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpineViewer.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task DetectAsync_ReturnsExactMatchForKnownJsonVersionAsync()
    {
        string skeletonPath = WriteFile("hero.json", """{"skeleton":{"spine":"4.1.00"}}""");
        string atlasPath = WriteFile("hero.atlas", "hero.png\n");
        string texturePath = WriteFile("hero.png", "png");
        SpineAssetFileSet assetFileSet = new(skeletonPath, atlasPath, [texturePath]);

        SpineVersionMatch result = await _service.DetectAsync(assetFileSet, CancellationToken.None);

        Assert.True(result.IsExactMatch);
        Assert.Equal("spine-4.1.00", result.SuggestedRuntimeId);
        Assert.Equal("4.1.00", result.DetectedExportVersion);
    }

    [Fact]
    public async Task DetectAsync_ReturnsCompatibilityWarningForBinarySkeletonAsync()
    {
        string skeletonPath = WriteFile("hero.skel", "binary");
        string atlasPath = WriteFile("hero.atlas", "hero.png\n");
        string texturePath = WriteFile("hero.png", "png");
        SpineAssetFileSet assetFileSet = new(skeletonPath, atlasPath, [texturePath]);

        SpineVersionMatch result = await _service.DetectAsync(assetFileSet, CancellationToken.None);

        Assert.False(result.IsExactMatch);
        Assert.Null(result.SuggestedRuntimeId);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "version-detection-binary-heuristic");
        Assert.Equal(["spine-3.8.95", "spine-4.1.00"], result.CompatibleRuntimeIds);
    }

    [Fact]
    public async Task DetectAsync_UsesEmbeddedBinaryVersionWhenAvailableAsync()
    {
        string skeletonPath = WriteFile("hero.skel", "header\03.8.76\0payload");
        string atlasPath = WriteFile("hero.atlas", "hero.png\n");
        string texturePath = WriteFile("hero.png", "png");
        SpineAssetFileSet assetFileSet = new(skeletonPath, atlasPath, [texturePath]);

        SpineVersionMatch result = await _service.DetectAsync(assetFileSet, CancellationToken.None);

        Assert.False(result.IsExactMatch);
        Assert.Equal("3.8.76", result.DetectedExportVersion);
        Assert.Equal("spine-3.8.95", result.SuggestedRuntimeId);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "version-detection-compatible-runtime-family");
    }

    [Fact]
    public async Task DetectAsync_ReturnsErrorForInvalidJsonSkeletonAsync()
    {
        string skeletonPath = WriteFile("hero.json", "{ this is not valid json");
        string atlasPath = WriteFile("hero.atlas", "hero.png\n");
        string texturePath = WriteFile("hero.png", "png");
        SpineAssetFileSet assetFileSet = new(skeletonPath, atlasPath, [texturePath]);

        SpineVersionMatch result = await _service.DetectAsync(assetFileSet, CancellationToken.None);

        Assert.False(result.IsExactMatch);
        Assert.Null(result.SuggestedRuntimeId);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "version-detection-invalid-json-skeleton");
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
