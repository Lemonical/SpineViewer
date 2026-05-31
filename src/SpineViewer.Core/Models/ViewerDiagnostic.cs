using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents one user-facing diagnostic surfaced by the viewer.
/// </summary>
public sealed record ViewerDiagnostic
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerDiagnostic"/> class.
    /// </summary>
    /// <param name="code">The stable diagnostic code.</param>
    /// <param name="severity">The severity of the diagnostic.</param>
    /// <param name="message">The user-facing summary message.</param>
    /// <param name="source">The subsystem or artifact that produced the diagnostic.</param>
    /// <param name="details">Optional technical details for deeper inspection.</param>
    /// <param name="suggestedAction">Optional recovery guidance for the user.</param>
    public ViewerDiagnostic(
        string code,
        ViewerDiagnosticSeverity severity,
        string message,
        string source,
        string? details = null,
        string? suggestedAction = null)
    {
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code));
        Severity = severity;
        Message = Guard.NotNullOrWhiteSpace(message, nameof(message));
        Source = Guard.NotNullOrWhiteSpace(source, nameof(source));
        Details = Guard.NullIfWhiteSpace(details);
        SuggestedAction = Guard.NullIfWhiteSpace(suggestedAction);
    }

    /// <summary>
    /// Gets the stable diagnostic code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the severity of the diagnostic.
    /// </summary>
    public ViewerDiagnosticSeverity Severity { get; init; }

    /// <summary>
    /// Gets the user-facing summary message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the subsystem or artifact that produced the diagnostic.
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    /// Gets optional technical details for deeper inspection.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Gets optional recovery guidance for the user.
    /// </summary>
    public string? SuggestedAction { get; init; }
}
