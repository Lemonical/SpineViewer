using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using System.Text.Json;

namespace SpineViewer.Infrastructure.Spine.Adapters;

/// <summary>
/// Probes and loads asset sets through the Spine 3.8.95 runtime lane.
/// </summary>
public sealed class Spine38RuntimeAdapter : ISpineRuntimeAdapter
{
    private static readonly SpineRuntimeDescriptor DescriptorInstance = new(
        "spine-3.8.95",
        "Spine 3.8.95",
        "3.8.x",
        new SpineRuntimeCapabilities(
            [
                new(SpineRuntimeFeature.JsonSkeleton, true),
                new(SpineRuntimeFeature.BinarySkeleton, true),
                new(SpineRuntimeFeature.Events, true),
                new(SpineRuntimeFeature.Clipping, false, "Clipping support is not yet guaranteed in the rewrite runtime lane."),
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
        string extension = Path.GetExtension(request.AssetFileSet.SkeletonPath);

        try
        {
            if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
            {
                await using FileStream stream = File.OpenRead(request.AssetFileSet.SkeletonPath);
                using JsonDocument _ = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                FileInfo fileInfo = new(request.AssetFileSet.SkeletonPath);
                if (fileInfo.Length == 0)
                {
                    return CreateFailureResult(
                        request,
                        "runtime-load-empty-binary-skeleton",
                        "The binary skeleton file is empty.",
                        $"Skeleton path: '{request.AssetFileSet.SkeletonPath}'.",
                        "Re-export the skeleton file and try again.");
                }
            }

            return new SpineLoadResult(
                true,
                request.ProjectReference,
                request.AssetFileSet,
                DescriptorInstance,
                request.VersionMatch,
                request.ProbeResult.UnsupportedFeatures,
                Array.Empty<ViewerDiagnostic>());
        }
        catch (JsonException exception)
        {
            return CreateFailureResult(
                request,
                "runtime-load-invalid-json-skeleton",
                "The skeleton JSON could not be parsed by the selected runtime.",
                exception.Message,
                "Verify that the skeleton file is valid JSON and was exported correctly.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return CreateFailureResult(
                request,
                "runtime-load-skeleton-read-failed",
                "The selected runtime could not read the skeleton file.",
                exception.Message,
                "Verify that the skeleton file exists and is accessible.");
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
}
