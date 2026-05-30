namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Represents one discrete unit of startup work for the application pipeline.
/// </summary>
public interface IApplicationStartupTask
{
    /// <summary>
    /// Executes the startup task.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels startup work.</param>
    /// <returns>A task that completes when the task has finished.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
