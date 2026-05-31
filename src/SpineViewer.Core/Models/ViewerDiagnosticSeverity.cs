namespace SpineViewer.Core.Models;

/// <summary>
/// Describes how urgently a diagnostic should be presented to the user.
/// </summary>
public enum ViewerDiagnosticSeverity
{
    /// <summary>
    /// Provides informative context that does not block the current workflow.
    /// </summary>
    Information,

    /// <summary>
    /// Indicates a recoverable concern or compatibility risk.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates a load-blocking or session-blocking failure.
    /// </summary>
    Error,
}
