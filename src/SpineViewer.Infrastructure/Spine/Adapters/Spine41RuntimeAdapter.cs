using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using SpineViewer.Infrastructure.Spine.Loading;

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
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!request.ProbeResult.CanLoad)
        {
            return Task.FromResult(
                new SpineLoadResult(
                    false,
                    request.ProjectReference,
                    request.AssetFileSet,
                    Descriptor,
                    request.VersionMatch,
                    request.ProbeResult.UnsupportedFeatures,
                    request.ProbeResult.Diagnostics));
        }

        return LoadSkeletonAsync(request, cancellationToken);
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

    private static async Task<SpineLoadResult> LoadSkeletonAsync(
        SpineLoadRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<UnsupportedSpineFeature> unsupportedFeatures =
                LegacyRuntimeAssetLoader.ValidateLoad(DescriptorInstance.RuntimeId, request.AssetFileSet);

            return new SpineLoadResult(
                true,
                request.ProjectReference,
                request.AssetFileSet,
                DescriptorInstance,
                request.VersionMatch,
                unsupportedFeatures,
                Array.Empty<ViewerDiagnostic>());
        }
        catch (LegacyRuntimeAssetLoader.LoadException exception)
        {
            return CreateFailureResult(request, exception);
        }
    }

    private static SpineLoadResult CreateFailureResult(
        SpineLoadRequest request,
        string diagnosticCode,
        string message,
        string details,
        string suggestedAction)
    {
        ViewerDiagnostic diagnostic = new(
            diagnosticCode,
            ViewerDiagnosticSeverity.Error,
            message,
            DescriptorInstance.RuntimeId,
            details,
            suggestedAction);

        return new SpineLoadResult(
            false,
            request.ProjectReference,
            request.AssetFileSet,
            DescriptorInstance,
            request.VersionMatch,
            request.ProbeResult.UnsupportedFeatures,
            [diagnostic]);
    }

    private static SpineLoadResult CreateFailureResult(
        SpineLoadRequest request,
        LegacyRuntimeAssetLoader.LoadException exception)
    {
        string extension = Path.GetExtension(request.AssetFileSet.SkeletonPath);
        Exception detailsException = exception.InnerException ?? exception;

        return exception.Stage switch
        {
            LegacyRuntimeAssetLoader.LoadFailureStage.DependentTexture => CreateFailureResult(
                request,
                "runtime-load-missing-dependent-texture",
                "A texture referenced by the atlas could not be found.",
                detailsException.Message,
                "Verify that every atlas page texture exists next to the exported assets."),
            LegacyRuntimeAssetLoader.LoadFailureStage.Atlas when detailsException is IOException or UnauthorizedAccessException => CreateFailureResult(
                request,
                "runtime-load-atlas-read-failed",
                "The selected runtime could not read the atlas file.",
                detailsException.Message,
                "Verify that the atlas file exists and is accessible."),
            LegacyRuntimeAssetLoader.LoadFailureStage.Atlas => CreateFailureResult(
                request,
                "runtime-load-invalid-atlas",
                "The atlas could not be parsed by the selected runtime.",
                detailsException.Message,
                "Verify that the atlas file matches the exported skeleton data."),
            LegacyRuntimeAssetLoader.LoadFailureStage.Skeleton when detailsException is IOException or UnauthorizedAccessException => CreateFailureResult(
                request,
                "runtime-load-skeleton-read-failed",
                "The selected runtime could not read the skeleton file.",
                detailsException.Message,
                "Verify that the skeleton file exists and is accessible."),
            LegacyRuntimeAssetLoader.LoadFailureStage.Skeleton when string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase) => CreateFailureResult(
                request,
                "runtime-load-invalid-json-skeleton",
                "The skeleton JSON could not be parsed by the selected runtime.",
                detailsException.Message,
                "Verify that the skeleton file is valid JSON and was exported correctly."),
            _ => CreateFailureResult(
                request,
                "runtime-load-invalid-binary-skeleton",
                "The binary skeleton could not be parsed by the selected runtime.",
                detailsException.Message,
                "Verify that the binary skeleton matches the selected runtime family and was exported correctly."),
        };
    }
}
