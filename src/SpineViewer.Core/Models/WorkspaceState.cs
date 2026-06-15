using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the durable application-facing workspace state for the current viewer process.
/// </summary>
public sealed record WorkspaceState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkspaceState"/> class.
    /// </summary>
    /// <param name="currentSession">The currently open session, if any.</param>
    /// <param name="recentFiles">The persisted recent-file entries.</param>
    /// <param name="diagnostics">The current workspace diagnostics.</param>
    /// <param name="statusText">The current user-facing status text.</param>
    /// <param name="isBusy">Indicates whether the workspace is running a lifecycle operation.</param>
    public WorkspaceState(
        SpineProjectSession? currentSession,
        IEnumerable<SpineProjectReference> recentFiles,
        IEnumerable<ViewerDiagnostic> diagnostics,
        string statusText,
        bool isBusy)
    {
        CurrentSession = currentSession;
        RecentFiles = Guard.MaterializeReadOnlyList(recentFiles, nameof(recentFiles));
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
        StatusText = Guard.NotNullOrWhiteSpace(statusText, nameof(statusText));
        IsBusy = isBusy;
    }

    /// <summary>
    /// Gets the currently open session, if any.
    /// </summary>
    public SpineProjectSession? CurrentSession { get; init; }

    /// <summary>
    /// Gets the persisted recent-file entries.
    /// </summary>
    public IReadOnlyList<SpineProjectReference> RecentFiles { get; init; }

    /// <summary>
    /// Gets the current workspace diagnostics.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }

    /// <summary>
    /// Gets the current user-facing status text.
    /// </summary>
    public string StatusText { get; init; }

    /// <summary>
    /// Gets a value indicating whether the workspace is running a lifecycle operation.
    /// </summary>
    public bool IsBusy { get; init; }

    /// <summary>
    /// Gets a value indicating whether a session is currently open.
    /// </summary>
    public bool HasSession => CurrentSession is not null;

    /// <summary>
    /// Gets a value indicating whether the current workspace can be reloaded.
    /// </summary>
    public bool CanReload => CurrentSession is not null && !IsBusy;
}
