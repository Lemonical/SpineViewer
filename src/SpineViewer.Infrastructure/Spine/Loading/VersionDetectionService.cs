using System.Text.Json;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Detects the most likely Spine export version for a resolved asset file set.
/// </summary>
public sealed class VersionDetectionService : IVersionDetectionService
{
    private static readonly string[] FallbackRuntimeIds =
    [
        "spine-4.1.00",
        "spine-3.8.95",
    ];

    /// <inheritdoc />
    public async Task<SpineVersionMatch> DetectAsync(
        SpineAssetFileSet assetFileSet,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(assetFileSet);
        cancellationToken.ThrowIfCancellationRequested();

        string extension = Path.GetExtension(assetFileSet.SkeletonPath);

        if (string.Equals(extension, ".json", StringComparison.OrdinalIgnoreCase))
        {
            return await DetectFromJsonAsync(assetFileSet.SkeletonPath, cancellationToken).ConfigureAwait(false);
        }

        if (string.Equals(extension, ".skel", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(extension, ".bytes", StringComparison.OrdinalIgnoreCase))
        {
            return DetectFromBinary(assetFileSet.SkeletonPath);
        }

        ViewerDiagnostic diagnostic = new(
            "version-detection-unsupported-skeleton-format",
            ViewerDiagnosticSeverity.Error,
            "The skeleton file format is not supported for version detection.",
            nameof(VersionDetectionService),
            $"Skeleton path: '{assetFileSet.SkeletonPath}'.",
            "Choose a supported .json, .skel, or .bytes skeleton file.");

        return new SpineVersionMatch(null, null, Array.Empty<string>(), false, [diagnostic]);
    }

    private static async Task<SpineVersionMatch> DetectFromJsonAsync(
        string skeletonPath,
        CancellationToken cancellationToken)
    {
        try
        {
            await using FileStream stream = File.OpenRead(skeletonPath);
            JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (!document.RootElement.TryGetProperty("skeleton", out JsonElement skeletonElement) ||
                skeletonElement.ValueKind != JsonValueKind.Object ||
                !skeletonElement.TryGetProperty("spine", out JsonElement versionElement) ||
                versionElement.ValueKind != JsonValueKind.String)
            {
                ViewerDiagnostic missingVersionDiagnostic = new(
                    "version-detection-export-version-missing",
                    ViewerDiagnosticSeverity.Warning,
                    "The skeleton JSON does not declare a Spine export version.",
                    nameof(VersionDetectionService),
                    $"Skeleton path: '{skeletonPath}'.",
                    "The project can still be probed, but runtime selection may require confirmation.");

                return new SpineVersionMatch(
                    null,
                    null,
                    FallbackRuntimeIds,
                    false,
                    [missingVersionDiagnostic]);
            }

            string? detectedVersion = versionElement.GetString();
            return BuildVersionMatch(detectedVersion, skeletonPath);
        }
        catch (JsonException exception)
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-invalid-json-skeleton",
                ViewerDiagnosticSeverity.Error,
                "The skeleton JSON could not be parsed.",
                nameof(VersionDetectionService),
                exception.Message,
                "Verify that the skeleton file is valid JSON and was exported correctly.");

            return new SpineVersionMatch(null, null, Array.Empty<string>(), false, [diagnostic]);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-skeleton-read-failed",
                ViewerDiagnosticSeverity.Error,
                "The skeleton file could not be read during version detection.",
                nameof(VersionDetectionService),
                exception.Message,
                "Verify that the skeleton file exists and is accessible.");

            return new SpineVersionMatch(null, null, Array.Empty<string>(), false, [diagnostic]);
        }
    }

    private static SpineVersionMatch DetectFromBinary(string skeletonPath)
    {
        try
        {
            FileInfo fileInfo = new(skeletonPath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                ViewerDiagnostic emptyFileDiagnostic = new(
                    "version-detection-empty-binary-skeleton",
                    ViewerDiagnosticSeverity.Error,
                    "The binary skeleton file is empty.",
                    nameof(VersionDetectionService),
                    $"Skeleton path: '{skeletonPath}'.",
                    "Re-export the skeleton file and try again.");

                return new SpineVersionMatch(null, null, Array.Empty<string>(), false, [emptyFileDiagnostic]);
            }

            ViewerDiagnostic fallbackDiagnostic = new(
                "version-detection-binary-heuristic",
                ViewerDiagnosticSeverity.Warning,
                "Binary skeleton version detection is using compatibility probing rather than an exact export version.",
                nameof(VersionDetectionService),
                $"Skeleton path: '{skeletonPath}'.",
                "Review the selected runtime before continuing if the asset's export version matters.");

            return new SpineVersionMatch(null, null, FallbackRuntimeIds, false, [fallbackDiagnostic]);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-binary-read-failed",
                ViewerDiagnosticSeverity.Error,
                "The binary skeleton file could not be read during version detection.",
                nameof(VersionDetectionService),
                exception.Message,
                "Verify that the skeleton file exists and is accessible.");

            return new SpineVersionMatch(null, null, Array.Empty<string>(), false, [diagnostic]);
        }
    }

    private static SpineVersionMatch BuildVersionMatch(string? detectedVersion, string skeletonPath)
    {
        if (string.IsNullOrWhiteSpace(detectedVersion))
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-export-version-empty",
                ViewerDiagnosticSeverity.Warning,
                "The skeleton JSON declared an empty Spine export version.",
                nameof(VersionDetectionService),
                $"Skeleton path: '{skeletonPath}'.",
                "The project can still be probed, but runtime selection may require confirmation.");

            return new SpineVersionMatch(null, null, FallbackRuntimeIds, false, [diagnostic]);
        }

        if (string.Equals(detectedVersion, "4.1.00", StringComparison.OrdinalIgnoreCase))
        {
            return new SpineVersionMatch(detectedVersion, "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>());
        }

        if (string.Equals(detectedVersion, "3.8.95", StringComparison.OrdinalIgnoreCase))
        {
            return new SpineVersionMatch(detectedVersion, "spine-3.8.95", true, Array.Empty<ViewerDiagnostic>());
        }

        if (detectedVersion.StartsWith("4.1.", StringComparison.OrdinalIgnoreCase))
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-compatible-runtime-family",
                ViewerDiagnosticSeverity.Warning,
                "The project appears to target the Spine 4.1 runtime family, but not the exact shipped runtime version.",
                nameof(VersionDetectionService),
                $"Detected export version: '{detectedVersion}'.",
                "Review the compatibility warning before loading.");

            return new SpineVersionMatch(detectedVersion, "spine-4.1.00", false, [diagnostic]);
        }

        if (detectedVersion.StartsWith("3.8.", StringComparison.OrdinalIgnoreCase))
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-compatible-runtime-family",
                ViewerDiagnosticSeverity.Warning,
                "The project appears to target the Spine 3.8 runtime family, but not the exact shipped runtime version.",
                nameof(VersionDetectionService),
                $"Detected export version: '{detectedVersion}'.",
                "Review the compatibility warning before loading.");

            return new SpineVersionMatch(detectedVersion, "spine-3.8.95", false, [diagnostic]);
        }

        if (detectedVersion.StartsWith("4.", StringComparison.OrdinalIgnoreCase))
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-unsupported-runtime-family",
                ViewerDiagnosticSeverity.Warning,
                "The project targets a Spine 4.x runtime family that is outside the exact release baseline.",
                nameof(VersionDetectionService),
                $"Detected export version: '{detectedVersion}'.",
                "The loader will try the nearest supported runtime family, but compatibility is not guaranteed.");

            return new SpineVersionMatch(
                detectedVersion,
                "spine-4.1.00",
                ["spine-4.1.00"],
                false,
                [diagnostic]);
        }

        if (detectedVersion.StartsWith("3.", StringComparison.OrdinalIgnoreCase))
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-unsupported-runtime-family",
                ViewerDiagnosticSeverity.Warning,
                "The project targets a Spine 3.x runtime family that is outside the exact release baseline.",
                nameof(VersionDetectionService),
                $"Detected export version: '{detectedVersion}'.",
                "The loader will try the nearest supported runtime family, but compatibility is not guaranteed.");

            return new SpineVersionMatch(
                detectedVersion,
                "spine-3.8.95",
                ["spine-3.8.95"],
                false,
                [diagnostic]);
        }

        ViewerDiagnostic unsupportedDiagnostic = new(
            "version-detection-unknown-export-version",
            ViewerDiagnosticSeverity.Warning,
            "The project export version does not map to a supported runtime family.",
            nameof(VersionDetectionService),
            $"Detected export version: '{detectedVersion}'.",
            "The viewer will rely on compatibility probing and may not be able to load the project.");

        return new SpineVersionMatch(detectedVersion, null, FallbackRuntimeIds, false, [unsupportedDiagnostic]);
    }
}
