using System.Runtime.InteropServices;
using SpineViewer.Core.Abstractions;

namespace SpineViewer.Infrastructure.Startup;

/// <summary>
/// Emits a basic environment snapshot when the application starts.
/// </summary>
public sealed class LogApplicationStartupTask : IApplicationStartupTask
{
    private readonly IAppLogger<LogApplicationStartupTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogApplicationStartupTask"/> class.
    /// </summary>
    /// <param name="logger">The logger used for startup messages.</param>
    public LogApplicationStartupTask(IAppLogger<LogApplicationStartupTask> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string message =
            $"SpineViewer shell booting on {RuntimeInformation.OSDescription} " +
            $"with {RuntimeInformation.FrameworkDescription}.";

        _logger.LogInformation(message);
        return Task.CompletedTask;
    }
}
