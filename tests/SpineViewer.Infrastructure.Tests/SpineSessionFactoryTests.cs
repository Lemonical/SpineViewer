using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Infrastructure.Spine.Sessions;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class SpineSessionFactoryTests
{
    [Fact]
    public void Create_UsesViewerSettingsToResetTransientState()
    {
        SpineSessionFactory factory = new();
        ViewerSettings settings = new(
            ViewerTheme.FollowSystem,
            false,
            false,
            true,
            true,
            false,
            1.5,
            10,
            true,
            null);

        SpineProjectSession session = factory.Create(CreateSuccessfulLoadResult(), settings);

        Assert.False(session.Playback.IsPlaying);
        Assert.False(session.Playback.IsLooping);
        Assert.Equal(1.5, session.Playback.Speed);
        Assert.Equal(TimeSpan.Zero, session.Playback.CurrentTime);
        Assert.Equal(TimeSpan.FromSeconds(5), session.Playback.Duration);
        Assert.Equal(1.0, session.Viewport.Zoom);
        Assert.False(session.Viewport.ShowGrid);
        Assert.False(session.Viewport.ShowOrigin);
        Assert.True(session.Viewport.ShowBones);
        Assert.True(session.Viewport.ShowBounds);
        Assert.Equal(ViewportBackgroundStyle.Black, session.Viewport.BackgroundStyle);
    }

    [Fact]
    public void Create_RejectsFailedLoadResult()
    {
        SpineSessionFactory factory = new();

        Assert.Throws<ArgumentException>(
            () => factory.Create(
                CreateSuccessfulLoadResult() with
                {
                    IsSuccessful = false,
                },
                new ViewerSettings()));
    }

    [Fact]
    public void Create_WithInspection_SeedsTrackStackAndOverlayDefaults()
    {
        SpineSessionFactory factory = new();
        ViewerSettings settings = new ViewerSettings
        {
            ShowMeshWireframeByDefault = true,
            ShowSlotOutlinesByDefault = true,
            ShowLabelsByDefault = true,
            DefaultTrackMixDuration = TimeSpan.FromSeconds(0.25),
            DefaultTrackTimeScale = 1.25,
        };

        SpineProjectSession session = factory.Create(CreateSuccessfulLoadResultWithInspection(), settings);

        AnimationTrackState track = Assert.Single(session.Playback.Tracks);
        Assert.Equal("idle", track.AnimationName);
        Assert.Equal(TimeSpan.FromSeconds(0.25), track.MixDuration);
        Assert.Equal(1.25, track.TimeScale);
        Assert.True(session.Viewport.ShowMeshWireframe);
        Assert.True(session.Viewport.ShowSlotOutlines);
        Assert.True(session.Viewport.ShowLabels);
        Assert.Equal("default", session.SelectedSkinName);
    }

    private static LoadSpineProjectResult CreateSuccessfulLoadResult()
    {
        return new LoadSpineProjectResult(
            true,
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)])),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            Array.Empty<ViewerDiagnostic>());
    }

    private static LoadSpineProjectResult CreateSuccessfulLoadResultWithInspection()
    {
        SpineProjectInspection inspection = new(
            new SpineExportMetadata("4.1.00", "./images", "./audio", 512, 256, 30, 0, 0, 1, 1, 0, 0),
            [new SpineAnimationInfo("idle", TimeSpan.FromSeconds(2), 2, 4)],
            [new SpineSkinInfo("default", 1, 1, true)],
            Array.Empty<SpineBoneInfo>(),
            Array.Empty<SpineSlotInfo>(),
            Array.Empty<SpineAttachmentInfo>(),
            Array.Empty<SpineAtlasPageInfo>(),
            Array.Empty<SpineAtlasRegionInfo>(),
            Array.Empty<ViewerDiagnostic>());

        return new LoadSpineProjectResult(
            true,
            new SpineProjectReference("Hero", "hero.json", "hero.atlas"),
            new SpineAssetFileSet("hero.json", "hero.atlas", ["hero.png"]),
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)])),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            inspection,
            Array.Empty<UnsupportedSpineFeature>(),
            Array.Empty<ViewerDiagnostic>());
    }
}
