namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Runs the ordered application startup workflow before the shell becomes available.
/// </summary>
public interface IApplicationStartupPipeline
{
    /// <summary>
    /// Executes all registered startup tasks in registration order.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels startup work.</param>
    /// <returns>A task that completes when startup has finished.</returns>
    Task RunAsync(CancellationToken cancellationToken);
}
