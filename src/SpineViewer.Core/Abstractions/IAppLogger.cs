namespace SpineViewer.Core.Abstractions;

/// <summary>
/// Provides application-facing logging without coupling core code to a concrete logging library.
/// </summary>
/// <typeparam name="TCategory">The category associated with the consuming type.</typeparam>
public interface IAppLogger<TCategory>
{
    /// <summary>
    /// Writes an informational log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogInformation(string message);

    /// <summary>
    /// Writes a warning log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Writes an error log entry.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception associated with the failure.</param>
    void LogError(string message, Exception exception);
}
