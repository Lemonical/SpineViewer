using SpineViewer.Core.Models;

namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Carries the current workspace and camera state into dedicated viewport overlay components.
/// </summary>
public sealed class ViewportOverlayContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportOverlayContext"/> class.
    /// </summary>
    /// <param name="workspaceState">The current workspace state snapshot.</param>
    /// <param name="viewportState">The current viewport state snapshot.</param>
    /// <param name="hostLayout">The current render-host layout.</param>
    public ViewportOverlayContext(
        WorkspaceState workspaceState,
        ViewportState viewportState,
        ViewportHostLayout hostLayout)
    {
        WorkspaceState = workspaceState ?? throw new ArgumentNullException(nameof(workspaceState));
        ViewportState = viewportState ?? throw new ArgumentNullException(nameof(viewportState));
        HostLayout = hostLayout ?? throw new ArgumentNullException(nameof(hostLayout));
    }

    /// <summary>
    /// Gets the current workspace state snapshot.
    /// </summary>
    public WorkspaceState WorkspaceState { get; }

    /// <summary>
    /// Gets the current viewport state snapshot.
    /// </summary>
    public ViewportState ViewportState { get; }

    /// <summary>
    /// Gets the current render-host layout.
    /// </summary>
    public ViewportHostLayout HostLayout { get; }

    /// <summary>
    /// Gets the active session, if any.
    /// </summary>
    public SpineProjectSession? CurrentSession => WorkspaceState.CurrentSession;

    /// <summary>
    /// Gets a value indicating whether a session is currently active.
    /// </summary>
    public bool HasActiveSession => CurrentSession is not null;
}
