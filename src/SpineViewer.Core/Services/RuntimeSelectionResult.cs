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
    /// <param name="selectedProbeResult">The runtime probe result selected for loading, if any.</param>
    /// <param name="isExactVersionMatch">Indicates whether the selected runtime is an exact version match.</param>
    /// <param name="requiresUserConfirmation">Indicates whether the selection should be confirmed by the user.</param>
    /// <param name="diagnostics">The diagnostics produced during selection.</param>
    public RuntimeSelectionResult(
        SpineRuntimeProbeResult? selectedProbeResult,
        bool isExactVersionMatch,
        bool requiresUserConfirmation,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        SelectedProbeResult = selectedProbeResult;
        IsExactVersionMatch = isExactVersionMatch;
        RequiresUserConfirmation = requiresUserConfirmation;
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));

        if (IsExactVersionMatch && SelectedProbeResult is null)
        {
            throw new ArgumentException(
                "An exact version match requires a selected runtime probe result.",
                nameof(selectedProbeResult));
        }
    }

    /// <summary>
    /// Gets the runtime probe result selected for loading, if any.
    /// </summary>
    public SpineRuntimeProbeResult? SelectedProbeResult { get; init; }

    /// <summary>
    /// Gets the runtime selected for loading, if any.
    /// </summary>
    public SpineRuntimeDescriptor? SelectedRuntime => SelectedProbeResult?.Runtime;

    /// <summary>
    /// Gets a value indicating whether the selected runtime is an exact version match.
    /// </summary>
    public bool IsExactVersionMatch { get; init; }

    /// <summary>
    /// Gets a value indicating whether the selection should be confirmed by the user.
    /// </summary>
    public bool RequiresUserConfirmation { get; init; }

    /// <summary>
    /// Gets the unsupported features reported for the selected runtime.
    /// </summary>
    public IReadOnlyList<UnsupportedSpineFeature> UnsupportedFeatures =>
        SelectedProbeResult?.UnsupportedFeatures ?? Array.Empty<UnsupportedSpineFeature>();

    /// <summary>
    /// Gets the diagnostics produced during selection.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
