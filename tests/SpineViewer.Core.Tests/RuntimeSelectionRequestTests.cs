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
            " ");

        Assert.Null(request.PreferredRuntimeId);
    }

    [Fact]
    public void Constructor_AssignsProjectAndVersionInputs()
    {
        RuntimeSelectionRequest request = new(
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineVersionMatch(
                "3.8.95",
                "spine-3.8.95",
                ["spine-3.8.95", "spine-4.1.00"],
                true,
                Array.Empty<ViewerDiagnostic>()));

        Assert.Equal("Hero", request.ProjectReference.DisplayName);
        Assert.Equal(["spine-3.8.95", "spine-4.1.00"], request.VersionMatch.CompatibleRuntimeIds);
    }
}
