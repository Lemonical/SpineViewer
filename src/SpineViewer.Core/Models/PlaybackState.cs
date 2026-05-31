using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the current playback transport state for a loaded session.
/// </summary>
public sealed record PlaybackState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackState"/> class with default playback settings.
    /// </summary>
    public PlaybackState()
        : this(false, true, 1.0, TimeSpan.Zero, Array.Empty<AnimationTrackState>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackState"/> class.
    /// </summary>
    /// <param name="isPlaying">Indicates whether playback is currently advancing.</param>
    /// <param name="isLooping">Indicates whether playback loops by default.</param>
    /// <param name="speed">The global playback speed multiplier.</param>
    /// <param name="currentTime">The current playback time within the active preview.</param>
    /// <param name="tracks">The active animation track stack.</param>
    public PlaybackState(
        bool isPlaying,
        bool isLooping,
        double speed,
        TimeSpan currentTime,
        IEnumerable<AnimationTrackState> tracks)
    {
        IsPlaying = isPlaying;
        IsLooping = isLooping;
        Speed = Guard.PositiveFinite(speed, nameof(speed));
        CurrentTime = Guard.NonNegative(currentTime, nameof(currentTime));
        Tracks = Guard.MaterializeReadOnlyList(tracks, nameof(tracks));
    }

    /// <summary>
    /// Gets a value indicating whether playback is currently advancing.
    /// </summary>
    public bool IsPlaying { get; init; }

    /// <summary>
    /// Gets a value indicating whether playback loops by default.
    /// </summary>
    public bool IsLooping { get; init; }

    /// <summary>
    /// Gets the global playback speed multiplier.
    /// </summary>
    public double Speed { get; init; }

    /// <summary>
    /// Gets the current playback time within the active preview.
    /// </summary>
    public TimeSpan CurrentTime { get; init; }

    /// <summary>
    /// Gets the active animation track stack.
    /// </summary>
    public IReadOnlyList<AnimationTrackState> Tracks { get; init; }
}
