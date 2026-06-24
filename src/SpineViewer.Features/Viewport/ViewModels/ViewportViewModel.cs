using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.ViewModels;

/// <summary>
/// Coordinates viewport rendering, camera interaction, and overlay controls without embedding draw logic in the view.
/// </summary>
public sealed partial class ViewportViewModel : ObservableObject
{
    private const double KeyboardZoomFactor = 1.15;
    private readonly IRenderInvalidationService _renderInvalidationService;
    private readonly IViewportFrameScheduler _frameScheduler;
    private readonly IViewportCameraService _cameraService;
    private readonly IViewportSceneComposer _sceneComposer;
    private readonly IWorkspaceSessionService _workspaceSessionService;
    private readonly ViewportBackgroundStyle[] _backgroundOptions =
        Enum.GetValues<ViewportBackgroundStyle>();
    private ViewportHostLayout _hostLayout = new(0.0, 0.0);
    private WorkspaceState _lastWorkspaceState;
    private ViewportState _currentViewportState;
    private long _frameVersion;
    private ViewportRenderScene _renderScene;
    private bool _isActive;
    private bool _isSynchronizingControls;
    private bool _isPanning;
    private double _lastPointerX;
    private double _lastPointerY;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportViewModel"/> class.
    /// </summary>
    /// <param name="workspaceSessionService">The workspace service that owns the current session state.</param>
    /// <param name="renderInvalidationService">The render invalidation service used to define redraw flow.</param>
    /// <param name="frameScheduler">The recurring frame scheduler used when playback is active.</param>
    /// <param name="cameraService">The camera service used for camera-state mutations.</param>
    /// <param name="sceneComposer">The scene composer used to build immutable render snapshots.</param>
    public ViewportViewModel(
        IWorkspaceSessionService workspaceSessionService,
        IRenderInvalidationService renderInvalidationService,
        IViewportFrameScheduler frameScheduler,
        IViewportCameraService cameraService,
        IViewportSceneComposer sceneComposer)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
        _renderInvalidationService =
            renderInvalidationService ?? throw new ArgumentNullException(nameof(renderInvalidationService));
        _frameScheduler = frameScheduler ?? throw new ArgumentNullException(nameof(frameScheduler));
        _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
        _sceneComposer = sceneComposer ?? throw new ArgumentNullException(nameof(sceneComposer));

        _lastWorkspaceState = _workspaceSessionService.State;
        _currentViewportState = _lastWorkspaceState.CurrentSession?.Viewport ?? new ViewportState();
        _renderScene = _sceneComposer.Compose(_lastWorkspaceState, _hostLayout, _frameVersion);

        SynchronizeControlsFromViewportState(_currentViewportState, _lastWorkspaceState.CurrentSession is not null);
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
    /// Gets the selectable viewport background options.
    /// </summary>
    public IReadOnlyList<ViewportBackgroundStyle> BackgroundOptions => _backgroundOptions;

    /// <summary>
    /// Gets the visible zoom summary for the current viewport.
    /// </summary>
    public string ZoomSummaryText => $"{_currentViewportState.Zoom * 100.0:0}%";

    /// <summary>
    /// Gets the concise interaction guidance shown beside the viewport controls.
    /// </summary>
    public string InteractionHintText =>
        "Drag to pan. Wheel to zoom the pointer. F fits the preview. 0 resets the camera.";

    /// <summary>
    /// Gets a value indicating whether a session is currently loaded into the viewport.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FitToViewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCameraCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomInCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZoomOutCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleGridCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleOriginCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleBonesCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleBoundsCommand))]
    [NotifyCanExecuteChangedFor(nameof(CycleBackgroundCommand))]
    private bool hasActiveSession;

    /// <summary>
    /// Gets a value indicating whether the grid overlay is visible.
    /// </summary>
    [ObservableProperty]
    private bool isGridVisible;

    /// <summary>
    /// Gets a value indicating whether the origin overlay is visible.
    /// </summary>
    [ObservableProperty]
    private bool isOriginVisible;

    /// <summary>
    /// Gets a value indicating whether the bones overlay is visible.
    /// </summary>
    [ObservableProperty]
    private bool isBonesVisible;

    /// <summary>
    /// Gets a value indicating whether the bounds overlay is visible.
    /// </summary>
    [ObservableProperty]
    private bool isBoundsVisible;

    /// <summary>
    /// Gets the currently selected background style.
    /// </summary>
    [ObservableProperty]
    private ViewportBackgroundStyle selectedBackgroundStyle;

    /// <summary>
    /// Activates the viewport and enables recurring frame scheduling when playback requires it.
    /// </summary>
    public void Activate()
    {
        _isActive = true;
        RefreshRenderScene();
        TryAutoFitCurrentSessionIfDefault();
        UpdateScheduler();
    }

    /// <summary>
    /// Begins a pointer-driven panning gesture.
    /// </summary>
    /// <param name="pointerX">The pointer X position in device-independent pixels.</param>
    /// <param name="pointerY">The pointer Y position in device-independent pixels.</param>
    public void BeginPan(double pointerX, double pointerY)
    {
        if (!CanAdjustSessionViewport())
        {
            return;
        }

        _isPanning = true;
        _lastPointerX = pointerX;
        _lastPointerY = pointerY;
    }

    /// <summary>
    /// Deactivates the viewport and stops recurring frame scheduling.
    /// </summary>
    public void Deactivate()
    {
        _isActive = false;
        _isPanning = false;
        _frameScheduler.Stop();
    }

    /// <summary>
    /// Ends the current pointer-driven panning gesture.
    /// </summary>
    public void EndPan()
    {
        _isPanning = false;
    }

    /// <summary>
    /// Applies mouse-wheel zoom centered on the supplied pointer position.
    /// </summary>
    /// <param name="pointerX">The pointer X position in device-independent pixels.</param>
    /// <param name="pointerY">The pointer Y position in device-independent pixels.</param>
    /// <param name="wheelDelta">The incoming wheel delta.</param>
    public void HandlePointerWheel(
        double pointerX,
        double pointerY,
        double wheelDelta)
    {
        if (!CanAdjustSessionViewport() || wheelDelta == 0.0)
        {
            return;
        }

        double zoomFactor = Math.Pow(1.12, wheelDelta);
        ApplyViewportState(
            _cameraService.ZoomAtPoint(
                _currentViewportState,
                _hostLayout,
                pointerX,
                pointerY,
                zoomFactor));
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
        NotifyCameraCommandAvailabilityChanged();

        if (TryAutoFitCurrentSessionIfDefault())
        {
            return;
        }

        UpdateScheduler();
        _renderInvalidationService.RequestInvalidation(RenderInvalidationReason.HostLayoutChanged);
    }

    /// <summary>
    /// Continues the current pointer-driven panning gesture.
    /// </summary>
    /// <param name="pointerX">The pointer X position in device-independent pixels.</param>
    /// <param name="pointerY">The pointer Y position in device-independent pixels.</param>
    public void UpdatePan(double pointerX, double pointerY)
    {
        if (!_isPanning || !CanAdjustSessionViewport())
        {
            return;
        }

        double deltaX = pointerX - _lastPointerX;
        double deltaY = pointerY - _lastPointerY;
        _lastPointerX = pointerX;
        _lastPointerY = pointerY;

        if (deltaX == 0.0 && deltaY == 0.0)
        {
            return;
        }

        ApplyViewportState(_cameraService.Pan(_currentViewportState, deltaX, deltaY));
    }

    partial void OnIsBonesVisibleChanged(bool value)
    {
        UpdateOverlayVisibility(
            _currentViewportState.ShowGrid,
            _currentViewportState.ShowOrigin,
            value,
            _currentViewportState.ShowBounds);
    }

    partial void OnIsBoundsVisibleChanged(bool value)
    {
        UpdateOverlayVisibility(_currentViewportState.ShowGrid, _currentViewportState.ShowOrigin, _currentViewportState.ShowBones, value);
    }

    partial void OnIsGridVisibleChanged(bool value)
    {
        UpdateOverlayVisibility(value, _currentViewportState.ShowOrigin, _currentViewportState.ShowBones, _currentViewportState.ShowBounds);
    }

    partial void OnIsOriginVisibleChanged(bool value)
    {
        UpdateOverlayVisibility(_currentViewportState.ShowGrid, value, _currentViewportState.ShowBones, _currentViewportState.ShowBounds);
    }

    partial void OnSelectedBackgroundStyleChanged(ViewportBackgroundStyle value)
    {
        if (_isSynchronizingControls || !HasActiveSession)
        {
            return;
        }

        ApplyViewportState(_currentViewportState with { BackgroundStyle = value });
    }

    [RelayCommand(CanExecute = nameof(CanAdjustSessionViewport))]
    private void FitToView()
    {
        if (RenderScene.ContentBounds is null)
        {
            return;
        }

        ApplyViewportState(_cameraService.FitToView(_currentViewportState, _hostLayout, RenderScene.ContentBounds));
    }

    [RelayCommand(CanExecute = nameof(CanAdjustSessionViewport))]
    private void ResetCamera()
    {
        ApplyViewportState(_cameraService.Reset(_currentViewportState));
    }

    [RelayCommand(CanExecute = nameof(CanAdjustSessionViewport))]
    private void ZoomIn()
    {
        ApplyViewportState(
            _cameraService.ZoomAtPoint(
                _currentViewportState,
                _hostLayout,
                _hostLayout.Width / 2.0,
                _hostLayout.Height / 2.0,
                KeyboardZoomFactor));
    }

    [RelayCommand(CanExecute = nameof(CanAdjustSessionViewport))]
    private void ZoomOut()
    {
        ApplyViewportState(
            _cameraService.ZoomAtPoint(
                _currentViewportState,
                _hostLayout,
                _hostLayout.Width / 2.0,
                _hostLayout.Height / 2.0,
                1.0 / KeyboardZoomFactor));
    }

    [RelayCommand(CanExecute = nameof(CanToggleSessionViewport))]
    private void ToggleGrid()
    {
        IsGridVisible = !IsGridVisible;
    }

    [RelayCommand(CanExecute = nameof(CanToggleSessionViewport))]
    private void ToggleOrigin()
    {
        IsOriginVisible = !IsOriginVisible;
    }

    [RelayCommand(CanExecute = nameof(CanToggleSessionViewport))]
    private void ToggleBones()
    {
        IsBonesVisible = !IsBonesVisible;
    }

    [RelayCommand(CanExecute = nameof(CanToggleSessionViewport))]
    private void ToggleBounds()
    {
        IsBoundsVisible = !IsBoundsVisible;
    }

    [RelayCommand(CanExecute = nameof(CanToggleSessionViewport))]
    private void CycleBackground()
    {
        int currentIndex = Array.IndexOf(_backgroundOptions, SelectedBackgroundStyle);
        int nextIndex = (currentIndex + 1) % _backgroundOptions.Length;
        SelectedBackgroundStyle = _backgroundOptions[nextIndex];
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

    private void ApplyViewportState(ViewportState viewportState)
    {
        if (!HasActiveSession || viewportState == _currentViewportState)
        {
            return;
        }

        _workspaceSessionService.UpdateViewportState(viewportState);
    }

    private bool CanAdjustSessionViewport()
    {
        return HasActiveSession && _hostLayout.HasSurface;
    }

    private bool CanToggleSessionViewport()
    {
        return HasActiveSession;
    }

    private void NotifyCameraCommandAvailabilityChanged()
    {
        FitToViewCommand.NotifyCanExecuteChanged();
        ResetCameraCommand.NotifyCanExecuteChanged();
        ZoomInCommand.NotifyCanExecuteChanged();
        ZoomOutCommand.NotifyCanExecuteChanged();
    }

    private void OnRenderInvalidated(object? sender, RenderInvalidatedEventArgs e)
    {
        _frameVersion++;
        RefreshRenderScene();
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        WorkspaceState previousState = _lastWorkspaceState;
        WorkspaceState currentState = _workspaceSessionService.State;
        RenderInvalidationReason reason = DetermineInvalidationReason(previousState, currentState);
        _lastWorkspaceState = currentState;
        _currentViewportState = currentState.CurrentSession?.Viewport ?? new ViewportState();

        SynchronizeControlsFromViewportState(_currentViewportState, currentState.CurrentSession is not null);
        UpdateScheduler();

        if (TryAutoFitSessionIfNeeded(previousState, currentState))
        {
            return;
        }

        if (reason == RenderInvalidationReason.PlaybackChanged && _frameScheduler.IsRunning)
        {
            RefreshRenderScene();
            return;
        }

        _renderInvalidationService.RequestInvalidation(reason);
    }

    private void RefreshRenderScene()
    {
        RenderScene = _sceneComposer.Compose(_workspaceSessionService.State, _hostLayout, _frameVersion);
    }

    private void SynchronizeControlsFromViewportState(
        ViewportState viewportState,
        bool hasActiveSession)
    {
        _isSynchronizingControls = true;

        try
        {
            HasActiveSession = hasActiveSession;
            IsGridVisible = viewportState.ShowGrid;
            IsOriginVisible = viewportState.ShowOrigin;
            IsBonesVisible = viewportState.ShowBones;
            IsBoundsVisible = viewportState.ShowBounds;
            SelectedBackgroundStyle = viewportState.BackgroundStyle;
            OnPropertyChanged(nameof(ZoomSummaryText));
        }
        finally
        {
            _isSynchronizingControls = false;
        }
    }

    private bool TryAutoFitCurrentSessionIfDefault()
    {
        return TryAutoFitSessionIfNeeded(_lastWorkspaceState, _workspaceSessionService.State);
    }

    private bool TryAutoFitSessionIfNeeded(
        WorkspaceState previousState,
        WorkspaceState currentState)
    {
        SpineProjectSession? currentSession = currentState.CurrentSession;

        if (currentSession is null || !_hostLayout.HasSurface || !IsViewportAtDefault(currentSession.Viewport))
        {
            return false;
        }

        bool isNewSession = previousState.CurrentSession?.SessionId != currentSession.SessionId;
        bool needsFirstLayoutFit = previousState.CurrentSession?.SessionId == currentSession.SessionId &&
                                   previousState.CurrentSession?.Viewport == currentSession.Viewport;

        if (!isNewSession && !needsFirstLayoutFit)
        {
            return false;
        }

        ViewportRenderScene previewScene = _sceneComposer.Compose(currentState, _hostLayout, _frameVersion);
        if (previewScene.ContentBounds is null)
        {
            return false;
        }

        ViewportState fittedViewportState = _cameraService.FitToView(
            currentSession.Viewport,
            _hostLayout,
            previewScene.ContentBounds);

        if (fittedViewportState == currentSession.Viewport)
        {
            return false;
        }

        ApplyViewportState(fittedViewportState);
        return true;
    }

    private void UpdateOverlayVisibility(
        bool showGrid,
        bool showOrigin,
        bool showBones,
        bool showBounds)
    {
        if (_isSynchronizingControls || !HasActiveSession)
        {
            return;
        }

        ApplyViewportState(
            _currentViewportState with
            {
                ShowGrid = showGrid,
                ShowOrigin = showOrigin,
                ShowBones = showBones,
                ShowBounds = showBounds,
            });
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

    private static bool IsViewportAtDefault(ViewportState viewportState)
    {
        return viewportState.Zoom == 1.0 &&
               viewportState.OffsetX == 0.0 &&
               viewportState.OffsetY == 0.0;
    }
}
