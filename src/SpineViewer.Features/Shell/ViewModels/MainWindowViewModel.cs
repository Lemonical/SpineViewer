using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Diagnostics.ViewModels;
using SpineViewer.Features.Inspector.ViewModels;
using SpineViewer.Features.Playback.ViewModels;
using SpineViewer.Features.Settings.ViewModels;
using SpineViewer.Features.Shell.Models;
using SpineViewer.Features.Viewport.ViewModels;

namespace SpineViewer.Features.Shell.ViewModels;

/// <summary>
/// Presents the main shell state for the rewrite-side workspace host.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private const double CompactLayoutThreshold = 1180.0;
    private readonly IReadOnlyList<RuntimeSelectionOption> _runtimeOptions;
    private readonly string _runtimeSupportSummaryText;
    private readonly IWorkspaceSessionService _workspaceSessionService;
    private bool _isSynchronizingRuntimeSelection;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="workspaceSessionService">The workspace service that drives shell session state.</param>
    /// <param name="runtimeCatalog">The runtime catalog that describes the registered runtime adapters.</param>
    /// <param name="viewport">The viewport view model shown inside the shell.</param>
    /// <param name="playback">The playback transport view model shown inside the shell.</param>
    /// <param name="trackEditor">The track editor view model hosted by the shell.</param>
    /// <param name="inspector">The inspector view model hosted by the shell.</param>
    /// <param name="diagnosticsPanel">The diagnostics panel view model hosted by the shell.</param>
    /// <param name="settings">The settings view model hosted by the shell.</param>
    public MainWindowViewModel(
        IWorkspaceSessionService workspaceSessionService,
        ISpineRuntimeCatalog runtimeCatalog,
        ViewportViewModel viewport,
        PlaybackTransportViewModel playback,
        AnimationTrackEditorViewModel trackEditor,
        AssetInspectorViewModel inspector,
        DiagnosticsPanelViewModel diagnosticsPanel,
        ViewerSettingsViewModel settings)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
        ArgumentNullException.ThrowIfNull(runtimeCatalog);
        Viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
        Playback = playback ?? throw new ArgumentNullException(nameof(playback));
        TrackEditor = trackEditor ?? throw new ArgumentNullException(nameof(trackEditor));
        Inspector = inspector ?? throw new ArgumentNullException(nameof(inspector));
        DiagnosticsPanel = diagnosticsPanel ?? throw new ArgumentNullException(nameof(diagnosticsPanel));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _runtimeOptions = BuildRuntimeOptions(runtimeCatalog.GetAvailableRuntimes());
        _runtimeSupportSummaryText = BuildRuntimeSupportSummary(runtimeCatalog.GetAvailableRuntimes());
        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
        ApplyState(_workspaceSessionService.State);
    }

    /// <summary>
    /// Gets the shell window title.
    /// </summary>
    public string Title => CurrentSession is null
        ? "SpineViewer"
        : $"{CurrentSession.Project.DisplayName} - SpineViewer";

    /// <summary>
    /// Gets the viewport view model hosted by the shell.
    /// </summary>
    public ViewportViewModel Viewport { get; }

    /// <summary>
    /// Gets the playback transport view model hosted by the shell.
    /// </summary>
    public PlaybackTransportViewModel Playback { get; }

    /// <summary>
    /// Gets the track editor view model hosted by the shell.
    /// </summary>
    public AnimationTrackEditorViewModel TrackEditor { get; }

    /// <summary>
    /// Gets the inspector view model hosted by the shell.
    /// </summary>
    public AssetInspectorViewModel Inspector { get; }

    /// <summary>
    /// Gets the diagnostics panel view model hosted by the shell.
    /// </summary>
    public DiagnosticsPanelViewModel DiagnosticsPanel { get; }

    /// <summary>
    /// Gets the settings view model hosted by the shell.
    /// </summary>
    public ViewerSettingsViewModel Settings { get; }

    /// <summary>
    /// Gets the current session shown by the shell.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(HasActiveSession))]
    [NotifyPropertyChangedFor(nameof(HasNoActiveSession))]
    [NotifyPropertyChangedFor(nameof(CurrentProjectName))]
    [NotifyPropertyChangedFor(nameof(CurrentAssetSummary))]
    [NotifyPropertyChangedFor(nameof(CurrentRuntimeSummary))]
    [NotifyPropertyChangedFor(nameof(CurrentVersionSummary))]
    [NotifyPropertyChangedFor(nameof(CurrentTextureSummary))]
    [NotifyPropertyChangedFor(nameof(CurrentPlaybackSummary))]
    [NotifyPropertyChangedFor(nameof(CurrentViewportSummary))]
    [NotifyPropertyChangedFor(nameof(CurrentSkeletonPath))]
    [NotifyPropertyChangedFor(nameof(CurrentAtlasPath))]
    [NotifyPropertyChangedFor(nameof(HeaderModelName))]
    [NotifyPropertyChangedFor(nameof(HeaderVersionText))]
    [NotifyPropertyChangedFor(nameof(HasHeaderVersion))]
    [NotifyPropertyChangedFor(nameof(WorkspaceCardTitle))]
    [NotifyPropertyChangedFor(nameof(RuntimeShortName))]
    [NotifyPropertyChangedFor(nameof(IsRuntimeExactMatch))]
    [NotifyPropertyChangedFor(nameof(RuntimeSelectionStatusText))]
    [NotifyPropertyChangedFor(nameof(SessionDetailsTitle))]
    [NotifyPropertyChangedFor(nameof(ShowLandingState))]
    [NotifyPropertyChangedFor(nameof(ShowLoadingState))]
    [NotifyPropertyChangedFor(nameof(ShowFirstRunState))]
    [NotifyPropertyChangedFor(nameof(ShowLoadFailureState))]
    [NotifyPropertyChangedFor(nameof(WorkspaceExperienceTitle))]
    [NotifyPropertyChangedFor(nameof(WorkspaceExperienceDescription))]
    [NotifyPropertyChangedFor(nameof(CanRetryLastOpen))]
    [NotifyPropertyChangedFor(nameof(RecoveryPrimaryActionText))]
    [NotifyCanExecuteChangedFor(nameof(ReloadSessionCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseSessionCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetryLastOpenCommand))]
    private SpineProjectSession? currentSession;

    /// <summary>
    /// Gets the current recent-file list.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecentFiles))]
    [NotifyPropertyChangedFor(nameof(HasNoRecentFiles))]
    [NotifyPropertyChangedFor(nameof(RecentFilesSummaryText))]
    [NotifyCanExecuteChangedFor(nameof(ClearRecentProjectsCommand))]
    private IReadOnlyList<SpineProjectReference> recentFiles = Array.Empty<SpineProjectReference>();

    /// <summary>
    /// Gets the currently selected recent project.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenSelectedRecentProjectCommand))]
    private SpineProjectReference? selectedRecentProject;

    /// <summary>
    /// Gets the current diagnostics shown in the shell.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiagnostics))]
    [NotifyPropertyChangedFor(nameof(HasNoDiagnostics))]
    [NotifyPropertyChangedFor(nameof(DiagnosticSummaryText))]
    [NotifyPropertyChangedFor(nameof(ShowLandingState))]
    [NotifyPropertyChangedFor(nameof(ShowFirstRunState))]
    [NotifyPropertyChangedFor(nameof(ShowLoadFailureState))]
    [NotifyPropertyChangedFor(nameof(WorkspaceExperienceTitle))]
    [NotifyPropertyChangedFor(nameof(WorkspaceExperienceDescription))]
    [NotifyPropertyChangedFor(nameof(CanRetryLastOpen))]
    [NotifyPropertyChangedFor(nameof(RecoveryPrimaryActionText))]
    [NotifyPropertyChangedFor(nameof(HasBlockingDiagnostics))]
    private IReadOnlyList<ViewerDiagnostic> diagnostics = Array.Empty<ViewerDiagnostic>();

    /// <summary>
    /// Gets the currently selected diagnostic.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedDiagnostic))]
    [NotifyPropertyChangedFor(nameof(SelectedDiagnosticTitle))]
    [NotifyPropertyChangedFor(nameof(SelectedDiagnosticMessage))]
    [NotifyPropertyChangedFor(nameof(SelectedDiagnosticSource))]
    [NotifyPropertyChangedFor(nameof(SelectedDiagnosticSuggestedAction))]
    [NotifyPropertyChangedFor(nameof(HasSelectedDiagnosticSuggestedAction))]
    private ViewerDiagnostic? selectedDiagnostic;

    /// <summary>
    /// Gets the status line shown at the bottom of the shell.
    /// </summary>
    [ObservableProperty]
    private string statusText = "Ready.";

    /// <summary>
    /// Gets a value indicating whether the workspace is currently running an operation.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActivitySummary))]
    [NotifyPropertyChangedFor(nameof(CanStartOpenFlow))]
    [NotifyPropertyChangedFor(nameof(CanChangeRuntimeSelection))]
    [NotifyPropertyChangedFor(nameof(ShowLandingState))]
    [NotifyPropertyChangedFor(nameof(ShowLoadingState))]
    [NotifyPropertyChangedFor(nameof(ShowFirstRunState))]
    [NotifyPropertyChangedFor(nameof(ShowLoadFailureState))]
    [NotifyCanExecuteChangedFor(nameof(ReloadSessionCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseSessionCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenSelectedRecentProjectCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearRecentProjectsCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveRecentProjectCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetryLastOpenCommand))]
    private bool isBusy;

    /// <summary>
    /// Gets a value indicating whether the shell is using its compact layout.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LayoutModeLabel))]
    private bool isCompactLayout;

    /// <summary>
    /// Gets a value indicating whether the workspace pane is visible.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WorkspacePaneButtonText))]
    private bool isWorkspacePaneVisible = true;

    /// <summary>
    /// Gets a value indicating whether the details pane is visible.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailsPaneButtonText))]
    private bool isDetailsPaneVisible = true;

    /// <summary>
    /// Gets the currently selected runtime option for new loads and reloads.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedRuntimeOptionDescription))]
    [NotifyPropertyChangedFor(nameof(RuntimeSelectionStatusText))]
    private RuntimeSelectionOption? selectedRuntimeOption;

    /// <summary>
    /// Gets a value indicating whether a session is currently open.
    /// </summary>
    public bool HasActiveSession => CurrentSession is not null;

    /// <summary>
    /// Gets a value indicating whether no session is currently open.
    /// </summary>
    public bool HasNoActiveSession => !HasActiveSession;

    /// <summary>
    /// Gets a value indicating whether recent-file entries are available.
    /// </summary>
    public bool HasRecentFiles => RecentFiles.Count > 0;

    /// <summary>
    /// Gets a value indicating whether no recent-file entries are available.
    /// </summary>
    public bool HasNoRecentFiles => !HasRecentFiles;

    /// <summary>
    /// Gets a value indicating whether diagnostics are available.
    /// </summary>
    public bool HasDiagnostics => Diagnostics.Count > 0;

    /// <summary>
    /// Gets a value indicating whether no diagnostics are available.
    /// </summary>
    public bool HasNoDiagnostics => !HasDiagnostics;

    /// <summary>
    /// Gets a value indicating whether at least one error diagnostic is active.
    /// </summary>
    public bool HasBlockingDiagnostics => Diagnostics.Any(static diagnostic => diagnostic.Severity == ViewerDiagnosticSeverity.Error);

    /// <summary>
    /// Gets a value indicating whether a diagnostic is currently selected.
    /// </summary>
    public bool HasSelectedDiagnostic => SelectedDiagnostic is not null;

    /// <summary>
    /// Gets a value indicating whether the selected diagnostic includes recovery guidance.
    /// </summary>
    public bool HasSelectedDiagnosticSuggestedAction =>
        !string.IsNullOrWhiteSpace(SelectedDiagnostic?.SuggestedAction);

    /// <summary>
    /// Gets the headline used for the current project area.
    /// </summary>
    public string CurrentProjectName => CurrentSession?.Project.DisplayName ?? "Open a Spine Model";

    /// <summary>
    /// Gets the active asset pairing summary.
    /// </summary>
    public string CurrentAssetSummary => CurrentSession is null
        ? "Choose a .json, .skel, .bytes, or .atlas file to begin."
        : $"{Path.GetFileName(CurrentSession.AssetFileSet.SkeletonPath)} + {Path.GetFileName(CurrentSession.AssetFileSet.AtlasPath)}";

    /// <summary>
    /// Gets the active runtime summary.
    /// </summary>
    public string CurrentRuntimeSummary => CurrentSession?.Runtime.DisplayName ??
                                           _runtimeSupportSummaryText;

    /// <summary>
    /// Gets the version-detection summary for the current session.
    /// </summary>
    public string CurrentVersionSummary => CurrentSession switch
    {
        null => "The viewer will explain whether it chose an exact runtime match or a compatible fallback.",
        { VersionMatch.DetectedExportVersion: null } => "The viewer could not determine an exact export version and will explain the fallback it used.",
        { VersionMatch.IsExactMatch: true } => $"Detected export {CurrentSession.VersionMatch.DetectedExportVersion} with an exact runtime match.",
        _ => $"Detected export {CurrentSession.VersionMatch.DetectedExportVersion} with a compatible runtime selection.",
    };

    /// <summary>
    /// Gets the texture-resolution summary for the current session.
    /// </summary>
    public string CurrentTextureSummary => CurrentSession is null
        ? "Atlas texture dependencies will be checked automatically."
        : $"{CurrentSession.AssetFileSet.DependentTexturePaths.Count} texture asset(s) resolved.";

    /// <summary>
    /// Gets the playback summary for the current session.
    /// </summary>
    public string CurrentPlaybackSummary => CurrentSession is null
        ? "Open a model to enable playback controls."
        : $"{CurrentSession.Playback.Status} | {CurrentSession.Playback.Tracks.Count} track(s) | {(CurrentSession.Playback.IsLooping ? "Loop on" : "Loop off")} | {CurrentSession.Playback.Speed:0.00}x | {CurrentSession.Playback.CurrentTime:mm\\:ss\\.ff} / {CurrentSession.Playback.Duration:mm\\:ss\\.ff}";

    /// <summary>
    /// Gets the viewport summary for the current session.
    /// </summary>
    public string CurrentViewportSummary => CurrentSession is null
        ? "Drop a Spine model here to preview it."
        : $"Zoom {CurrentSession.Viewport.Zoom:P0} | {CurrentSession.Viewport.BackgroundStyle} | Debug {CountActiveViewportOverlays(CurrentSession.Viewport)} active";

    /// <summary>
    /// Gets the skeleton path for the current session.
    /// </summary>
    public string CurrentSkeletonPath => CurrentSession?.AssetFileSet.SkeletonPath ??
                                         "Choose a skeleton file or start from a matching atlas.";

    /// <summary>
    /// Gets the atlas path for the current session.
    /// </summary>
    public string CurrentAtlasPath => CurrentSession?.AssetFileSet.AtlasPath ??
                                      "Choose an atlas file or let the viewer auto-pair a matching companion.";

    /// <summary>
    /// Gets the current diagnostic counter summary.
    /// </summary>
    public string DiagnosticSummaryText => Diagnostics.Count == 0
        ? "No diagnostics"
        : $"{Diagnostics.Count} diagnostic(s)";

    /// <summary>
    /// Gets the current recent-file counter summary.
    /// </summary>
    public string RecentFilesSummaryText => RecentFiles.Count == 0
        ? "No recent projects"
        : $"{RecentFiles.Count} recent project(s)";

    /// <summary>
    /// Gets the current shell activity summary.
    /// </summary>
    public string ActivitySummary => IsBusy ? "Working" : "Idle";

    /// <summary>
    /// Gets a value indicating whether the shell can start a new open flow.
    /// </summary>
    public bool CanStartOpenFlow => !IsBusy;

    /// <summary>
    /// Gets the current responsive layout label.
    /// </summary>
    public string LayoutModeLabel => IsCompactLayout ? "Compact layout" : "Docked layout";

    /// <summary>
    /// Gets the section title shown for the current session card.
    /// </summary>
    public string SessionDetailsTitle => HasActiveSession ? "Active Session" : "Open Guidance";

    /// <summary>
    /// Gets the skeleton file name shown beside the brand in the shell header.
    /// </summary>
    public string HeaderModelName => CurrentSession is null
        ? "No model loaded"
        : Path.GetFileName(CurrentSession.AssetFileSet.SkeletonPath);

    /// <summary>
    /// Gets the short export/runtime version badge shown in the shell header.
    /// </summary>
    public string HeaderVersionText => CurrentSession?.VersionMatch.DetectedExportVersion
        ?? CurrentSession?.Runtime.SupportedExportRange
        ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the header version badge should be shown.
    /// </summary>
    public bool HasHeaderVersion => CurrentSession is not null && !string.IsNullOrWhiteSpace(HeaderVersionText);

    /// <summary>
    /// Gets the model base name used as the workspace card heading.
    /// </summary>
    public string WorkspaceCardTitle => CurrentSession?.Project.DisplayName ?? string.Empty;

    /// <summary>
    /// Gets the short runtime identifier shown in the workspace card.
    /// </summary>
    public string RuntimeShortName => CurrentSession?.Runtime.DisplayName ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the active runtime is an exact version match.
    /// </summary>
    public bool IsRuntimeExactMatch => CurrentSession?.VersionMatch.IsExactMatch ?? false;

    /// <summary>
    /// Gets the runtime options available to the shell.
    /// </summary>
    public IReadOnlyList<RuntimeSelectionOption> RuntimeOptions => _runtimeOptions;

    /// <summary>
    /// Gets a value indicating whether the runtime selector is available.
    /// </summary>
    public bool CanChangeRuntimeSelection => !IsBusy;

    /// <summary>
    /// Gets the helper text for the currently selected runtime option.
    /// </summary>
    public string SelectedRuntimeOptionDescription => SelectedRuntimeOption?.Description ??
        "Auto-detect chooses the closest compatible registered runtime.";

    /// <summary>
    /// Gets the status text for the current runtime-selection mode.
    /// </summary>
    public string RuntimeSelectionStatusText => SelectedRuntimeOption switch
    {
        { IsAutoDetect: true } when CurrentSession is null =>
            "Auto-detect uses the exported Spine version to choose the closest registered runtime adapter.",
        { IsAutoDetect: true } =>
            $"Auto-detected with {CurrentSession!.Runtime.DisplayName}.",
        RuntimeSelectionOption runtimeOption when CurrentSession is null =>
            $"The next model will load with {runtimeOption.DisplayName}.",
        RuntimeSelectionOption runtimeOption =>
            $"Changing this option reloads the current model with {runtimeOption.DisplayName}.",
        _ => string.Empty,
    };

    /// <summary>
    /// Gets a value indicating whether the viewport should show the landing experience.
    /// </summary>
    public bool ShowLandingState => HasNoActiveSession && !IsBusy;

    /// <summary>
    /// Gets a value indicating whether the viewport should show loading progress.
    /// </summary>
    public bool ShowLoadingState => HasNoActiveSession && IsBusy;

    /// <summary>
    /// Gets a value indicating whether the landing experience is showing first-run guidance.
    /// </summary>
    public bool ShowFirstRunState => ShowLandingState && !HasDiagnostics;

    /// <summary>
    /// Gets a value indicating whether the landing experience is showing recovery guidance.
    /// </summary>
    public bool ShowLoadFailureState => ShowLandingState && HasDiagnostics;

    /// <summary>
    /// Gets the current landing-experience title.
    /// </summary>
    public string WorkspaceExperienceTitle => ShowLoadFailureState
        ? "Couldn't load that model"
        : "Load a Spine model";

    /// <summary>
    /// Gets the current landing-experience description.
    /// </summary>
    public string WorkspaceExperienceDescription => ShowLoadFailureState
        ? "The workspace is still ready. Review the recovery guidance below, then try another file or reopen a recent project."
        : "Choose a skeleton or atlas file, or drop the files here.";

    /// <summary>
    /// Gets a value indicating whether the last open attempt can be retried.
    /// </summary>
    public bool CanRetryLastOpen => _workspaceSessionService.CanRetryLastOpen && !IsBusy;

    /// <summary>
    /// Gets the primary recovery action label for the current failure.
    /// </summary>
    public string RecoveryPrimaryActionText => HasBlockingDiagnostics
        ? "Replace Missing Files..."
        : "Load Model...";

    /// <summary>
    /// Gets the file formats accepted by the Phase 10 open flow.
    /// </summary>
    public string OpenFormatsSummary => "Supported inputs: .atlas, .json, .skel, and .bytes.";

    /// <summary>
    /// Gets the pairing guidance shown for the first-run open flow.
    /// </summary>
    public string PairingGuidanceText =>
        "Start with one file when the atlas and skeleton share the same base name and folder. If they do not, select or drop exactly one atlas file and one skeleton file together.";

    /// <summary>
    /// Gets the version-detection guidance shown for the first-run open flow.
    /// </summary>
    public string VersionDetectionGuidanceText =>
        $"{_runtimeSupportSummaryText} Auto-detect prefers the closest compatible export family, and you can override it from the runtime selector.";

    /// <summary>
    /// Gets the loading-state description shown while the workspace is opening a model.
    /// </summary>
    public string LoadingStateDescription =>
        "Checking companion files, texture pages, and runtime compatibility.";

    /// <summary>
    /// Gets the drop hint shown in the landing experience.
    /// </summary>
    public string DropHintText =>
        "Drop one or two files here: one skeleton file and one atlas file at most.";

    /// <summary>
    /// Gets the keyboard shortcut summary shown in the shell.
    /// </summary>
    public string KeyboardShortcutsSummaryText =>
        "Shortcuts: Ctrl+O open, Ctrl+Shift+R retry last attempt, F fit view, 0 reset camera, G grid, O origin, B background, Space play/pause.";

    /// <summary>
    /// Gets the toolbar label for the workspace-pane toggle.
    /// </summary>
    public string WorkspacePaneButtonText => IsWorkspacePaneVisible
        ? "Hide Workspace Pane"
        : "Show Workspace Pane";

    /// <summary>
    /// Gets the toolbar label for the details-pane toggle.
    /// </summary>
    public string DetailsPaneButtonText => IsDetailsPaneVisible
        ? "Hide Details Pane"
        : "Show Details Pane";

    /// <summary>
    /// Gets the selected diagnostic title.
    /// </summary>
    public string SelectedDiagnosticTitle => SelectedDiagnostic is null
        ? "Diagnostic Details"
        : $"{SelectedDiagnostic.Severity} | {SelectedDiagnostic.Code}";

    /// <summary>
    /// Gets the selected diagnostic message.
    /// </summary>
    public string SelectedDiagnosticMessage => SelectedDiagnostic?.Message ??
                                               "Open a model or select a recent project to inspect recovery details.";

    /// <summary>
    /// Gets the selected diagnostic source summary.
    /// </summary>
    public string SelectedDiagnosticSource => SelectedDiagnostic is null
        ? "Diagnostics will appear here when the viewer needs your attention."
        : $"Source: {SelectedDiagnostic.Source}";

    /// <summary>
    /// Gets the selected diagnostic recovery guidance.
    /// </summary>
    public string SelectedDiagnosticSuggestedAction => SelectedDiagnostic?.SuggestedAction ??
                                                       "Open a model or select a recent project to begin.";

    /// <summary>
    /// Opens one or two selected Spine files through the workspace lifecycle.
    /// </summary>
    /// <param name="selectedPaths">The selected atlas and/or skeleton paths.</param>
    /// <param name="cancellationToken">A token that cancels the open operation.</param>
    /// <returns>A task that completes when the open flow has finished.</returns>
    public Task OpenFilesAsync(
        IReadOnlyList<string> selectedPaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selectedPaths);

        if (selectedPaths.Count == 0 || IsBusy)
        {
            return Task.CompletedTask;
        }

        return _workspaceSessionService.OpenAsync(selectedPaths, cancellationToken);
    }

    /// <summary>
    /// Updates the shell's responsive layout mode for the current window width.
    /// </summary>
    /// <param name="availableWidth">The current window width in device-independent pixels.</param>
    public void UpdateLayoutWidth(double availableWidth)
    {
        if (availableWidth <= 0.0)
        {
            return;
        }

        IsCompactLayout = availableWidth < CompactLayoutThreshold;
    }

    /// <summary>
    /// Releases the workspace-state subscription held by the shell view model.
    /// </summary>
    public void Dispose()
    {
        _workspaceSessionService.StateChanged -= OnWorkspaceStateChanged;
        Viewport.Dispose();
        Playback.Dispose();
        TrackEditor.Dispose();
        Inspector.Dispose();
        DiagnosticsPanel.Dispose();
        Settings.Dispose();
    }

    private void ApplyState(WorkspaceState state)
    {
        CurrentSession = state.CurrentSession;
        RecentFiles = state.RecentFiles;
        Diagnostics = state.Diagnostics;
        StatusText = state.StatusText;
        IsBusy = state.IsBusy;
        SelectedRecentProject = ChooseRecentProjectSelection(state);
        SelectedDiagnostic = ChooseDiagnosticSelection(state.Diagnostics);
        SynchronizeRuntimeSelection(state.PreferredRuntimeId);
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        ApplyState(_workspaceSessionService.State);
    }

    partial void OnSelectedRuntimeOptionChanged(RuntimeSelectionOption? value)
    {
        if (_isSynchronizingRuntimeSelection || value is null)
        {
            return;
        }

        _ = ApplyRuntimeSelectionAsync(value);
    }

    [RelayCommand(CanExecute = nameof(CanReloadSession))]
    private Task ReloadSessionAsync()
    {
        return _workspaceSessionService.ReloadAsync(CancellationToken.None);
    }

    [RelayCommand(CanExecute = nameof(CanCloseSession))]
    private Task CloseSessionAsync()
    {
        return _workspaceSessionService.CloseAsync(CancellationToken.None);
    }

    [RelayCommand(CanExecute = nameof(CanOpenSelectedRecentProject))]
    private Task OpenSelectedRecentProjectAsync()
    {
        if (SelectedRecentProject is null)
        {
            return Task.CompletedTask;
        }

        return _workspaceSessionService.OpenAsync(SelectedRecentProject, CancellationToken.None);
    }

    [RelayCommand(CanExecute = nameof(CanClearRecentProjects))]
    private Task ClearRecentProjectsAsync()
    {
        return _workspaceSessionService.ClearRecentProjectsAsync(CancellationToken.None);
    }

    [RelayCommand(CanExecute = nameof(CanRemoveRecentProject))]
    private Task RemoveRecentProjectAsync(SpineProjectReference? projectReference)
    {
        if (projectReference is null)
        {
            return Task.CompletedTask;
        }

        return _workspaceSessionService.RemoveRecentProjectAsync(projectReference, CancellationToken.None);
    }

    [RelayCommand(CanExecute = nameof(CanRetryLastOpen))]
    private Task RetryLastOpenAsync()
    {
        return _workspaceSessionService.RetryLastOpenAsync(CancellationToken.None);
    }

    [RelayCommand]
    private void ToggleWorkspacePane()
    {
        IsWorkspacePaneVisible = !IsWorkspacePaneVisible;
    }

    [RelayCommand]
    private void ToggleDetailsPane()
    {
        IsDetailsPaneVisible = !IsDetailsPaneVisible;
    }

    private async Task ApplyRuntimeSelectionAsync(RuntimeSelectionOption runtimeOption)
    {
        _workspaceSessionService.UpdatePreferredRuntimeId(runtimeOption.RuntimeId);

        if (!HasActiveSession || IsBusy || !ReloadSessionCommand.CanExecute(null))
        {
            return;
        }

        await ReloadSessionCommand.ExecuteAsync(null);
    }

    private bool CanCloseSession()
    {
        return CurrentSession is not null && !IsBusy;
    }

    private bool CanOpenSelectedRecentProject()
    {
        return SelectedRecentProject is not null && !IsBusy;
    }

    private bool CanReloadSession()
    {
        return CurrentSession is not null && !IsBusy;
    }

    private bool CanClearRecentProjects()
    {
        return HasRecentFiles && !IsBusy;
    }

    private bool CanRemoveRecentProject(SpineProjectReference? projectReference)
    {
        return projectReference is not null && !IsBusy;
    }

    private static IReadOnlyList<RuntimeSelectionOption> BuildRuntimeOptions(
        IReadOnlyList<SpineRuntimeDescriptor> runtimes)
    {
        List<RuntimeSelectionOption> options =
        [
            new RuntimeSelectionOption(
                null,
                "Auto-detect",
                "Closest compatible registered runtime."),
        ];

        options.AddRange(
            runtimes.Select(
                runtime => new RuntimeSelectionOption(
                    runtime.RuntimeId,
                    runtime.DisplayName,
                    $"Supports {runtime.SupportedExportRange}.")));

        return options;
    }

    private static string BuildRuntimeSupportSummary(IReadOnlyList<SpineRuntimeDescriptor> runtimes)
    {
        if (runtimes.Count == 0)
        {
            return "No runtime adapters are currently registered.";
        }

        string joinedRuntimeNames = string.Join(", ", runtimes.Select(static runtime => runtime.DisplayName));
        return runtimes.Count == 1
            ? $"Available runtime adapter: {joinedRuntimeNames}."
            : $"Available runtime adapters: {joinedRuntimeNames}.";
    }

    private static int CountActiveViewportOverlays(ViewportState viewportState)
    {
        int activeCount = 0;

        if (viewportState.ShowGrid)
        {
            activeCount++;
        }

        if (viewportState.ShowOrigin)
        {
            activeCount++;
        }

        if (viewportState.ShowBones)
        {
            activeCount++;
        }

        if (viewportState.ShowBounds)
        {
            activeCount++;
        }

        if (viewportState.ShowMeshWireframe)
        {
            activeCount++;
        }

        if (viewportState.ShowSlotOutlines)
        {
            activeCount++;
        }

        if (viewportState.ShowLabels)
        {
            activeCount++;
        }

        if (viewportState.ShowMissingResourceIndicators)
        {
            activeCount++;
        }

        if (viewportState.ShowUnsupportedFeatureIndicators)
        {
            activeCount++;
        }

        return activeCount;
    }

    private SpineProjectReference? ChooseRecentProjectSelection(WorkspaceState state)
    {
        if (state.RecentFiles.Count == 0)
        {
            return null;
        }

        if (state.CurrentSession is not null)
        {
            SpineProjectReference? matchingSessionProject = state.RecentFiles
                .FirstOrDefault(projectReference => projectReference == state.CurrentSession.Project);

            if (matchingSessionProject is not null)
            {
                return matchingSessionProject;
            }
        }

        if (SelectedRecentProject is not null)
        {
            SpineProjectReference? matchingSelection = state.RecentFiles
                .FirstOrDefault(projectReference => projectReference == SelectedRecentProject);

            if (matchingSelection is not null)
            {
                return matchingSelection;
            }
        }

        return state.RecentFiles[0];
    }

    private ViewerDiagnostic? ChooseDiagnosticSelection(IReadOnlyList<ViewerDiagnostic> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return null;
        }

        if (SelectedDiagnostic is not null)
        {
            ViewerDiagnostic? matchingSelection = diagnostics
                .FirstOrDefault(diagnostic => diagnostic == SelectedDiagnostic);

            if (matchingSelection is not null)
            {
                return matchingSelection;
            }
        }

        return diagnostics[0];
    }

    private void SynchronizeRuntimeSelection(string? preferredRuntimeId)
    {
        RuntimeSelectionOption resolvedOption = _runtimeOptions.FirstOrDefault(
                option => string.Equals(option.RuntimeId, preferredRuntimeId, StringComparison.OrdinalIgnoreCase))
            ?? _runtimeOptions[0];

        if (ReferenceEquals(SelectedRuntimeOption, resolvedOption))
        {
            return;
        }

        _isSynchronizingRuntimeSelection = true;

        try
        {
            SelectedRuntimeOption = resolvedOption;
        }
        finally
        {
            _isSynchronizingRuntimeSelection = false;
        }
    }
}
