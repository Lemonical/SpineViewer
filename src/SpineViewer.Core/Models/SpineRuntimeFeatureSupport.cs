using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Describes whether a runtime supports one viewer-relevant feature.
/// </summary>
public sealed record SpineRuntimeFeatureSupport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimeFeatureSupport"/> class.
    /// </summary>
    /// <param name="feature">The feature described by this support entry.</param>
    /// <param name="isSupported">Indicates whether the feature is supported.</param>
    /// <param name="notes">Optional notes about the support level.</param>
    public SpineRuntimeFeatureSupport(
        SpineRuntimeFeature feature,
        bool isSupported,
        string? notes = null)
    {
        Feature = feature;
        IsSupported = isSupported;
        Notes = Guard.NullIfWhiteSpace(notes);
    }

    /// <summary>
    /// Gets the feature described by this support entry.
    /// </summary>
    public SpineRuntimeFeature Feature { get; init; }

    /// <summary>
    /// Gets a value indicating whether the feature is supported.
    /// </summary>
    public bool IsSupported { get; init; }

    /// <summary>
    /// Gets optional notes about the support level.
    /// </summary>
    public string? Notes { get; init; }
}
