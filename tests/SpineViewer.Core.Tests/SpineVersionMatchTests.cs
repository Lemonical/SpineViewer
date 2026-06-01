using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class SpineVersionMatchTests
{
    [Fact]
    public void Constructor_RejectsExactMatchWithoutSuggestedRuntime()
    {
        Assert.Throws<ArgumentException>(
            () => new SpineVersionMatch("4.1.00", null, true, Array.Empty<ViewerDiagnostic>()));
    }

    [Fact]
    public void Constructor_NormalizesOptionalStrings()
    {
        SpineVersionMatch versionMatch = new("  ", " ", false, Array.Empty<ViewerDiagnostic>());

        Assert.Null(versionMatch.DetectedExportVersion);
        Assert.Null(versionMatch.SuggestedRuntimeId);
    }
}
