using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Features.Workspace.Services;

/// <summary>
/// Coordinates workspace session lifecycle flows and keeps stale transient state from leaking across project boundaries.
/// </summary>
public sealed class WorkspaceSessionService : IWorkspaceSessionService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly IUiDispatcher _uiDispatcher;
    private readonly ISpineProjectLoader _projectLoader;
    private readonly ISpineProjectReferenceResolver _projectReferenceResolver;
    private readonly IRecentFilesService _recentFilesService;
    private readonly IRuntimeSelectionService _runtimeSelectionService;
    private readonly ISpineSessionFactory _sessionFactory;
    private readonly IViewerSettingsService _viewerSettingsService;
    private readonly IVersionDetectionService _versionDetectionService;
    private WorkspaceState _state = new(
        null,
        Array.Empty<SpineProjectReference>(),
        Array.Empty<ViewerDiagnostic>(),
        "Ready.",
        false)
    {
        PreferredRuntimeId = null,
    };
    private bool _isInitialized;
    private RetryRequest? _lastRetryRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSessionService"/> class.
    /// </summary>
    /// <param name="projectReferenceResolver">The resolver used to turn user-facing project references into concrete file sets.</param>
    /// <param name="versionDetectionService">The version-detection service used before runtime selection.</param>
    /// <param name="runtimeSelectionService">The runtime-selection service used before loading.</param>
    /// <param name="projectLoader">The loader used to load resolved projects.</param>
    /// <param name="sessionFactory">The factory used to create durable workspace sessions.</param>
    /// <param name="recentFilesService">The recent-files service for persisted history.</param>
    /// <param name="viewerSettingsService">The shared viewer settings service for restore and transient-state defaults.</param>
    /// <param name="uiDispatcher">The dispatcher used to publish workspace state changes on the UI thread.</param>
    public WorkspaceSessionService(
        ISpineProjectReferenceResolver projectReferenceResolver,
        IVersionDetectionService versionDetectionService,
        IRuntimeSelectionService runtimeSelectionService,
        ISpineProjectLoader projectLoader,
        ISpineSessionFactory sessionFactory,
        IRecentFilesService recentFilesService,
        IViewerSettingsService viewerSettingsService,
        IUiDispatcher uiDispatcher)
    {
        _projectReferenceResolver = projectReferenceResolver ?? throw new ArgumentNullException(nameof(projectReferenceResolver));
        _versionDetectionService = versionDetectionService ?? throw new ArgumentNullException(nameof(versionDetectionService));
        _runtimeSelectionService = runtimeSelectionService ?? throw new ArgumentNullException(nameof(runtimeSelectionService));
        _projectLoader = projectLoader ?? throw new ArgumentNullException(nameof(projectLoader));
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _recentFilesService = recentFilesService ?? throw new ArgumentNullException(nameof(recentFilesService));
        _viewerSettingsService = viewerSettingsService ?? throw new ArgumentNullException(nameof(viewerSettingsService));
        _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
    }

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public WorkspaceState State => _state;

    /// <inheritdoc />
    public bool CanRetryLastOpen => _lastRetryRequest is not null && !_state.IsBusy;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task OpenAsync(
        IReadOnlyList<string> selectedPaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(selectedPaths);

        string[] materializedSelectedPaths = selectedPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        if (materializedSelectedPaths.Length == 0)
        {
            throw new ArgumentException(
                "At least one selected path is required.",
                nameof(selectedPaths));
        }

        if (materializedSelectedPaths.Length > 2)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);

                FinishLifecycleOperation(
                    _state.CurrentSession,
                    [
                        new ViewerDiagnostic(
                            "project-selection-too-many-files",
                            ViewerDiagnosticSeverity.Error,
                            "Select or drop no more than one skeleton file and one atlas file at a time.",
                            nameof(WorkspaceSessionService),
                            $"Selected path count: {materializedSelectedPaths.Length}.",
                            "Choose one .atlas file and one .json, .skel, or .bytes file, or start with a single file and let the viewer auto-pair it."),
                    ],
                    "Select one skeleton file and one atlas file.",
                    false);
            }
            finally
            {
                _gate.Release();
            }

            return;
        }

        string selectedPath = materializedSelectedPaths[0];
        string? companionPath = materializedSelectedPaths.Length == 2
            ? materializedSelectedPaths[1]
            : null;

        _lastRetryRequest = RetryRequest.CreateSelection(selectedPath, companionPath);
        await OpenAsync(selectedPath, companionPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task OpenAsync(
        string selectedPath,
        string? companionPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
        _lastRetryRequest = RetryRequest.CreateSelection(selectedPath, companionPath);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            SpineProjectSession? existingSession = _state.CurrentSession;
            StartLifecycleOperation(existingSession, $"Opening {Path.GetFileName(selectedPath)}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveFromSelectionAsync(selectedPath, companionPath, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                existingSession,
                resolveResult,
                "Opened",
                "Failed to open",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                _state.CurrentSession,
                Array.Empty<ViewerDiagnostic>(),
                "Open canceled.",
                false);
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task OpenAsync(
        SpineProjectReference projectReference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectReference);
        _lastRetryRequest = RetryRequest.CreateProjectReference(projectReference);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            SpineProjectSession? existingSession = _state.CurrentSession;
            StartLifecycleOperation(existingSession, $"Opening {projectReference.DisplayName}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveForReopenAsync(projectReference, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                existingSession,
                resolveResult,
                "Opened",
                "Failed to open",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                _state.CurrentSession,
                Array.Empty<ViewerDiagnostic>(),
                "Open canceled.",
                false);
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveRecentProjectAsync(
        SpineProjectReference projectReference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(projectReference);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            await _recentFilesService.RemoveAsync(projectReference, cancellationToken).ConfigureAwait(false);

            IReadOnlyList<SpineProjectReference> recentFiles = await _recentFilesService
                .GetRecentFilesAsync(cancellationToken)
                .ConfigureAwait(false);

            PublishState(
                CreateWorkspaceState(
                    _state.CurrentSession,
                    recentFiles,
                    _state.Diagnostics,
                    $"Removed {projectReference.DisplayName} from recent projects.",
                    false));
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearRecentProjectsAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            await _recentFilesService.ClearAsync(cancellationToken).ConfigureAwait(false);

            PublishState(
                CreateWorkspaceState(
                    _state.CurrentSession,
                    Array.Empty<SpineProjectReference>(),
                    _state.Diagnostics,
                    "Cleared recent projects.",
                    false));
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ReloadAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);

            if (_state.CurrentSession is null)
            {
                FinishLifecycleOperation(
                    null,
                    Array.Empty<ViewerDiagnostic>(),
                    "No session is currently open.",
                    false);
                return;
            }

            SpineProjectReference projectReference = _state.CurrentSession.Project;
            SpineProjectSession existingSession = _state.CurrentSession;
            _lastRetryRequest = RetryRequest.CreateProjectReference(projectReference);
            StartLifecycleOperation(existingSession, $"Reloading {projectReference.DisplayName}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveForReopenAsync(projectReference, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                existingSession,
                resolveResult,
                "Reloaded",
                "Failed to reload",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                _state.CurrentSession,
                Array.Empty<ViewerDiagnostic>(),
                "Reload canceled.",
                false);
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            await PersistLastProjectReferenceCoreAsync(null, cancellationToken).ConfigureAwait(false);

            FinishLifecycleOperation(
                null,
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task RestoreLastSessionAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);

            ViewerSettings viewerSettings = _viewerSettingsService.CurrentSettings;
            if (!viewerSettings.RestoreLastSessionOnStartup || viewerSettings.LastProjectReference is null)
            {
                return;
            }

            SpineProjectSession? existingSession = _state.CurrentSession;
            _lastRetryRequest = RetryRequest.CreateProjectReference(viewerSettings.LastProjectReference);
            StartLifecycleOperation(existingSession, $"Restoring {viewerSettings.LastProjectReference.DisplayName}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveForReopenAsync(viewerSettings.LastProjectReference, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                existingSession,
                resolveResult,
                "Restored",
                "Failed to restore",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                _state.CurrentSession,
                Array.Empty<ViewerDiagnostic>(),
                "Restore canceled.",
                false);
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public Task RetryLastOpenAsync(CancellationToken cancellationToken)
    {
        if (_lastRetryRequest is null)
        {
            return Task.CompletedTask;
        }

        return _lastRetryRequest.Kind == RetryRequestKind.Selection
            ? OpenAsync(
                _lastRetryRequest.SelectedPath!,
                _lastRetryRequest.CompanionPath,
                cancellationToken)
            : OpenAsync(_lastRetryRequest.ProjectReference!, cancellationToken);
    }

    /// <inheritdoc />
    public void UpdatePlaybackState(PlaybackState playbackState)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        if (_state.CurrentSession is null)
        {
            return;
        }

        SpineProjectSession updatedSession = _state.CurrentSession with
        {
            Playback = playbackState,
        };

        FinishLifecycleOperation(
            updatedSession,
            _state.Diagnostics,
            _state.StatusText,
            _state.IsBusy);
    }

    /// <inheritdoc />
    public void UpdateViewportState(ViewportState viewportState)
    {
        ArgumentNullException.ThrowIfNull(viewportState);

        if (_state.CurrentSession is null)
        {
            return;
        }

        SpineProjectSession updatedSession = _state.CurrentSession with
        {
            Viewport = viewportState,
        };

        FinishLifecycleOperation(
            updatedSession,
            _state.Diagnostics,
            _state.StatusText,
            _state.IsBusy);
    }

    /// <inheritdoc />
    public void UpdateSelectedSkin(string? skinName)
    {
        if (_state.CurrentSession is null)
        {
            return;
        }

        SpineProjectSession updatedSession = _state.CurrentSession with
        {
            SelectedSkinName = string.IsNullOrWhiteSpace(skinName)
                ? null
                : skinName,
        };

        FinishLifecycleOperation(
            updatedSession,
            _state.Diagnostics,
            _state.StatusText,
            _state.IsBusy);
    }

    /// <inheritdoc />
    public void UpdatePreferredRuntimeId(string? runtimeId)
    {
        string? normalizedRuntimeId = string.IsNullOrWhiteSpace(runtimeId)
            ? null
            : runtimeId;
        if (string.Equals(_state.PreferredRuntimeId, normalizedRuntimeId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        PublishState(_state with { PreferredRuntimeId = normalizedRuntimeId });
    }

    private static IReadOnlyList<ViewerDiagnostic> MergeDiagnostics(
        params IEnumerable<ViewerDiagnostic>[] diagnosticGroups)
    {
        return diagnosticGroups
            .SelectMany(static group => group)
            .Distinct()
            .ToArray();
    }

    private async Task CompleteOpenFromResolveAsync(
        SpineProjectSession? fallbackSession,
        ResolveSpineProjectResult resolveResult,
        string successVerb,
        string failureVerb,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!resolveResult.IsSuccessful ||
            resolveResult.ProjectReference is null ||
            resolveResult.AssetFileSet is null)
        {
            await PersistLastProjectReferenceCoreAsync(null, cancellationToken).ConfigureAwait(false);
            await RefreshRecentFilesCoreAsync(cancellationToken).ConfigureAwait(false);

            string failedDisplayName = resolveResult.ProjectReference?.DisplayName ?? "project";
            FinishLifecycleOperation(
                fallbackSession,
                resolveResult.Diagnostics,
                $"{failureVerb} {failedDisplayName}.",
                false);
            return;
        }

        SpineVersionMatch versionMatch = await _versionDetectionService
            .DetectAsync(resolveResult.AssetFileSet, cancellationToken)
            .ConfigureAwait(false);

        RuntimeSelectionResult selectionResult = await _runtimeSelectionService
            .SelectAsync(
                new RuntimeSelectionRequest(
                    resolveResult.ProjectReference,
                    resolveResult.AssetFileSet,
                    versionMatch,
                    _state.PreferredRuntimeId),
                cancellationToken)
            .ConfigureAwait(false);

        if (selectionResult.SelectedRuntime is null)
        {
            IReadOnlyList<ViewerDiagnostic> failedDiagnostics = MergeDiagnostics(
                resolveResult.Diagnostics,
                selectionResult.Diagnostics);

            await PersistLastProjectReferenceCoreAsync(null, cancellationToken).ConfigureAwait(false);
            await RefreshRecentFilesCoreAsync(cancellationToken).ConfigureAwait(false);

            FinishLifecycleOperation(
                fallbackSession,
                failedDiagnostics,
                $"{failureVerb} {resolveResult.ProjectReference.DisplayName}.",
                false);
            return;
        }

        LoadSpineProjectResult loadResult = await _projectLoader
            .LoadAsync(
                new LoadSpineProjectRequest(
                    resolveResult.ProjectReference,
                    resolveResult.AssetFileSet,
                    selectionResult.SelectedRuntime,
                    versionMatch),
                cancellationToken)
            .ConfigureAwait(false);

        LoadSpineProjectResult mergedLoadResult = loadResult with
        {
            Diagnostics = MergeDiagnostics(
                resolveResult.Diagnostics,
                selectionResult.Diagnostics,
                loadResult.Diagnostics),
        };

        if (!mergedLoadResult.IsSuccessful)
        {
            await PersistLastProjectReferenceCoreAsync(null, cancellationToken).ConfigureAwait(false);
            await RefreshRecentFilesCoreAsync(cancellationToken).ConfigureAwait(false);

            FinishLifecycleOperation(
                fallbackSession,
                mergedLoadResult.Diagnostics,
                $"{failureVerb} {resolveResult.ProjectReference.DisplayName}.",
                false);
            return;
        }

        SpineProjectSession session = _sessionFactory.Create(mergedLoadResult, _viewerSettingsService.CurrentSettings);

        await _recentFilesService.AddAsync(session.Project, cancellationToken).ConfigureAwait(false);
        await PersistLastProjectReferenceCoreAsync(session.Project, cancellationToken).ConfigureAwait(false);
        await RefreshRecentFilesCoreAsync(cancellationToken).ConfigureAwait(false);

        FinishLifecycleOperation(
            session,
            session.Diagnostics,
            $"{successVerb} {session.Project.DisplayName}.",
            false);
    }

    private async Task EnsureInitializedCoreAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized)
        {
            return;
        }

        await _viewerSettingsService.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await RefreshRecentFilesCoreAsync(cancellationToken).ConfigureAwait(false);

        if (!_state.HasSession)
        {
            FinishLifecycleOperation(
                null,
                Array.Empty<ViewerDiagnostic>(),
                "Ready.",
                false);
        }

        _isInitialized = true;
    }

    private void FinishLifecycleOperation(
        SpineProjectSession? currentSession,
        IReadOnlyList<ViewerDiagnostic> diagnostics,
        string statusText,
        bool isBusy)
    {
        PublishState(CreateWorkspaceState(currentSession, _state.RecentFiles, diagnostics, statusText, isBusy));
    }

    private async Task PersistLastProjectReferenceCoreAsync(
        SpineProjectReference? projectReference,
        CancellationToken cancellationToken)
    {
        await _viewerSettingsService
            .SaveAsync(
                _viewerSettingsService.CurrentSettings with
                {
                    LastProjectReference = projectReference,
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task RefreshRecentFilesCoreAsync(CancellationToken cancellationToken)
    {
        await _recentFilesService.RemoveMissingEntriesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<SpineProjectReference> recentFiles = await _recentFilesService
            .GetRecentFilesAsync(cancellationToken)
            .ConfigureAwait(false);

        PublishState(CreateWorkspaceState(_state.CurrentSession, recentFiles, _state.Diagnostics, _state.StatusText, _state.IsBusy));
    }

    private void StartLifecycleOperation(
        SpineProjectSession? currentSession,
        string statusText)
    {
        PublishState(CreateWorkspaceState(currentSession, _state.RecentFiles, Array.Empty<ViewerDiagnostic>(), statusText, true));
    }

    private WorkspaceState CreateWorkspaceState(
        SpineProjectSession? currentSession,
        IReadOnlyList<SpineProjectReference> recentFiles,
        IReadOnlyList<ViewerDiagnostic> diagnostics,
        string statusText,
        bool isBusy)
    {
        return new WorkspaceState(
            currentSession,
            recentFiles,
            diagnostics,
            statusText,
            isBusy)
        {
            PreferredRuntimeId = _state.PreferredRuntimeId,
        };
    }

    private void PublishState(WorkspaceState workspaceState)
    {
        _uiDispatcher.Invoke(
            () =>
            {
                _state = workspaceState;
                StateChanged?.Invoke(this, EventArgs.Empty);
            });
    }

    private enum RetryRequestKind
    {
        Selection,
        ProjectReference,
    }

    private sealed record RetryRequest(
        RetryRequestKind Kind,
        string? SelectedPath,
        string? CompanionPath,
        SpineProjectReference? ProjectReference)
    {
        public static RetryRequest CreateProjectReference(SpineProjectReference projectReference)
        {
            ArgumentNullException.ThrowIfNull(projectReference);
            return new RetryRequest(RetryRequestKind.ProjectReference, null, null, projectReference);
        }

        public static RetryRequest CreateSelection(string selectedPath, string? companionPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
            return new RetryRequest(RetryRequestKind.Selection, selectedPath, companionPath, null);
        }
    }
}
