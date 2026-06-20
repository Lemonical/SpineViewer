using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Contracts;

/// <summary>
/// Composes immutable viewport render scenes from workspace, camera, and overlay services.
/// </summary>
public interface IViewportSceneComposer
{
    /// <summary>
    /// Composes a render scene for the current workspace frame.
    /// </summary>
    /// <param name="workspaceState">The current workspace state.</param>
    /// <param name="hostLayout">The current render-host layout.</param>
    /// <param name="frameVersion">The monotonically increasing frame version.</param>
    /// <returns>The immutable render scene for the frame.</returns>
    ViewportRenderScene Compose(
        WorkspaceState workspaceState,
        ViewportHostLayout hostLayout,
        long frameVersion);
}
