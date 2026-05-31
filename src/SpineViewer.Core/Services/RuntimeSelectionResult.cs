using SpineViewer.Core.Models;
using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Services;

/// <summary>
/// Represents the runtime-selection outcome for a Spine project.
/// </summary>
public sealed record RuntimeSelectionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeSelectionResult"/> class.
    /// </summary>
    /// <param name="selectedRuntime">The runtime selected for loading, if any.</param>
    /// <param name="requiresUserConfirmation">Indicates whether the selection should be confirmed by the user.</param>
    /// <param name="diagnostics">The diagnostics produced during selection.</param>
    public RuntimeSelectionResult(
        SpineRuntimeDescriptor? selectedRuntime,
        bool requiresUserConfirmation,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        SelectedRuntime = selectedRuntime;
        RequiresUserConfirmation = requiresUserConfirmation;
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets the runtime selected for loading, if any.
    /// </summary>
    public SpineRuntimeDescriptor? SelectedRuntime { get; init; }

    /// <summary>
    /// Gets a value indicating whether the selection should be confirmed by the user.
    /// </summary>
    public bool RequiresUserConfirmation { get; init; }

    /// <summary>
    /// Gets the diagnostics produced during selection.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
