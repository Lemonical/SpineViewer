using SpineViewer.Core.Abstractions;

namespace SpineViewer.Core.Services;

/// <summary>
/// Runs registered startup tasks and records high-level startup progress.
/// </summary>
public sealed class ApplicationStartupPipeline : IApplicationStartupPipeline
{
    private readonly IAppLogger<ApplicationStartupPipeline> _logger;
    private readonly IReadOnlyList<IApplicationStartupTask> _startupTasks;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationStartupPipeline"/> class.
    /// </summary>
    /// <param name="startupTasks">The ordered startup tasks to execute.</param>
    /// <param name="logger">The logger used for startup progress.</param>
    public ApplicationStartupPipeline(
        IEnumerable<IApplicationStartupTask> startupTasks,
        IAppLogger<ApplicationStartupPipeline> logger)
    {
        _startupTasks = startupTasks.ToList();
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting application pipeline with {_startupTasks.Count} task(s).");

        foreach (IApplicationStartupTask startupTask in _startupTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation($"Running startup task {startupTask.GetType().Name}.");
            await startupTask.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Application startup pipeline completed.");
    }
}
