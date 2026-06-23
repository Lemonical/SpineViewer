using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class ViewportStateTests
{
    [Fact]
    public void DefaultConstructor_UsesViewerFriendlyDefaults()
    {
        ViewportState state = new();

        Assert.Equal(1.0, state.Zoom);
        Assert.Equal(0.0, state.OffsetX);
        Assert.Equal(0.0, state.OffsetY);
        Assert.True(state.ShowGrid);
        Assert.True(state.ShowOrigin);
        Assert.False(state.ShowBones);
        Assert.False(state.ShowBounds);
        Assert.Equal(ViewportBackgroundStyle.Studio, state.BackgroundStyle);
    }

    [Fact]
    public void Constructor_RejectsNonPositiveZoom()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ViewportState(0.0, 0.0, 0.0, true, true, false, false));
    }
}
