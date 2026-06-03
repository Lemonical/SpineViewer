using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the outcome of probing one runtime adapter against a resolved asset set.
/// </summary>
public sealed record SpineRuntimeProbeResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimeProbeResult"/> class.
    /// </summary>
    /// <param name="runtime">The runtime that produced the probe result.</param>
    /// <param name="supportStatus">The runtime's support status for the asset set.</param>
    /// <param name="unsupportedFeatures">The unsupported features discovered during the probe.</param>
    /// <param name="diagnostics">The diagnostics produced during the probe.</param>
    public SpineRuntimeProbeResult(
        SpineRuntimeDescriptor runtime,
        SpineRuntimeSupportStatus supportStatus,
        IEnumerable<UnsupportedSpineFeature> unsupportedFeatures,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        Runtime = Guard.NotNull(runtime, nameof(runtime));
        SupportStatus = supportStatus;
        UnsupportedFeatures = Guard.MaterializeReadOnlyList(
            unsupportedFeatures,
            nameof(unsupportedFeatures));
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));

        if (supportStatus == SpineRuntimeSupportStatus.SupportedWithWarnings && UnsupportedFeatures.Count == 0 && Diagnostics.Count == 0)
        {
            throw new ArgumentException(
                "A warning probe result must include unsupported features or diagnostics.",
                nameof(unsupportedFeatures));
        }

        if (supportStatus == SpineRuntimeSupportStatus.Supported && UnsupportedFeatures.Count > 0)
        {
            throw new ArgumentException(
                "A fully supported probe result cannot report unsupported features.",
                nameof(unsupportedFeatures));
        }
    }

    /// <summary>
    /// Gets the runtime that produced the probe result.
    /// </summary>
    public SpineRuntimeDescriptor Runtime { get; init; }

    /// <summary>
    /// Gets the runtime's support status for the asset set.
    /// </summary>
    public SpineRuntimeSupportStatus SupportStatus { get; init; }

    /// <summary>
    /// Gets the unsupported features discovered during the probe.
    /// </summary>
    public IReadOnlyList<UnsupportedSpineFeature> UnsupportedFeatures { get; init; }

    /// <summary>
    /// Gets the diagnostics produced during the probe.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }

    /// <summary>
    /// Gets a value indicating whether the runtime can attempt a load.
    /// </summary>
    public bool CanLoad => SupportStatus != SpineRuntimeSupportStatus.Unsupported;
}
