using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Playback.Contracts;
using SpineViewer.Features.Playback.Models;

namespace SpineViewer.Features.Playback.ViewModels;

/// <summary>
/// Presents an immediate-apply multi-track editing workflow for the current playback state.
/// </summary>
public sealed partial class AnimationTrackEditorViewModel : ObservableObject, IDisposable
{
    private readonly IPlaybackTrackEditorService _playbackTrackEditorService;
    private readonly IViewerSettingsService _viewerSettingsService;
    private readonly IWorkspaceSessionService _workspaceSessionService;
    private SpineProjectSession? _currentSession;
    private bool _isSynchronizingSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationTrackEditorViewModel"/> class.
    /// </summary>
    public AnimationTrackEditorViewModel(
        IWorkspaceSessionService workspaceSessionService,
        IPlaybackTrackEditorService playbackTrackEditorService,
        IViewerSettingsService viewerSettingsService)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
        _playbackTrackEditorService =
            playbackTrackEditorService ?? throw new ArgumentNullException(nameof(playbackTrackEditorService));
        _viewerSettingsService = viewerSettingsService ?? throw new ArgumentNullException(nameof(viewerSettingsService));

        ApplyState(_workspaceSessionService.State);
        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Gets a value indicating whether a session is currently available for track editing.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTrackCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveSelectedTrackDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveSelectedTrackUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedTrackCommand))]
    private bool hasActiveSession;

    /// <summary>
    /// Gets the current track stack.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTracks))]
    [NotifyPropertyChangedFor(nameof(HasNoTracks))]
    private IReadOnlyList<AnimationTrackState> tracks = Array.Empty<AnimationTrackState>();

    /// <summary>
    /// Gets the available animation names for new or reassigned tracks.
    /// </summary>
    [ObservableProperty]
    private IReadOnlyList<string> availableAnimationNames = Array.Empty<string>();

    /// <summary>
    /// Gets the current validation issues.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ValidationSummaryText))]
    private IReadOnlyList<PlaybackTrackValidationIssue> validationIssues = Array.Empty<PlaybackTrackValidationIssue>();

    /// <summary>
    /// Gets the currently selected track.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoSelectedTrack))]
    [NotifyPropertyChangedFor(nameof(HasSelectedTrack))]
    [NotifyCanExecuteChangedFor(nameof(MoveSelectedTrackDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveSelectedTrackUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveSelectedTrackCommand))]
    private AnimationTrackState? selectedTrack;

    /// <summary>
    /// Gets the animation name selected for adding a new track.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTrackCommand))]
    private string? selectedNewTrackAnimationName;

    /// <summary>
    /// Gets the editable animation name for the selected track.
    /// </summary>
    [ObservableProperty]
    private string? selectedTrackAnimationName;

    /// <summary>
    /// Gets a value indicating whether the selected track is enabled.
    /// </summary>
    [ObservableProperty]
    private bool selectedTrackIsEnabled;

    /// <summary>
    /// Gets a value indicating whether the selected track is looping.
    /// </summary>
    [ObservableProperty]
    private bool selectedTrackIsLooping;

    /// <summary>
    /// Gets the editable time scale for the selected track.
    /// </summary>
    [ObservableProperty]
    private double selectedTrackTimeScale = 1.0;

    /// <summary>
    /// Gets the editable mix duration, in seconds, for the selected track.
    /// </summary>
    [ObservableProperty]
    private double selectedTrackMixDurationSeconds;

    /// <summary>
    /// Releases the workspace-state subscription held by the track editor.
    /// </summary>
    public void Dispose()
    {
        _workspaceSessionService.StateChanged -= OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Gets a value indicating whether any tracks are currently configured.
    /// </summary>
    public bool HasTracks => Tracks.Count > 0;

    /// <summary>
    /// Gets a value indicating whether no tracks are currently configured.
    /// </summary>
    public bool HasNoTracks => !HasTracks;

    /// <summary>
    /// Gets a value indicating whether a track is currently selected.
    /// </summary>
    public bool HasSelectedTrack => SelectedTrack is not null;

    /// <summary>
    /// Gets a value indicating whether no track is currently selected.
    /// </summary>
    public bool HasNoSelectedTrack => !HasSelectedTrack;

    /// <summary>
    /// Gets the summary shown for the current validation state.
    /// </summary>
    public string ValidationSummaryText => ValidationIssues.Count == 0
        ? "Track configuration looks valid."
        : $"{ValidationIssues.Count} track configuration issue(s)";

    /// <summary>
    /// Gets the guidance text shown when no tracks are configured.
    /// </summary>
    public string EmptyStateText => HasActiveSession
        ? "Choose an animation and add a track to start layering playback."
        : "Open a Spine project to edit animation tracks.";

    partial void OnSelectedTrackChanged(AnimationTrackState? value)
    {
        SynchronizeSelectedTrackEditor(value);
    }

    partial void OnSelectedTrackAnimationNameChanged(string? value)
    {
        if (_isSynchronizingSelection || SelectedTrack is null || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.SetTrackAnimation(
                _currentSession!.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                value));
    }

    partial void OnSelectedTrackIsEnabledChanged(bool value)
    {
        if (_isSynchronizingSelection || SelectedTrack is null)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.SetTrackEnabled(
                _currentSession!.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                value));
    }

    partial void OnSelectedTrackIsLoopingChanged(bool value)
    {
        if (_isSynchronizingSelection || SelectedTrack is null)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.SetTrackLooping(
                _currentSession!.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                value));
    }

    partial void OnSelectedTrackMixDurationSecondsChanged(double value)
    {
        if (_isSynchronizingSelection || SelectedTrack is null || value < 0)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.SetTrackMixDuration(
                _currentSession!.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                TimeSpan.FromSeconds(value)));
    }

    partial void OnSelectedTrackTimeScaleChanged(double value)
    {
        if (_isSynchronizingSelection || SelectedTrack is null || value <= 0)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.SetTrackTimeScale(
                _currentSession!.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                value));
    }

    [RelayCommand(CanExecute = nameof(CanAddTrack))]
    private void AddTrack()
    {
        if (_currentSession is null || string.IsNullOrWhiteSpace(SelectedNewTrackAnimationName))
        {
            return;
        }

        ViewerSettings settings = _viewerSettingsService.CurrentSettings;
        ApplyPlaybackUpdate(
            _playbackTrackEditorService.AddTrack(
                _currentSession.Playback,
                _currentSession.Inspection,
                SelectedNewTrackAnimationName,
                settings.LoopPlaybackByDefault,
                settings.DefaultTrackTimeScale,
                settings.DefaultTrackMixDuration));
    }

    [RelayCommand(CanExecute = nameof(CanMoveSelectedTrackDown))]
    private void MoveSelectedTrackDown()
    {
        if (_currentSession is null || SelectedTrack is null)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.MoveTrack(
                _currentSession.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                SelectedTrack.TrackIndex + 1));
    }

    [RelayCommand(CanExecute = nameof(CanMoveSelectedTrackUp))]
    private void MoveSelectedTrackUp()
    {
        if (_currentSession is null || SelectedTrack is null)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.MoveTrack(
                _currentSession.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId,
                SelectedTrack.TrackIndex - 1));
    }

    [RelayCommand(CanExecute = nameof(CanRemoveSelectedTrack))]
    private void RemoveSelectedTrack()
    {
        if (_currentSession is null || SelectedTrack is null)
        {
            return;
        }

        ApplyPlaybackUpdate(
            _playbackTrackEditorService.RemoveTrack(
                _currentSession.Playback,
                _currentSession.Inspection,
                SelectedTrack.TrackId));
    }

    private void ApplyPlaybackUpdate(PlaybackState playbackState)
    {
        if (!HasActiveSession)
        {
            return;
        }

        _workspaceSessionService.UpdatePlaybackState(playbackState);
    }

    private void ApplyState(WorkspaceState workspaceState)
    {
        _currentSession = workspaceState.CurrentSession;
        HasActiveSession = _currentSession is not null;
        Tracks = _currentSession?.Playback.Tracks ?? Array.Empty<AnimationTrackState>();
        AvailableAnimationNames = _currentSession?.Inspection.Animations
            .Select(static animation => animation.Name)
            .ToArray() ?? Array.Empty<string>();
        SelectedNewTrackAnimationName = ChooseNewTrackAnimationName();
        ValidationIssues = _currentSession is null
            ? Array.Empty<PlaybackTrackValidationIssue>()
            : _playbackTrackEditorService.Validate(
                _currentSession.Playback,
                _currentSession.Inspection,
                _currentSession.Runtime);

        SelectedTrack = ChooseSelectedTrack();
    }

    private bool CanAddTrack()
    {
        return HasActiveSession && !string.IsNullOrWhiteSpace(SelectedNewTrackAnimationName);
    }

    private bool CanMoveSelectedTrackDown()
    {
        return SelectedTrack is not null && SelectedTrack.TrackIndex < Tracks.Count - 1;
    }

    private bool CanMoveSelectedTrackUp()
    {
        return SelectedTrack is not null && SelectedTrack.TrackIndex > 0;
    }

    private bool CanRemoveSelectedTrack()
    {
        return SelectedTrack is not null;
    }

    private AnimationTrackState? ChooseSelectedTrack()
    {
        if (Tracks.Count == 0)
        {
            return null;
        }

        if (SelectedTrack is null)
        {
            return Tracks[0];
        }

        return Tracks.FirstOrDefault(track => track.TrackId == SelectedTrack.TrackId) ?? Tracks[0];
    }

    private string? ChooseNewTrackAnimationName()
    {
        if (AvailableAnimationNames.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(SelectedNewTrackAnimationName) &&
            AvailableAnimationNames.Contains(SelectedNewTrackAnimationName, StringComparer.OrdinalIgnoreCase))
        {
            return SelectedNewTrackAnimationName;
        }

        return AvailableAnimationNames[0];
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        ApplyState(_workspaceSessionService.State);
    }

    private void SynchronizeSelectedTrackEditor(AnimationTrackState? track)
    {
        _isSynchronizingSelection = true;

        try
        {
            SelectedTrackAnimationName = track?.AnimationName;
            SelectedTrackIsEnabled = track?.IsEnabled ?? false;
            SelectedTrackIsLooping = track?.IsLooping ?? false;
            SelectedTrackTimeScale = track?.TimeScale ?? 1.0;
            SelectedTrackMixDurationSeconds = track?.MixDuration.TotalSeconds ?? 0.0;
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }
}
