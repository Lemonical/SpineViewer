using CommunityToolkit.Mvvm.ComponentModel;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Shell.ViewModels;

/// <summary>
/// Presents the main shell state for the rewrite-side workspace host.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly IWorkspaceSessionService _workspaceSessionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="workspaceSessionService">The workspace service that drives shell session state.</param>
    public MainWindowViewModel(IWorkspaceSessionService workspaceSessionService)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
        ApplyState(_workspaceSessionService.State);
    }

    /// <summary>
    /// Gets the shell window title.
    /// </summary>
    public string Title => "SpineViewer";

    /// <summary>
    /// Gets a value indicating whether a session is currently open.
    /// </summary>
    [ObservableProperty]
    private bool hasActiveSession;

    /// <summary>
    /// Gets the primary empty-state or session message shown in the shell.
    /// </summary>
    [ObservableProperty]
    private string welcomeMessage = "Welcome to SpineViewer.";

    /// <summary>
    /// Gets the current session summary shown in the shell.
    /// </summary>
    [ObservableProperty]
    private string sessionSummary = "No session is currently open.";

    /// <summary>
    /// Gets the current runtime summary shown in the shell.
    /// </summary>
    [ObservableProperty]
    private string runtimeSummary = "No runtime selected.";

    /// <summary>
    /// Gets the current recent-files summary shown in the shell.
    /// </summary>
    [ObservableProperty]
    private string recentFilesSummary = "No recent projects yet.";

    /// <summary>
    /// Gets the current diagnostic summary shown in the shell.
    /// </summary>
    [ObservableProperty]
    private string diagnosticSummary = "No diagnostics.";

    /// <summary>
    /// Gets the status line shown at the bottom of the shell.
    /// </summary>
    [ObservableProperty]
    private string statusText = "Ready.";

    private void ApplyState(WorkspaceState state)
    {
        StatusText = state.StatusText;
        HasActiveSession = state.HasSession;
        RecentFilesSummary = state.RecentFiles.Count == 0
            ? "No recent projects yet."
            : string.Join(", ", state.RecentFiles.Select(static projectReference => projectReference.DisplayName));
        DiagnosticSummary = state.Diagnostics.Count == 0
            ? "No diagnostics."
            : $"{state.Diagnostics.Count} diagnostic(s) available.";

        if (state.CurrentSession is null)
        {
            WelcomeMessage = "Welcome to SpineViewer.";
            SessionSummary = "No session is currently open.";
            RuntimeSummary = "No runtime selected.";
            return;
        }

        WelcomeMessage = state.CurrentSession.Project.DisplayName;
        SessionSummary =
            $"{state.CurrentSession.AssetFileSet.SkeletonPath} | {state.CurrentSession.AssetFileSet.AtlasPath}";
        RuntimeSummary = state.CurrentSession.Runtime.DisplayName;
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        ApplyState(_workspaceSessionService.State);
    }
}
