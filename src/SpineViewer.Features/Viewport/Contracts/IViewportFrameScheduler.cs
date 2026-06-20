namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Schedules recurring viewport frames when playback requires continuous redraws.
/// </summary>
public interface IViewportFrameScheduler
{
    /// <summary>
    /// Gets a value indicating whether recurring frame scheduling is currently active.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Starts recurring viewport frame scheduling.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops recurring viewport frame scheduling.
    /// </summary>
    void Stop();
}
