using SpineViewer.Infrastructure.Spine.Loading;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class SpineProjectReferenceResolverTests : IDisposable
{
    private readonly string _temporaryDirectory;
    private readonly SpineProjectReferenceResolver _resolver = new();

    public SpineProjectReferenceResolverTests()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpineViewer.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_temporaryDirectory);
    }

    [Fact]
    public async Task ResolveFromSelectionAsync_AutoPairsAtlasAndTextureForSkeletonSelectionAsync()
    {
        string skeletonPath = WriteFile("hero.json", """{"skeleton":{"spine":"4.1.00"}}""");
        string atlasPath = WriteFile("hero.atlas", "hero.png\nsize: 8,8\nformat: RGBA8888\nfilter: Linear,Linear\nrepeat: none\n");
        string texturePath = WriteFile("hero.png", "not-a-real-png");

        var result = await _resolver.ResolveFromSelectionAsync(
            skeletonPath,
            null,
            CancellationToken.None);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.ProjectReference);
        Assert.NotNull(result.AssetFileSet);
        Assert.Equal(atlasPath, result.ProjectReference!.AtlasPath);
        Assert.Equal(skeletonPath, result.ProjectReference.SkeletonPath);
        Assert.Equal([texturePath], result.AssetFileSet!.DependentTexturePaths);
    }

    [Fact]
    public async Task ResolveFromSelectionAsync_ReturnsErrorWhenTextureIsMissingAsync()
    {
        string skeletonPath = WriteFile("hero.json", """{"skeleton":{"spine":"4.1.00"}}""");
        WriteFile("hero.atlas", "hero.png\nsize: 8,8\nformat: RGBA8888\nfilter: Linear,Linear\nrepeat: none\n");

        var result = await _resolver.ResolveFromSelectionAsync(
            skeletonPath,
            null,
            CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "atlas-texture-missing");
    }

    [Fact]
    public async Task ResolveForReopenAsync_ResolvesRelativeTextureDependenciesAsync()
    {
        string dataDirectory = Path.Combine(_temporaryDirectory, "data");
        string texturesDirectory = Path.Combine(dataDirectory, "textures");
        Directory.CreateDirectory(texturesDirectory);

        string skeletonPath = WriteFile(Path.Combine("data", "hero.json"), """{"skeleton":{"spine":"4.1.00"}}""");
        string atlasPath = WriteFile(
            Path.Combine("data", "hero.atlas"),
            "textures/hero.png\nsize: 8,8\nformat: RGBA8888\nfilter: Linear,Linear\nrepeat: none\n");
        string texturePath = WriteFile(Path.Combine("data", "textures", "hero.png"), "not-a-real-png");

        var result = await _resolver.ResolveForReopenAsync(
            new SpineViewer.Core.Models.SpineProjectReference("Hero", skeletonPath, atlasPath),
            CancellationToken.None);

        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.AssetFileSet);
        Assert.Equal([texturePath], result.AssetFileSet!.DependentTexturePaths);
    }

    [Fact]
    public async Task ResolveFromSelectionAsync_ReturnsErrorForInvalidAtlasEntriesAsync()
    {
        string skeletonPath = WriteFile("hero.json", """{"skeleton":{"spine":"4.1.00"}}""");
        WriteFile("hero.atlas", "size: 8,8\n");

        var result = await _resolver.ResolveFromSelectionAsync(
            skeletonPath,
            null,
            CancellationToken.None);

        Assert.False(result.IsSuccessful);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "atlas-page-missing-before-entry");
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
        string? directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content);
        return fullPath;
    }
}
