using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class SpineVersionMatchTests
{
    [Fact]
    public void Constructor_RejectsExactMatchWithoutSuggestedRuntime()
    {
        Assert.Throws<ArgumentException>(
            () => new SpineVersionMatch(
                "4.1.00",
                null,
                ["spine-4.1.00"],
                true,
                Array.Empty<ViewerDiagnostic>()));
    }

    [Fact]
    public void Constructor_NormalizesOptionalStrings()
    {
        SpineVersionMatch versionMatch = new("  ", " ", false, Array.Empty<ViewerDiagnostic>());

        Assert.Null(versionMatch.DetectedExportVersion);
        Assert.Null(versionMatch.SuggestedRuntimeId);
    }

    [Fact]
    public void Constructor_MaterializesDistinctCompatibleRuntimeIds()
    {
        SpineVersionMatch versionMatch = new(
            "4.1.00",
            "spine-4.1.00",
            ["spine-4.1.00", "SPINE-4.1.00", "spine-3.8.95"],
            false,
            Array.Empty<ViewerDiagnostic>());

        Assert.Equal(["spine-4.1.00", "spine-3.8.95"], versionMatch.CompatibleRuntimeIds);
    }
}
