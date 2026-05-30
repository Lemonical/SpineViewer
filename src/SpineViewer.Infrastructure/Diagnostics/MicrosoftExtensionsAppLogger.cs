using Microsoft.Extensions.Logging;
using SpineViewer.Core.Abstractions;

namespace SpineViewer.Infrastructure.Diagnostics;

/// <summary>
/// Adapts <see cref="ILogger{TCategoryName}"/> to the core logging abstraction.
/// </summary>
/// <typeparam name="TCategory">The consuming type category.</typeparam>
public sealed class MicrosoftExtensionsAppLogger<TCategory> : IAppLogger<TCategory>
{
    private readonly ILogger<TCategory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftExtensionsAppLogger{TCategory}"/> class.
    /// </summary>
    /// <param name="logger">The underlying Microsoft.Extensions logger.</param>
    public MicrosoftExtensionsAppLogger(ILogger<TCategory> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void LogInformation(string message)
    {
        _logger.LogInformation("{Message}", message);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        _logger.LogWarning("{Message}", message);
    }

    /// <inheritdoc />
    public void LogError(string message, Exception exception)
    {
        _logger.LogError(exception, "{Message}", message);
    }
}
