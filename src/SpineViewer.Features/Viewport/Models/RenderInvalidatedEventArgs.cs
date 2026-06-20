namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Describes one viewport render invalidation request.
/// </summary>
public sealed class RenderInvalidatedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderInvalidatedEventArgs"/> class.
    /// </summary>
    /// <param name="reason">The reason the viewport requested a new frame.</param>
    public RenderInvalidatedEventArgs(RenderInvalidationReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets the reason the viewport requested a new frame.
    /// </summary>
    public RenderInvalidationReason Reason { get; }
}
