using SpineViewer.Core.Abstractions;

namespace SpineViewer.App.Startup;

/// <summary>
/// Restores persisted workspace session state before the main shell is shown.
/// </summary>
public sealed class RestoreLastSessionStartupTask : IApplicationStartupTask
{
    private readonly IWorkspaceSessionService _workspaceSessionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreLastSessionStartupTask"/> class.
    /// </summary>
    /// <param name="workspaceSessionService">The workspace session service to initialize and restore.</param>
    public RestoreLastSessionStartupTask(IWorkspaceSessionService workspaceSessionService)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _workspaceSessionService.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _workspaceSessionService.RestoreLastSessionAsync(cancellationToken).ConfigureAwait(false);
    }
}
