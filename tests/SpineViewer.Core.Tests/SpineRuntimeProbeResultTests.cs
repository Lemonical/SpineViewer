using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class SpineRuntimeProbeResultTests
{
    [Fact]
    public void Constructor_RejectsSupportedResultWithUnsupportedFeatures()
    {
        SpineRuntimeDescriptor runtime = new(
            "spine-4.1.00",
            "Spine 4.1.00",
            "4.1.x",
            new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)]));

        Assert.Throws<ArgumentException>(
            () => new SpineRuntimeProbeResult(
                runtime,
                SpineRuntimeSupportStatus.Supported,
                [new UnsupportedSpineFeature("clip", "Clipping")],
                Array.Empty<ViewerDiagnostic>()));
    }

    [Fact]
    public void Constructor_RejectsWarningResultWithoutWarningDetails()
    {
        SpineRuntimeDescriptor runtime = new(
            "spine-4.1.00",
            "Spine 4.1.00",
            "4.1.x",
            new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)]));

        Assert.Throws<ArgumentException>(
            () => new SpineRuntimeProbeResult(
                runtime,
                SpineRuntimeSupportStatus.SupportedWithWarnings,
                Array.Empty<UnsupportedSpineFeature>(),
                Array.Empty<ViewerDiagnostic>()));
    }
}
