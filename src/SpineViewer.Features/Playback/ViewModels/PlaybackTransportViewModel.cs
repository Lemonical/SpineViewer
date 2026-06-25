using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Contracts;
using SpineViewer.Features.Playback.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Playback.ViewModels;

/// <summary>
/// Presents the playback transport state and routes transport actions through immutable playback-state transitions.
/// </summary>
public sealed partial class PlaybackTransportViewModel : ObservableObject, IDisposable
{
    private readonly IWorkspaceSessionService _workspaceSessionService;
    private readonly IPlaybackStateService _playbackStateService;
    private readonly IRenderInvalidationService _renderInvalidationService;
    private readonly IViewportFrameScheduler _frameScheduler;
    private readonly PlaybackSpeedOption[] _speedOptions =
    [
        new PlaybackSpeedOption(0.25, "0.25x"),
        new PlaybackSpeedOption(0.50, "0.50x"),
        new PlaybackSpeedOption(0.75, "0.75x"),
        new PlaybackSpeedOption(1.00, "1.00x"),
        new PlaybackSpeedOption(1.25, "1.25x"),
        new PlaybackSpeedOption(1.50, "1.50x"),
        new PlaybackSpeedOption(2.00, "2.00x"),
    ];
    private PlaybackState _currentPlaybackState = new();
    private bool _isSynchronizingControls;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackTransportViewModel"/> class.
    /// </summary>
    /// <param name="workspaceSessionService">The workspace session service that owns the current session.</param>
    /// <param name="playbackStateService">The service that applies playback-state transitions.</param>
    /// <param name="renderInvalidationService">The render invalidation service used for frame-tick playback updates.</param>
    /// <param name="frameScheduler">The viewport frame scheduler that defines the active frame cadence.</param>
    public PlaybackTransportViewModel(
        IWorkspaceSessionService workspaceSessionService,
        IPlaybackStateService playbackStateService,
        IRenderInvalidationService renderInvalidationService,
        IViewportFrameScheduler frameScheduler)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
        _playbackStateService = playbackStateService ?? throw new ArgumentNullException(nameof(playbackStateService));
        _renderInvalidationService =
            renderInvalidationService ?? throw new ArgumentNullException(nameof(renderInvalidationService));
        _frameScheduler = frameScheduler ?? throw new ArgumentNullException(nameof(frameScheduler));

        ApplyState(_workspaceSessionService.State);
        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
        _renderInvalidationService.RenderInvalidated += OnRenderInvalidated;
    }

    /// <summary>
    /// Gets the available playback speed options.
    /// </summary>
    public IReadOnlyList<PlaybackSpeedOption> SpeedOptions => _speedOptions;

    /// <summary>
    /// Gets the visible transport status label.
    /// </summary>
    public string StatusText => _currentPlaybackState.Status.ToString();

    /// <summary>
    /// Gets the visible current-time label.
    /// </summary>
    public string CurrentTimeText => FormatTimestamp(_currentPlaybackState.CurrentTime);

    /// <summary>
    /// Gets the visible duration label.
    /// </summary>
    public string DurationText => FormatTimestamp(_currentPlaybackState.Duration);

    /// <summary>
    /// Gets the combined current-time and duration text.
    /// </summary>
    public string TimeSummaryText => $"{CurrentTimeText} / {DurationText}";

    /// <summary>
    /// Gets the fixed shortcut hint shown beside the transport controls.
    /// </summary>
    public string ShortcutHintText => "Space plays or pauses. S stops. R restarts. Right Arrow steps. L toggles looping.";

    /// <summary>
    /// Gets the slider maximum in seconds.
    /// </summary>
    public double TimelineMaximumSeconds => _currentPlaybackState.Duration.TotalSeconds;

    /// <summary>
    /// Gets a value indicating whether a session is currently active.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestartCommand))]
    [NotifyCanExecuteChangedFor(nameof(FrameStepCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePlayPauseCommand))]
    [NotifyCanExecuteChangedFor(nameof(ToggleLoopCommand))]
    private bool hasActiveSession;

    /// <summary>
    /// Gets a value indicating whether looping is enabled.
    /// </summary>
    [ObservableProperty]
    private bool isLooping;

    /// <summary>
    /// Gets or sets the current scrubber position in seconds.
    /// </summary>
    [ObservableProperty]
    private double timelinePositionSeconds;

    /// <summary>
    /// Gets the currently selected playback speed option.
    /// </summary>
    [ObservableProperty]
    private PlaybackSpeedOption? selectedSpeedOption;

    /// <summary>
    /// Releases the event subscriptions held by the transport view model.
    /// </summary>
    public void Dispose()
    {
        _workspaceSessionService.StateChanged -= OnWorkspaceStateChanged;
        _renderInvalidationService.RenderInvalidated -= OnRenderInvalidated;
    }

    partial void OnIsLoopingChanged(bool value)
    {
        if (_isSynchronizingControls || !HasActiveSession)
        {
            return;
        }

        ApplyPlaybackState(_playbackStateService.SetLooping(_currentPlaybackState, value));
    }

    partial void OnSelectedSpeedOptionChanged(PlaybackSpeedOption? value)
    {
        if (_isSynchronizingControls || !HasActiveSession || value is null)
        {
            return;
        }

        ApplyPlaybackState(_playbackStateService.SetSpeed(_currentPlaybackState, value.Multiplier));
    }

    partial void OnTimelinePositionSecondsChanged(double value)
    {
        if (_isSynchronizingControls || !HasActiveSession)
        {
            return;
        }

        ApplyPlaybackState(_playbackStateService.ScrubTo(_currentPlaybackState, TimeSpan.FromSeconds(value)));
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        ApplyPlaybackState(_playbackStateService.Play(_currentPlaybackState));
    }

    [RelayCommand(CanExecute = nameof(CanPause))]
    private void Pause()
    {
        ApplyPlaybackState(_playbackStateService.Pause(_currentPlaybackState));
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        ApplyPlaybackState(_playbackStateService.Stop(_currentPlaybackState));
    }

    [RelayCommand(CanExecute = nameof(CanRestart))]
    private void Restart()
    {
        ApplyPlaybackState(_playbackStateService.Restart(_currentPlaybackState));
    }

    [RelayCommand(CanExecute = nameof(CanFrameStep))]
    private void FrameStep()
    {
        ApplyPlaybackState(_playbackStateService.StepForward(_currentPlaybackState));
    }

    [RelayCommand(CanExecute = nameof(CanTogglePlayPause))]
    private void TogglePlayPause()
    {
        ApplyPlaybackState(
            _currentPlaybackState.IsPlaying
                ? _playbackStateService.Pause(_currentPlaybackState)
                : _playbackStateService.Play(_currentPlaybackState));
    }

    [RelayCommand(CanExecute = nameof(CanToggleLoop))]
    private void ToggleLoop()
    {
        IsLooping = !IsLooping;
    }

    private void ApplyPlaybackState(PlaybackState playbackState)
    {
        if (!HasActiveSession || playbackState == _currentPlaybackState)
        {
            return;
        }

        _workspaceSessionService.UpdatePlaybackState(playbackState);
    }

    private void ApplyState(WorkspaceState workspaceState)
    {
        _currentPlaybackState = workspaceState.CurrentSession?.Playback ?? new PlaybackState();

        _isSynchronizingControls = true;

        try
        {
            HasActiveSession = workspaceState.CurrentSession is not null;
            IsLooping = _currentPlaybackState.IsLooping;
            TimelinePositionSeconds = _currentPlaybackState.CurrentTime.TotalSeconds;
            SelectedSpeedOption = SelectSpeedOption(_currentPlaybackState.Speed);

            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CurrentTimeText));
            OnPropertyChanged(nameof(DurationText));
            OnPropertyChanged(nameof(TimeSummaryText));
            OnPropertyChanged(nameof(TimelineMaximumSeconds));
        }
        finally
        {
            _isSynchronizingControls = false;
        }

        NotifyCommandAvailabilityChanged();
    }

    private bool CanFrameStep()
    {
        return HasActiveSession;
    }

    private bool CanPause()
    {
        return HasActiveSession && _currentPlaybackState.IsPlaying;
    }

    private bool CanPlay()
    {
        return HasActiveSession && !_currentPlaybackState.IsPlaying;
    }

    private bool CanRestart()
    {
        return HasActiveSession;
    }

    private bool CanStop()
    {
        return HasActiveSession &&
               (!_currentPlaybackState.IsStopped || _currentPlaybackState.CurrentTime > TimeSpan.Zero);
    }

    private bool CanToggleLoop()
    {
        return HasActiveSession;
    }

    private bool CanTogglePlayPause()
    {
        return HasActiveSession;
    }

    private static string FormatTimestamp(TimeSpan value)
    {
        return $"{(int)value.TotalMinutes:00}:{value.Seconds:00}.{value.Milliseconds / 10:00}";
    }

    private void NotifyCommandAvailabilityChanged()
    {
        PlayCommand.NotifyCanExecuteChanged();
        PauseCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        RestartCommand.NotifyCanExecuteChanged();
        FrameStepCommand.NotifyCanExecuteChanged();
        TogglePlayPauseCommand.NotifyCanExecuteChanged();
        ToggleLoopCommand.NotifyCanExecuteChanged();
    }

    private void OnRenderInvalidated(object? sender, RenderInvalidatedEventArgs e)
    {
        if (e.Reason != RenderInvalidationReason.FrameTick || !HasActiveSession || !_currentPlaybackState.IsPlaying)
        {
            return;
        }

        ApplyPlaybackState(_playbackStateService.Advance(_currentPlaybackState, _frameScheduler.FrameInterval));
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        ApplyState(_workspaceSessionService.State);
    }

    private PlaybackSpeedOption SelectSpeedOption(double speed)
    {
        PlaybackSpeedOption? exactOption = _speedOptions.FirstOrDefault(
            option => Math.Abs(option.Multiplier - speed) < 0.001);

        if (exactOption is not null)
        {
            return exactOption;
        }

        return _speedOptions
            .OrderBy(option => Math.Abs(option.Multiplier - speed))
            .First();
    }
}
