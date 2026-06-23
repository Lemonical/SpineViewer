using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the current playback transport state for a loaded session.
/// </summary>
public sealed record PlaybackState
{
    private static readonly TimeSpan DefaultTimelineDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackState"/> class with default playback settings.
    /// </summary>
    public PlaybackState()
        : this(
            PlaybackTransportStatus.Stopped,
            true,
            1.0,
            TimeSpan.Zero,
            DefaultTimelineDuration,
            Array.Empty<AnimationTrackState>())
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
        : this(
            isPlaying
                ? PlaybackTransportStatus.Playing
                : currentTime > TimeSpan.Zero
                    ? PlaybackTransportStatus.Paused
                    : PlaybackTransportStatus.Stopped,
            isLooping,
            speed,
            currentTime,
            DefaultTimelineDuration,
            tracks)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackState"/> class.
    /// </summary>
    /// <param name="status">The visible transport state.</param>
    /// <param name="isLooping">Indicates whether playback loops by default.</param>
    /// <param name="speed">The global playback speed multiplier.</param>
    /// <param name="currentTime">The current playback time within the active preview.</param>
    /// <param name="duration">The visible duration of the active preview timeline.</param>
    /// <param name="tracks">The active animation track stack.</param>
    public PlaybackState(
        PlaybackTransportStatus status,
        bool isLooping,
        double speed,
        TimeSpan currentTime,
        TimeSpan duration,
        IEnumerable<AnimationTrackState> tracks)
    {
        Status = status;
        IsLooping = isLooping;
        Speed = Guard.PositiveFinite(speed, nameof(speed));
        CurrentTime = Guard.NonNegative(currentTime, nameof(currentTime));
        Duration = Guard.Positive(duration, nameof(duration));
        Tracks = Guard.MaterializeReadOnlyList(tracks, nameof(tracks));

        if (CurrentTime > Duration)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentTime),
                "Current time cannot exceed the playback duration.");
        }
    }

    /// <summary>
    /// Gets the visible transport status.
    /// </summary>
    public PlaybackTransportStatus Status { get; init; }

    /// <summary>
    /// Gets a value indicating whether playback is currently advancing.
    /// </summary>
    public bool IsPlaying => Status == PlaybackTransportStatus.Playing;

    /// <summary>
    /// Gets a value indicating whether playback is currently paused.
    /// </summary>
    public bool IsPaused => Status == PlaybackTransportStatus.Paused;

    /// <summary>
    /// Gets a value indicating whether playback is currently stopped.
    /// </summary>
    public bool IsStopped => Status == PlaybackTransportStatus.Stopped;

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
    /// Gets the visible duration of the active preview timeline.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the active animation track stack.
    /// </summary>
    public IReadOnlyList<AnimationTrackState> Tracks { get; init; }
}
