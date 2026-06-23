using Avalonia.Media;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Composes immutable viewport render scenes from workspace state and dedicated overlay components.
/// </summary>
public sealed class ViewportSceneComposer : IViewportSceneComposer
{
    private static readonly Color EmptyBackgroundColor = Color.FromRgb(0x0D, 0x11, 0x18);
    private static readonly Color BusyBackgroundColor = Color.FromRgb(0x18, 0x1F, 0x2B);
    private readonly IViewportCameraService _cameraService;
    private readonly IReadOnlyList<IViewportOverlaySource> _overlaySources;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportSceneComposer"/> class.
    /// </summary>
    /// <param name="cameraService">The camera service used to build the frame transform.</param>
    /// <param name="overlaySources">The ordered overlay sources that contribute frame primitives.</param>
    public ViewportSceneComposer(
        IViewportCameraService cameraService,
        IEnumerable<IViewportOverlaySource> overlaySources)
    {
        _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
        _overlaySources = overlaySources?.ToArray() ?? throw new ArgumentNullException(nameof(overlaySources));
    }

    /// <inheritdoc />
    public ViewportRenderScene Compose(
        WorkspaceState workspaceState,
        ViewportHostLayout hostLayout,
        long frameVersion)
    {
        ArgumentNullException.ThrowIfNull(workspaceState);
        ArgumentNullException.ThrowIfNull(hostLayout);

        ViewportState viewportState = workspaceState.CurrentSession?.Viewport ?? new ViewportState();
        ViewportRenderTransform transform = _cameraService.CreateTransform(viewportState, hostLayout);
        ViewportOverlayContext context = new(workspaceState, viewportState, hostLayout);
        List<ViewportOverlayLine> worldLines = [];

        foreach (IViewportOverlaySource overlaySource in _overlaySources)
        {
            worldLines.AddRange(overlaySource.CreateWorldLines(context));
        }

        return new ViewportRenderScene(
            SelectBackgroundColor(workspaceState),
            transform,
            worldLines,
            frameVersion,
            workspaceState.CurrentSession is not null,
            workspaceState.CurrentSession?.Project.DisplayName);
    }

    private static Color SelectBackgroundColor(WorkspaceState workspaceState)
    {
        if (workspaceState.IsBusy)
        {
            return BusyBackgroundColor;
        }

        if (workspaceState.CurrentSession is null)
        {
            return EmptyBackgroundColor;
        }

        return workspaceState.CurrentSession.Viewport.BackgroundStyle switch
        {
            ViewportBackgroundStyle.Studio => Color.FromRgb(0x10, 0x15, 0x1F),
            ViewportBackgroundStyle.Slate => Color.FromRgb(0x1B, 0x24, 0x33),
            ViewportBackgroundStyle.Blueprint => Color.FromRgb(0x0D, 0x24, 0x38),
            ViewportBackgroundStyle.Paper => Color.FromRgb(0xE7, 0xE0, 0xD3),
            _ => Color.FromRgb(0x10, 0x15, 0x1F),
        };
    }
}
