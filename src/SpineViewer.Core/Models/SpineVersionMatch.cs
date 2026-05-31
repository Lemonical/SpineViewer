using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the version-detection outcome for a Spine asset set.
/// </summary>
public sealed record SpineVersionMatch
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineVersionMatch"/> class.
    /// </summary>
    /// <param name="detectedExportVersion">The detected export version, if one could be determined.</param>
    /// <param name="suggestedRuntimeId">The preferred runtime identifier, if one could be suggested.</param>
    /// <param name="isExactMatch">Indicates whether the suggested runtime is an exact version match.</param>
    /// <param name="diagnostics">The diagnostics produced during version detection.</param>
    public SpineVersionMatch(
        string? detectedExportVersion,
        string? suggestedRuntimeId,
        bool isExactMatch,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        DetectedExportVersion = Guard.NullIfWhiteSpace(detectedExportVersion);
        SuggestedRuntimeId = Guard.NullIfWhiteSpace(suggestedRuntimeId);

        if (isExactMatch && SuggestedRuntimeId is null)
        {
            throw new ArgumentException(
                "An exact match requires a suggested runtime identifier.",
                nameof(suggestedRuntimeId));
        }

        IsExactMatch = isExactMatch;
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets the detected export version, if one could be determined.
    /// </summary>
    public string? DetectedExportVersion { get; init; }

    /// <summary>
    /// Gets the preferred runtime identifier, if one could be suggested.
    /// </summary>
    public string? SuggestedRuntimeId { get; init; }

    /// <summary>
    /// Gets a value indicating whether the suggested runtime is an exact version match.
    /// </summary>
    public bool IsExactMatch { get; init; }

    /// <summary>
    /// Gets the diagnostics produced during version detection.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
