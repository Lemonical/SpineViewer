namespace SpineViewer.Core.Models;

/// <summary>
/// Describes the runtime features a runtime adapter can support.
/// </summary>
public sealed record SpineRuntimeCapabilities
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimeCapabilities"/> class.
    /// </summary>
    /// <param name="featureSupport">The feature support metadata exposed by the runtime.</param>
    public SpineRuntimeCapabilities(IEnumerable<SpineRuntimeFeatureSupport> featureSupport)
    {
        FeatureSupport = BuildFeatureSupport(featureSupport);
    }

    /// <summary>
    /// Gets the feature support metadata exposed by the runtime.
    /// </summary>
    public IReadOnlyList<SpineRuntimeFeatureSupport> FeatureSupport { get; init; }

    /// <summary>
    /// Gets the feature support entry for the requested runtime feature.
    /// </summary>
    /// <param name="feature">The runtime feature to inspect.</param>
    /// <returns>The matching support entry.</returns>
    public SpineRuntimeFeatureSupport GetSupport(SpineRuntimeFeature feature)
    {
        foreach (SpineRuntimeFeatureSupport support in FeatureSupport)
        {
            if (support.Feature == feature)
            {
                return support;
            }
        }

        return new SpineRuntimeFeatureSupport(feature, false);
    }

    /// <summary>
    /// Gets a value indicating whether the runtime supports the requested feature.
    /// </summary>
    /// <param name="feature">The runtime feature to inspect.</param>
    /// <returns><see langword="true"/> when the feature is supported; otherwise, <see langword="false"/>.</returns>
    public bool Supports(SpineRuntimeFeature feature)
    {
        return GetSupport(feature).IsSupported;
    }

    private static IReadOnlyList<SpineRuntimeFeatureSupport> BuildFeatureSupport(
        IEnumerable<SpineRuntimeFeatureSupport> featureSupport)
    {
        ArgumentNullException.ThrowIfNull(featureSupport);

        Dictionary<SpineRuntimeFeature, SpineRuntimeFeatureSupport> supportByFeature = [];

        foreach (SpineRuntimeFeatureSupport support in featureSupport)
        {
            if (!supportByFeature.TryAdd(support.Feature, support))
            {
                throw new ArgumentException(
                    $"Duplicate feature support metadata was provided for '{support.Feature}'.",
                    nameof(featureSupport));
            }
        }

        return supportByFeature.Values
            .OrderBy(static support => support.Feature)
            .ToArray();
    }
}
