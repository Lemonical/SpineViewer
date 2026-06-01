using SpineViewer.Core.Models;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class PlaybackStateTests
{
    [Fact]
    public void DefaultConstructor_UsesExpectedDefaults()
    {
        PlaybackState state = new();

        Assert.False(state.IsPlaying);
        Assert.True(state.IsLooping);
        Assert.Equal(1.0, state.Speed);
        Assert.Equal(TimeSpan.Zero, state.CurrentTime);
        Assert.Empty(state.Tracks);
    }

    [Fact]
    public void Constructor_RejectsNegativeCurrentTime()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PlaybackState(false, true, 1.0, TimeSpan.FromSeconds(-1), Array.Empty<AnimationTrackState>()));
    }

    [Fact]
    public void Constructor_MaterializesTrackCollection()
    {
        List<AnimationTrackState> tracks =
        [
            new(Guid.NewGuid(), 0, "idle", true, 1.0, true),
        ];

        PlaybackState state = new(false, true, 1.0, TimeSpan.Zero, tracks);
        tracks.Add(new AnimationTrackState(Guid.NewGuid(), 1, "walk", true, 1.0, true));

        Assert.Single(state.Tracks);
    }
}
