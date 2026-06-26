using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes one exported Spine animation and its inspection metadata.
/// </summary>
public sealed record SpineAnimationInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineAnimationInfo"/> class.
    /// </summary>
    /// <param name="name">The animation name.</param>
    /// <param name="duration">The inspected animation duration.</param>
    /// <param name="timelineCount">The number of timelines found in the animation.</param>
    /// <param name="keyframeCount">The total number of keyframes found in the animation.</param>
    public SpineAnimationInfo(
        string name,
        TimeSpan duration,
        int timelineCount,
        int keyframeCount)
    {
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name));
        Duration = Guard.NonNegative(duration, nameof(duration));
        TimelineCount = Guard.NonNegative(timelineCount, nameof(timelineCount));
        KeyframeCount = Guard.NonNegative(keyframeCount, nameof(keyframeCount));
    }

    /// <summary>
    /// Gets the animation name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the inspected animation duration.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Gets the number of timelines found in the animation.
    /// </summary>
    public int TimelineCount { get; init; }

    /// <summary>
    /// Gets the total number of keyframes found in the animation.
    /// </summary>
    public int KeyframeCount { get; init; }
}
