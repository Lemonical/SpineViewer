using CommunityToolkit.Mvvm.ComponentModel;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.ViewModels;

/// <summary>
/// Coordinates viewport render-scene updates without embedding scheduling or drawing logic in the view.
/// </summary>
public sealed partial class ViewportViewModel : ObservableObject
{
    private readonly IRenderInvalidationService _renderInvalidationService;
    private readonly IViewportFrameScheduler _frameScheduler;
    private readonly IViewportSceneComposer _sceneComposer;
    private readonly IWorkspaceSessionService _workspaceSessionService;
    private ViewportHostLayout _hostLayout = new(0.0, 0.0);
    private WorkspaceState _lastWorkspaceState;
    private long _frameVersion;
    private ViewportRenderScene _renderScene;
    private bool _isActive;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportViewModel"/> class.
    /// </summary>
    /// <param name="workspaceSessionService">The workspace service that owns the current session state.</param>
    /// <param name="renderInvalidationService">The render invalidation service used to define redraw flow.</param>
    /// <param name="frameScheduler">The recurring frame scheduler used when playback is active.</param>
    /// <param name="sceneComposer">The scene composer used to build immutable render snapshots.</param>
    public ViewportViewModel(
        IWorkspaceSessionService workspaceSessionService,
        IRenderInvalidationService renderInvalidationService,
        IViewportFrameScheduler frameScheduler,
        IViewportSceneComposer sceneComposer)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
        _renderInvalidationService =
            renderInvalidationService ?? throw new ArgumentNullException(nameof(renderInvalidationService));
        _frameScheduler = frameScheduler ?? throw new ArgumentNullException(nameof(frameScheduler));
        _sceneComposer = sceneComposer ?? throw new ArgumentNullException(nameof(sceneComposer));

        _lastWorkspaceState = _workspaceSessionService.State;
        _renderScene = _sceneComposer.Compose(_lastWorkspaceState, _hostLayout, _frameVersion);

        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
        _renderInvalidationService.RenderInvalidated += OnRenderInvalidated;
    }

    /// <summary>
    /// Gets the immutable render scene for the current frame.
    /// </summary>
    public ViewportRenderScene RenderScene
    {
        get => _renderScene;
        private set => SetProperty(ref _renderScene, value);
    }

    /// <summary>
    /// Activates the viewport and enables recurring frame scheduling when playback requires it.
    /// </summary>
    public void Activate()
    {
        _isActive = true;
        RefreshRenderScene();
        UpdateScheduler();
    }

    /// <summary>
    /// Deactivates the viewport and stops recurring frame scheduling.
    /// </summary>
    public void Deactivate()
    {
        _isActive = false;
        _frameScheduler.Stop();
    }

    /// <summary>
    /// Updates the current render-host size.
    /// </summary>
    /// <param name="width">The current host width in device-independent pixels.</param>
    /// <param name="height">The current host height in device-independent pixels.</param>
    public void UpdateHostLayout(double width, double height)
    {
        ViewportHostLayout updatedLayout = new(width, height);

        if (_hostLayout == updatedLayout)
        {
            return;
        }

        _hostLayout = updatedLayout;
        UpdateScheduler();
        _renderInvalidationService.RequestInvalidation(RenderInvalidationReason.HostLayoutChanged);
    }

    private static RenderInvalidationReason DetermineInvalidationReason(
        WorkspaceState previousState,
        WorkspaceState currentState)
    {
        if (previousState.CurrentSession?.SessionId != currentState.CurrentSession?.SessionId)
        {
            return RenderInvalidationReason.SessionChanged;
        }

        if (!Equals(previousState.CurrentSession?.Viewport, currentState.CurrentSession?.Viewport))
        {
            return RenderInvalidationReason.ViewportChanged;
        }

        if (!Equals(previousState.CurrentSession?.Playback, currentState.CurrentSession?.Playback))
        {
            return RenderInvalidationReason.PlaybackChanged;
        }

        return RenderInvalidationReason.SessionChanged;
    }

    private void OnRenderInvalidated(object? sender, RenderInvalidatedEventArgs e)
    {
        _frameVersion++;
        RefreshRenderScene();
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        WorkspaceState currentState = _workspaceSessionService.State;
        RenderInvalidationReason reason = DetermineInvalidationReason(_lastWorkspaceState, currentState);
        _lastWorkspaceState = currentState;

        UpdateScheduler();
        _renderInvalidationService.RequestInvalidation(reason);
    }

    private void RefreshRenderScene()
    {
        RenderScene = _sceneComposer.Compose(_workspaceSessionService.State, _hostLayout, _frameVersion);
    }

    private void UpdateScheduler()
    {
        bool shouldRun = _isActive &&
                         _hostLayout.HasSurface &&
                         _workspaceSessionService.State.CurrentSession?.Playback.IsPlaying == true;

        if (shouldRun)
        {
            _frameScheduler.Start();
            return;
        }

        _frameScheduler.Stop();
    }
}
