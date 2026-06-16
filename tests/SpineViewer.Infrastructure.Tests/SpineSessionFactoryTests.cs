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
        Assert.Equal(1.0, session.Viewport.Zoom);
        Assert.False(session.Viewport.ShowGrid);
        Assert.False(session.Viewport.ShowOrigin);
        Assert.True(session.Viewport.ShowBones);
        Assert.True(session.Viewport.ShowBounds);
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
}
