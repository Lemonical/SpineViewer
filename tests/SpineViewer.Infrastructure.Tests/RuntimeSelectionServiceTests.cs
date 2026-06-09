using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Infrastructure.Spine.Adapters;
using SpineViewer.Infrastructure.Spine.Loading;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class RuntimeSelectionServiceTests
{
    private readonly RuntimeSelectionService _service;

    public RuntimeSelectionServiceTests()
    {
        _service = new RuntimeSelectionService(
            new SpineRuntimeCatalog(
                [
                    new Spine38RuntimeAdapter(),
                    new Spine41RuntimeAdapter(),
                ]));
    }

    [Fact]
    public async Task SelectAsync_UsesExactSuggestedRuntimeWithoutConfirmationAsync()
    {
        RuntimeSelectionRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineVersionMatch(
                "4.1.00",
                "spine-4.1.00",
                ["spine-4.1.00", "spine-3.8.95"],
                true,
                Array.Empty<ViewerDiagnostic>()));

        RuntimeSelectionResult result = await _service.SelectAsync(request, CancellationToken.None);

        Assert.NotNull(result.SelectedProbeResult);
        Assert.Equal("spine-4.1.00", result.SelectedRuntime?.RuntimeId);
        Assert.True(result.IsExactVersionMatch);
        Assert.False(result.RequiresUserConfirmation);
    }

    [Fact]
    public async Task SelectAsync_FallsBackFromMissingPreferredRuntimeWithWarningAsync()
    {
        RuntimeSelectionRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineVersionMatch(
                "4.1.00",
                "spine-4.1.00",
                ["spine-4.1.00"],
                true,
                Array.Empty<ViewerDiagnostic>()),
            "spine-9.9.99");

        RuntimeSelectionResult result = await _service.SelectAsync(request, CancellationToken.None);

        Assert.Equal("spine-4.1.00", result.SelectedRuntime?.RuntimeId);
        Assert.True(result.RequiresUserConfirmation);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "runtime-selection-preferred-runtime-missing");
    }

    [Fact]
    public async Task SelectAsync_ReturnsErrorWhenNoRuntimeCanLoadAssetAsync()
    {
        RuntimeSelectionRequest request = new(
            new SpineProjectReference("Hero", "hero.txt", "hero.atlas"),
            new SpineAssetFileSet("hero.txt", "hero.atlas", ["hero.png"]),
            new SpineVersionMatch(
                null,
                null,
                Array.Empty<string>(),
                false,
                Array.Empty<ViewerDiagnostic>()));

        RuntimeSelectionResult result = await _service.SelectAsync(request, CancellationToken.None);

        Assert.Null(result.SelectedRuntime);
        Assert.True(result.RequiresUserConfirmation);
        Assert.Contains(
            result.Diagnostics,
            static diagnostic => diagnostic.Code == "runtime-selection-no-compatible-runtime");
    }
}
