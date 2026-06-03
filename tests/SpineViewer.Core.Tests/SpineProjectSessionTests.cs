using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class SpineProjectSessionTests
{
    [Fact]
    public void Constructor_RejectsEmptySessionIdentifier()
    {
        Assert.Throws<ArgumentException>(
            () => new SpineProjectSession(
                Guid.Empty,
                CreateProjectReference(),
                CreateAssetFileSet(),
                CreateRuntimeDescriptor(),
                CreateVersionMatch(),
                new PlaybackState(),
                new ViewportState(),
                Array.Empty<ViewerDiagnostic>()));
    }

    [Fact]
    public void Constructor_PreservesAggregateState()
    {
        SpineProjectReference projectReference = CreateProjectReference();
        SpineAssetFileSet assetFileSet = CreateAssetFileSet();
        SpineRuntimeDescriptor runtimeDescriptor = CreateRuntimeDescriptor();
        SpineVersionMatch versionMatch = CreateVersionMatch();
        PlaybackState playbackState = new();
        ViewportState viewportState = new();
        ViewerDiagnostic diagnostic = new(
            "SV1001",
            ViewerDiagnosticSeverity.Warning,
            "Texture was missing at initial probe time.",
            "Atlas",
            suggestedAction: "Retry after restoring the atlas page.");

        SpineProjectSession session = new(
            Guid.NewGuid(),
            projectReference,
            assetFileSet,
            runtimeDescriptor,
            versionMatch,
            playbackState,
            viewportState,
            [diagnostic]);

        Assert.Equal(projectReference, session.Project);
        Assert.Equal(assetFileSet, session.AssetFileSet);
        Assert.Equal(runtimeDescriptor, session.Runtime);
        Assert.Equal(versionMatch, session.VersionMatch);
        Assert.Equal(playbackState, session.Playback);
        Assert.Equal(viewportState, session.Viewport);
        Assert.Equal(diagnostic, Assert.Single(session.Diagnostics));
    }

    private static SpineAssetFileSet CreateAssetFileSet()
    {
        return new SpineAssetFileSet(
            "fixtures/hero.json",
            "fixtures/hero.atlas",
            ["fixtures/hero.png"]);
    }

    private static SpineProjectReference CreateProjectReference()
    {
        return new SpineProjectReference("Hero", "fixtures/hero.json", "fixtures/hero.atlas");
    }

    private static SpineRuntimeDescriptor CreateRuntimeDescriptor()
    {
        return new SpineRuntimeDescriptor(
            "spine-4.1.00",
            "Spine 4.1.00",
            "4.1.x",
            new SpineRuntimeCapabilities(
                [
                    new(SpineRuntimeFeature.JsonSkeleton, true),
                    new(SpineRuntimeFeature.BinarySkeleton, true),
                    new(SpineRuntimeFeature.Events, true),
                    new(SpineRuntimeFeature.Clipping, true),
                    new(SpineRuntimeFeature.Meshes, true),
                    new(SpineRuntimeFeature.MultipleTracks, true),
                ]));
    }

    private static SpineVersionMatch CreateVersionMatch()
    {
        return new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>());
    }
}
