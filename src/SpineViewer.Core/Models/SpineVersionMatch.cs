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
    /// <param name="compatibleRuntimeIds">The runtime identifiers that appear compatible with the detected export version.</param>
    /// <param name="isExactMatch">Indicates whether the suggested runtime is an exact version match.</param>
    /// <param name="diagnostics">The diagnostics produced during version detection.</param>
    public SpineVersionMatch(
        string? detectedExportVersion,
        string? suggestedRuntimeId,
        IEnumerable<string> compatibleRuntimeIds,
        bool isExactMatch,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        DetectedExportVersion = Guard.NullIfWhiteSpace(detectedExportVersion);
        SuggestedRuntimeId = Guard.NullIfWhiteSpace(suggestedRuntimeId);
        CompatibleRuntimeIds = MaterializeCompatibleRuntimeIds(compatibleRuntimeIds);

        if (isExactMatch && SuggestedRuntimeId is null)
        {
            throw new ArgumentException(
                "An exact match requires a suggested runtime identifier.",
                nameof(suggestedRuntimeId));
        }

        if (SuggestedRuntimeId is not null &&
            CompatibleRuntimeIds.Count > 0 &&
            !CompatibleRuntimeIds.Contains(SuggestedRuntimeId, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The suggested runtime identifier must be included in the compatible runtime list when candidates are supplied.",
                nameof(compatibleRuntimeIds));
        }

        IsExactMatch = isExactMatch;
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

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
        : this(
            detectedExportVersion,
            suggestedRuntimeId,
            BuildCompatibleRuntimeIdsFromSuggestion(suggestedRuntimeId),
            isExactMatch,
            diagnostics)
    { }

    /// <summary>
    /// Gets the detected export version, if one could be determined.
    /// </summary>
    public string? DetectedExportVersion { get; init; }

    /// <summary>
    /// Gets the preferred runtime identifier, if one could be suggested.
    /// </summary>
    public string? SuggestedRuntimeId { get; init; }

    /// <summary>
    /// Gets the runtime identifiers that appear compatible with the detected export version.
    /// </summary>
    public IReadOnlyList<string> CompatibleRuntimeIds { get; init; }

    /// <summary>
    /// Gets a value indicating whether the suggested runtime is an exact version match.
    /// </summary>
    public bool IsExactMatch { get; init; }

    /// <summary>
    /// Gets the diagnostics produced during version detection.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }

    private static IReadOnlyList<string> MaterializeCompatibleRuntimeIds(
        IEnumerable<string> compatibleRuntimeIds)
    {
        return Guard.MaterializeNonEmptyStrings(compatibleRuntimeIds, nameof(compatibleRuntimeIds))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> BuildCompatibleRuntimeIdsFromSuggestion(string? suggestedRuntimeId)
    {
        string? normalizedRuntimeId = Guard.NullIfWhiteSpace(suggestedRuntimeId);
        return normalizedRuntimeId is null ? Array.Empty<string>() : [normalizedRuntimeId];
    }
}
