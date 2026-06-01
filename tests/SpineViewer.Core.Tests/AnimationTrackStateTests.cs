using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class AnimationTrackStateTests
{
    [Fact]
    public void Constructor_RejectsNegativeTrackIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new AnimationTrackState(Guid.NewGuid(), -1, "idle", true, 1.0, true));
    }

    [Fact]
    public void Constructor_RejectsEmptyTrackIdentifier()
    {
        Assert.Throws<ArgumentException>(
            () => new AnimationTrackState(Guid.Empty, 0, "idle", true, 1.0, true));
    }
}
