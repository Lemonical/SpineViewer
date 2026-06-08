using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Infrastructure.Spine.Adapters;

/// <summary>
/// Probes and loads asset sets through the Spine 4.1.00 runtime lane.
/// </summary>
public sealed class Spine41RuntimeAdapter : ISpineRuntimeAdapter
{
    private static readonly SpineRuntimeDescriptor DescriptorInstance = new(
        "spine-4.1.00",
        "Spine 4.1.00",
        "4.1.x",
        new SpineRuntimeCapabilities(
            [
                new(SpineRuntimeFeature.JsonSkeleton, true),
                new(SpineRuntimeFeature.BinarySkeleton, true),
                new(SpineRuntimeFeature.Events, true),
                new(SpineRuntimeFeature.Clipping, true),
                new(SpineRuntimeFeature.Meshes, true),
                new(SpineRuntimeFeature.MultipleTracks, true),
            ]));

    /// <inheritdoc />
    public SpineRuntimeDescriptor Descriptor => DescriptorInstance;

    /// <inheritdoc />
    public Task<SpineRuntimeProbeResult> ProbeAsync(
        SpineAssetFileSet assetFileSet,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(BuildProbeResult(assetFileSet));
    }

    /// <inheritdoc />
    public Task<SpineLoadResult> LoadAsync(
        SpineLoadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ViewerDiagnostic diagnostic = new(
            "runtime-load-not-implemented",
            ViewerDiagnosticSeverity.Warning,
            "Project loading is not implemented for this runtime yet.",
            Descriptor.RuntimeId,
            "The runtime adapter can be selected and probed, but the concrete load path is not available yet.",
            "Try another runtime only if the asset version clearly matches it.");

        SpineLoadResult result = new(
            false,
            request.ProjectReference,
            request.AssetFileSet,
            Descriptor,
            request.VersionMatch,
            request.ProbeResult.UnsupportedFeatures,
            [diagnostic]);

        return Task.FromResult(result);
    }

    private SpineRuntimeProbeResult BuildProbeResult(SpineAssetFileSet assetFileSet)
    {
        string extension = Path.GetExtension(assetFileSet.SkeletonPath);

        if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase) &&
            Descriptor.Capabilities.Supports(SpineRuntimeFeature.JsonSkeleton))
        {
            return new SpineRuntimeProbeResult(
                Descriptor,
                SpineRuntimeSupportStatus.Supported,
                Array.Empty<UnsupportedSpineFeature>(),
                Array.Empty<ViewerDiagnostic>());
        }

        if ((string.Equals(extension, ".skel", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(extension, ".bytes", StringComparison.OrdinalIgnoreCase)) &&
            Descriptor.Capabilities.Supports(SpineRuntimeFeature.BinarySkeleton))
        {
            return new SpineRuntimeProbeResult(
                Descriptor,
                SpineRuntimeSupportStatus.Supported,
                Array.Empty<UnsupportedSpineFeature>(),
                Array.Empty<ViewerDiagnostic>());
        }

        UnsupportedSpineFeature unsupportedFeature = new(
            "skeleton-data-format",
            "Skeleton data format",
            $"The '{extension}' skeleton file extension is not supported by {Descriptor.DisplayName}.");

        ViewerDiagnostic diagnostic = new(
            "runtime-probe-unsupported-skeleton-format",
            ViewerDiagnosticSeverity.Error,
            "The selected runtime cannot open this skeleton data format.",
            Descriptor.RuntimeId,
            $"The skeleton file '{assetFileSet.SkeletonPath}' uses the '{extension}' extension.",
            "Choose a runtime that supports the asset's skeleton data format.");

        return new SpineRuntimeProbeResult(
            Descriptor,
            SpineRuntimeSupportStatus.Unsupported,
            [unsupportedFeature],
            [diagnostic]);
    }
}
