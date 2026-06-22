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
    private readonly ISpineProjectLoader _projectLoader;
    private readonly ISpineProjectReferenceResolver _projectReferenceResolver;
    private readonly IRecentFilesService _recentFilesService;
    private readonly IRuntimeSelectionService _runtimeSelectionService;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ISpineSessionFactory _sessionFactory;
    private readonly IVersionDetectionService _versionDetectionService;
    private WorkspaceState _state = new(
        null,
        Array.Empty<SpineProjectReference>(),
        Array.Empty<ViewerDiagnostic>(),
        "Ready.",
        false);
    private ViewerSettings _viewerSettings = new();
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceSessionService"/> class.
    /// </summary>
    /// <param name="projectReferenceResolver">The resolver used to turn user-facing project references into concrete file sets.</param>
    /// <param name="versionDetectionService">The version-detection service used before runtime selection.</param>
    /// <param name="runtimeSelectionService">The runtime-selection service used before loading.</param>
    /// <param name="projectLoader">The loader used to load resolved projects.</param>
    /// <param name="sessionFactory">The factory used to create durable workspace sessions.</param>
    /// <param name="recentFilesService">The recent-files service for persisted history.</param>
    /// <param name="settingsRepository">The settings repository for restore and transient-state defaults.</param>
    public WorkspaceSessionService(
        ISpineProjectReferenceResolver projectReferenceResolver,
        IVersionDetectionService versionDetectionService,
        IRuntimeSelectionService runtimeSelectionService,
        ISpineProjectLoader projectLoader,
        ISpineSessionFactory sessionFactory,
        IRecentFilesService recentFilesService,
        ISettingsRepository settingsRepository)
    {
        _projectReferenceResolver = projectReferenceResolver ?? throw new ArgumentNullException(nameof(projectReferenceResolver));
        _versionDetectionService = versionDetectionService ?? throw new ArgumentNullException(nameof(versionDetectionService));
        _runtimeSelectionService = runtimeSelectionService ?? throw new ArgumentNullException(nameof(runtimeSelectionService));
        _projectLoader = projectLoader ?? throw new ArgumentNullException(nameof(projectLoader));
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _recentFilesService = recentFilesService ?? throw new ArgumentNullException(nameof(recentFilesService));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
    }

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    /// <inheritdoc />
    public WorkspaceState State => _state;

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

        await OpenAsync(selectedPath, companionPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task OpenAsync(
        string selectedPath,
        string? companionPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            StartLifecycleOperation($"Opening {Path.GetFileName(selectedPath)}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveFromSelectionAsync(selectedPath, companionPath, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                resolveResult,
                "Opened",
                "Failed to open",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                null,
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

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            StartLifecycleOperation($"Opening {projectReference.DisplayName}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveForReopenAsync(projectReference, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                resolveResult,
                "Opened",
                "Failed to open",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                null,
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
            StartLifecycleOperation($"Reloading {projectReference.DisplayName}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveForReopenAsync(projectReference, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                resolveResult,
                "Reloaded",
                "Failed to reload",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                null,
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

            if (!_viewerSettings.RestoreLastSessionOnStartup || _viewerSettings.LastProjectReference is null)
            {
                return;
            }

            StartLifecycleOperation($"Restoring {_viewerSettings.LastProjectReference.DisplayName}.");

            ResolveSpineProjectResult resolveResult = await _projectReferenceResolver
                .ResolveForReopenAsync(_viewerSettings.LastProjectReference, cancellationToken)
                .ConfigureAwait(false);

            await CompleteOpenFromResolveAsync(
                resolveResult,
                "Restored",
                "Failed to restore",
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            FinishLifecycleOperation(
                null,
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

    private static IReadOnlyList<ViewerDiagnostic> MergeDiagnostics(
        params IEnumerable<ViewerDiagnostic>[] diagnosticGroups)
    {
        return diagnosticGroups
            .SelectMany(static group => group)
            .Distinct()
            .ToArray();
    }

    private async Task CompleteOpenFromResolveAsync(
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
                null,
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
                    versionMatch),
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
                null,
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
                null,
                mergedLoadResult.Diagnostics,
                $"{failureVerb} {resolveResult.ProjectReference.DisplayName}.",
                false);
            return;
        }

        SpineProjectSession session = _sessionFactory.Create(mergedLoadResult, _viewerSettings);

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

        _viewerSettings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
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
        _state = new WorkspaceState(
            currentSession,
            _state.RecentFiles,
            diagnostics,
            statusText,
            isBusy);

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task PersistLastProjectReferenceCoreAsync(
        SpineProjectReference? projectReference,
        CancellationToken cancellationToken)
    {
        _viewerSettings = _viewerSettings with
        {
            LastProjectReference = projectReference,
        };

        await _settingsRepository.SaveAsync(_viewerSettings, cancellationToken).ConfigureAwait(false);
    }

    private async Task RefreshRecentFilesCoreAsync(CancellationToken cancellationToken)
    {
        await _recentFilesService.RemoveMissingEntriesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<SpineProjectReference> recentFiles = await _recentFilesService
            .GetRecentFilesAsync(cancellationToken)
            .ConfigureAwait(false);

        _state = new WorkspaceState(
            _state.CurrentSession,
            recentFiles,
            _state.Diagnostics,
            _state.StatusText,
            _state.IsBusy);

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void StartLifecycleOperation(string statusText)
    {
        _state = new WorkspaceState(
            null,
            _state.RecentFiles,
            Array.Empty<ViewerDiagnostic>(),
            statusText,
            true);

        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
