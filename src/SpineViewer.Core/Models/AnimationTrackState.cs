using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents one animation track in the playback stack.
/// </summary>
public sealed record AnimationTrackState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationTrackState"/> class.
    /// </summary>
    /// <param name="trackId">The stable identity of the track.</param>
    /// <param name="trackIndex">The zero-based runtime track index.</param>
    /// <param name="animationName">The animation assigned to the track.</param>
    /// <param name="isLooping">Indicates whether the track loops.</param>
    /// <param name="timeScale">The per-track playback speed multiplier.</param>
    /// <param name="isEnabled">Indicates whether the track participates in playback.</param>
    public AnimationTrackState(
        Guid trackId,
        int trackIndex,
        string animationName,
        bool isLooping,
        double timeScale,
        bool isEnabled)
    {
        TrackId = Guard.NonEmpty(trackId, nameof(trackId));
        TrackIndex = Guard.NonNegative(trackIndex, nameof(trackIndex));
        AnimationName = Guard.NotNullOrWhiteSpace(animationName, nameof(animationName));
        IsLooping = isLooping;
        TimeScale = Guard.PositiveFinite(timeScale, nameof(timeScale));
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// Gets the stable identity of the track.
    /// </summary>
    public Guid TrackId { get; init; }

    /// <summary>
    /// Gets the zero-based runtime track index.
    /// </summary>
    public int TrackIndex { get; init; }

    /// <summary>
    /// Gets the animation assigned to the track.
    /// </summary>
    public string AnimationName { get; init; }

    /// <summary>
    /// Gets a value indicating whether the track loops.
    /// </summary>
    public bool IsLooping { get; init; }

    /// <summary>
    /// Gets the per-track playback speed multiplier.
    /// </summary>
    public double TimeScale { get; init; }

    /// <summary>
    /// Gets a value indicating whether the track participates in playback.
    /// </summary>
    public bool IsEnabled { get; init; }
}
