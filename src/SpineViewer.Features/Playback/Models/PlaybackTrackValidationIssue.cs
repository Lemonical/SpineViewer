using SpineViewer.Core.Models;

namespace SpineViewer.Features.Playback.Models;

/// <summary>
/// Represents one user-facing track configuration issue surfaced by the track editor.
/// </summary>
public sealed record PlaybackTrackValidationIssue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaybackTrackValidationIssue"/> class.
    /// </summary>
    /// <param name="code">The stable issue code.</param>
    /// <param name="severity">The issue severity.</param>
    /// <param name="message">The issue message.</param>
    public PlaybackTrackValidationIssue(
        string code,
        ViewerDiagnosticSeverity severity,
        string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(code));
        }

        Code = code;
        Severity = severity;
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
        }

        Message = message;
    }

    /// <summary>
    /// Gets the stable issue code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the issue severity.
    /// </summary>
    public ViewerDiagnosticSeverity Severity { get; init; }

    /// <summary>
    /// Gets the issue message.
    /// </summary>
    public string Message { get; init; }
}
