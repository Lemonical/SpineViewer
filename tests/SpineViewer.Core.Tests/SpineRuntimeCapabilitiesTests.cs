using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class SpineRuntimeCapabilitiesTests
{
    [Fact]
    public void Supports_ReturnsTrueForDeclaredSupportedFeature()
    {
        SpineRuntimeCapabilities capabilities = new(
            [
                new(SpineRuntimeFeature.JsonSkeleton, true),
                new(SpineRuntimeFeature.BinarySkeleton, false),
            ]);

        Assert.True(capabilities.Supports(SpineRuntimeFeature.JsonSkeleton));
        Assert.False(capabilities.Supports(SpineRuntimeFeature.BinarySkeleton));
    }

    [Fact]
    public void Constructor_RejectsDuplicateFeatureMetadata()
    {
        Assert.Throws<ArgumentException>(
            () => new SpineRuntimeCapabilities(
                [
                    new(SpineRuntimeFeature.JsonSkeleton, true),
                    new(SpineRuntimeFeature.JsonSkeleton, false),
                ]));
    }
}
