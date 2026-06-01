using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class RuntimeSelectionRequestTests
{
    [Fact]
    public void Constructor_NormalizesBlankPreferredRuntime()
    {
        RuntimeSelectionRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            [new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities(true, true, true, true, true, true))],
            " ");

        Assert.Null(request.PreferredRuntimeId);
    }

    [Fact]
    public void Constructor_MaterializesAvailableRuntimeCollection()
    {
        List<SpineRuntimeDescriptor> runtimes =
        [
            new(
                "spine-3.8.95",
                "Spine 3.8.95",
                "3.8.x",
                new SpineRuntimeCapabilities(true, true, true, false, true, true)),
        ];

        RuntimeSelectionRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineVersionMatch("3.8.95", "spine-3.8.95", true, Array.Empty<ViewerDiagnostic>()),
            runtimes);

        runtimes.Add(
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities(true, true, true, true, true, true)));

        Assert.Single(request.AvailableRuntimes);
    }
}
