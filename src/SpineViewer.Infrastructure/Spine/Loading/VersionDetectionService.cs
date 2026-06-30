using System.Text.Json;
using System.Text.RegularExpressions;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Detects the most likely Spine export version for a resolved asset file set.
/// </summary>
public sealed class VersionDetectionService : IVersionDetectionService
{
    private static readonly Regex VersionPattern = new(
        @"(?<version>\d+\.\d+\.\d+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly ISpineRuntimeCatalog _runtimeCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionDetectionService"/> class.
    /// </summary>
    /// <param name="runtimeCatalog">The runtime catalog used to map detected exports to registered adapters.</param>
    public VersionDetectionService(ISpineRuntimeCatalog runtimeCatalog)
    {
        _runtimeCatalog = runtimeCatalog ?? throw new ArgumentNullException(nameof(runtimeCatalog));
    }

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
            return await DetectFromBinaryAsync(assetFileSet.SkeletonPath, cancellationToken).ConfigureAwait(false);
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

    private async Task<SpineVersionMatch> DetectFromJsonAsync(
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
                    BuildFallbackRuntimeIds(),
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

    private async Task<SpineVersionMatch> DetectFromBinaryAsync(
        string skeletonPath,
        CancellationToken cancellationToken)
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

            string? detectedVersion = await TryDetectBinaryVersionAsync(skeletonPath, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(detectedVersion))
            {
                return BuildVersionMatch(detectedVersion, skeletonPath);
            }

            ViewerDiagnostic fallbackDiagnostic = new(
                "version-detection-binary-heuristic",
                ViewerDiagnosticSeverity.Warning,
                "Binary skeleton version detection is using compatibility probing rather than an exact export version.",
                nameof(VersionDetectionService),
                $"Skeleton path: '{skeletonPath}'.",
                "Review the selected runtime before continuing if the asset's export version matters.");

            return new SpineVersionMatch(null, null, BuildFallbackRuntimeIds(), false, [fallbackDiagnostic]);
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

    private SpineVersionMatch BuildVersionMatch(string? detectedVersion, string skeletonPath)
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

            return new SpineVersionMatch(null, null, BuildFallbackRuntimeIds(), false, [diagnostic]);
        }

        IReadOnlyList<SpineRuntimeDescriptor> orderedRuntimes = GetOrderedRuntimes(detectedVersion);
        IReadOnlyList<string> compatibleRuntimeIds = orderedRuntimes
            .Select(static runtime => runtime.RuntimeId)
            .ToArray();

        if (TryFindExactRuntimeMatch(orderedRuntimes, detectedVersion, out SpineRuntimeDescriptor? exactRuntime) &&
            exactRuntime is not null)
        {
            return new SpineVersionMatch(
                detectedVersion,
                exactRuntime.RuntimeId,
                compatibleRuntimeIds,
                true,
                Array.Empty<ViewerDiagnostic>());
        }

        if (TryFindMinorFamilyRuntimeMatch(orderedRuntimes, detectedVersion, out SpineRuntimeDescriptor? compatibleRuntime) &&
            compatibleRuntime is not null)
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-compatible-runtime-family",
                ViewerDiagnosticSeverity.Warning,
                "The project appears to target a supported Spine runtime family, but not the exact shipped runtime version.",
                nameof(VersionDetectionService),
                $"Detected export version: '{detectedVersion}'. Selected runtime family: '{compatibleRuntime.SupportedExportRange}'.",
                "Review the compatibility warning before loading.");

            return new SpineVersionMatch(
                detectedVersion,
                compatibleRuntime.RuntimeId,
                compatibleRuntimeIds,
                false,
                [diagnostic]);
        }

        if (TryFindMajorFamilyRuntimeMatch(orderedRuntimes, detectedVersion, out SpineRuntimeDescriptor? majorFamilyRuntime) &&
            majorFamilyRuntime is not null)
        {
            ViewerDiagnostic diagnostic = new(
                "version-detection-unsupported-runtime-family",
                ViewerDiagnosticSeverity.Warning,
                "The project targets a Spine runtime family that is outside the exact release baseline.",
                nameof(VersionDetectionService),
                $"Detected export version: '{detectedVersion}'. Selected runtime family: '{majorFamilyRuntime.SupportedExportRange}'.",
                "The loader will try the nearest supported runtime family, but compatibility is not guaranteed.");

            return new SpineVersionMatch(
                detectedVersion,
                majorFamilyRuntime.RuntimeId,
                compatibleRuntimeIds,
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

        return new SpineVersionMatch(detectedVersion, null, compatibleRuntimeIds, false, [unsupportedDiagnostic]);
    }

    private IReadOnlyList<string> BuildFallbackRuntimeIds()
    {
        return _runtimeCatalog
            .GetAvailableRuntimes()
            .Select(static runtime => runtime.RuntimeId)
            .ToArray();
    }

    private static bool TryFindExactRuntimeMatch(
        IEnumerable<SpineRuntimeDescriptor> runtimes,
        string detectedVersion,
        out SpineRuntimeDescriptor? runtime)
    {
        runtime = runtimes.FirstOrDefault(
            candidate =>
                TryGetRuntimeVersion(candidate, out ParsedVersion runtimeVersion) &&
                TryParseVersion(detectedVersion, out ParsedVersion detectedVersionParts) &&
                runtimeVersion.Major == detectedVersionParts.Major &&
                runtimeVersion.Minor == detectedVersionParts.Minor &&
                runtimeVersion.Patch.HasValue &&
                runtimeVersion.Patch == detectedVersionParts.Patch);
        return runtime is not null;
    }

    private static bool TryFindMinorFamilyRuntimeMatch(
        IEnumerable<SpineRuntimeDescriptor> runtimes,
        string detectedVersion,
        out SpineRuntimeDescriptor? runtime)
    {
        runtime = runtimes.FirstOrDefault(
            candidate =>
                TryGetRuntimeVersion(candidate, out ParsedVersion runtimeVersion) &&
                TryParseVersion(detectedVersion, out ParsedVersion detectedVersionParts) &&
                runtimeVersion.Major == detectedVersionParts.Major &&
                runtimeVersion.Minor == detectedVersionParts.Minor);
        return runtime is not null;
    }

    private static bool TryFindMajorFamilyRuntimeMatch(
        IEnumerable<SpineRuntimeDescriptor> runtimes,
        string detectedVersion,
        out SpineRuntimeDescriptor? runtime)
    {
        runtime = runtimes.FirstOrDefault(
            candidate =>
                TryGetRuntimeVersion(candidate, out ParsedVersion runtimeVersion) &&
                TryParseVersion(detectedVersion, out ParsedVersion detectedVersionParts) &&
                runtimeVersion.Major == detectedVersionParts.Major);
        return runtime is not null;
    }

    private IReadOnlyList<SpineRuntimeDescriptor> GetOrderedRuntimes(string detectedVersion)
    {
        List<(SpineRuntimeDescriptor Runtime, int Index)> indexedRuntimes = _runtimeCatalog
            .GetAvailableRuntimes()
            .Select((runtime, index) => (runtime, index))
            .ToList();

        if (!TryParseVersion(detectedVersion, out ParsedVersion detectedVersionParts))
        {
            return indexedRuntimes
                .Select(static entry => entry.Runtime)
                .ToArray();
        }

        return indexedRuntimes
            .OrderByDescending(entry => GetCompatibilityScore(entry.Runtime, detectedVersionParts))
            .ThenBy(entry => entry.Index)
            .Select(static entry => entry.Runtime)
            .ToArray();
    }

    private static int GetCompatibilityScore(
        SpineRuntimeDescriptor runtime,
        ParsedVersion detectedVersion)
    {
        if (!TryGetRuntimeVersion(runtime, out ParsedVersion runtimeVersion))
        {
            return int.MinValue;
        }

        if (runtimeVersion.Major == detectedVersion.Major &&
            runtimeVersion.Minor == detectedVersion.Minor)
        {
            if (runtimeVersion.Patch.HasValue && detectedVersion.Patch.HasValue)
            {
                return 10_000 - Math.Abs(runtimeVersion.Patch.Value - detectedVersion.Patch.Value);
            }

            return 10_000;
        }

        if (runtimeVersion.Major == detectedVersion.Major)
        {
            return 1_000 - Math.Abs(runtimeVersion.Minor - detectedVersion.Minor);
        }

        return 0;
    }

    private static bool TryDetectVersionFromBuffer(
        ReadOnlySpan<byte> buffer,
        out string? detectedVersion)
    {
        string decodedText = System.Text.Encoding.ASCII.GetString(buffer);
        Match match = VersionPattern.Match(decodedText);
        if (match.Success)
        {
            detectedVersion = match.Groups["version"].Value;
            return true;
        }

        detectedVersion = null;
        return false;
    }

    private static bool TryGetRuntimeVersion(
        SpineRuntimeDescriptor runtime,
        out ParsedVersion version)
    {
        return TryParseVersion(runtime.RuntimeId, out version) ||
            TryParseVersion(runtime.DisplayName, out version) ||
            TryParseVersion(runtime.SupportedExportRange, out version);
    }

    private static bool TryParseVersion(
        string? text,
        out ParsedVersion version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        Match match = VersionPattern.Match(text);
        if (!match.Success)
        {
            return false;
        }

        string[] segments = match.Groups["version"].Value.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 3 ||
            !int.TryParse(segments[0], out int major) ||
            !int.TryParse(segments[1], out int minor) ||
            !int.TryParse(segments[2], out int patch))
        {
            return false;
        }

        version = new ParsedVersion(major, minor, patch);
        return true;
    }

    private static async Task<string?> TryDetectBinaryVersionAsync(
        string skeletonPath,
        CancellationToken cancellationToken)
    {
        const int VersionProbeByteCount = 4 * 1024;
        byte[] buffer = new byte[VersionProbeByteCount];

        await using FileStream stream = File.OpenRead(skeletonPath);
        int bytesRead = await stream
            .ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
            .ConfigureAwait(false);

        return bytesRead > 0 && TryDetectVersionFromBuffer(buffer.AsSpan(0, bytesRead), out string? detectedVersion)
            ? detectedVersion
            : null;
    }

    private readonly record struct ParsedVersion(int Major, int Minor, int? Patch);
}
