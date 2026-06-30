using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class ViewerSettingsTests
{
    [Fact]
    public void DefaultConstructor_UsesReleaseOneDefaults()
    {
        ViewerSettings settings = new();

        Assert.Equal(ViewerTheme.FollowSystem, settings.Theme);
        Assert.True(settings.ShowGridByDefault);
        Assert.True(settings.ShowOriginByDefault);
        Assert.False(settings.ShowBonesByDefault);
        Assert.False(settings.ShowBoundsByDefault);
        Assert.True(settings.LoopPlaybackByDefault);
        Assert.Equal(1.0, settings.DefaultPlaybackSpeed);
        Assert.Equal(10, settings.RecentFilesLimit);
        Assert.True(settings.RestoreLastSessionOnStartup);
        Assert.True(settings.UseCustomTitleBar);
        Assert.Null(settings.LastProjectReference);
    }

    [Fact]
    public void Constructor_RejectsInvalidPlaybackSpeed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ViewerSettings(
                ViewerTheme.Dark,
                true,
                true,
                false,
                false,
                true,
                0.0,
                10));
    }

    [Fact]
    public void Constructor_RejectsNonPositiveRecentFilesLimit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ViewerSettings(
                ViewerTheme.Dark,
                true,
                true,
                false,
                false,
                true,
                1.0,
                0));
    }
}
