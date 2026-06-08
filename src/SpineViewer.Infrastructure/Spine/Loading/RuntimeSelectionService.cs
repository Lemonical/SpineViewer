using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Selects the best available runtime adapter for a resolved Spine project.
/// </summary>
public sealed class RuntimeSelectionService : IRuntimeSelectionService
{
    private readonly ISpineRuntimeCatalog _runtimeCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeSelectionService"/> class.
    /// </summary>
    /// <param name="runtimeCatalog">The runtime catalog used for adapter lookup.</param>
    public RuntimeSelectionService(ISpineRuntimeCatalog runtimeCatalog)
    {
        _runtimeCatalog = runtimeCatalog ?? throw new ArgumentNullException(nameof(runtimeCatalog));
    }

    /// <inheritdoc />
    public async Task<RuntimeSelectionResult> SelectAsync(
        RuntimeSelectionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        List<ViewerDiagnostic> selectionDiagnostics =
        [
            .. request.VersionMatch.Diagnostics,
        ];

        IReadOnlyList<SpineRuntimeDescriptor> availableRuntimes = _runtimeCatalog.GetAvailableRuntimes();
        HashSet<string> availableRuntimeIds = availableRuntimes
            .Select(static runtime => runtime.RuntimeId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        SpineRuntimeProbeResult? selectedProbeResult = null;

        foreach (string candidateRuntimeId in BuildCandidateRuntimeIds(request, availableRuntimes))
        {
            if (!availableRuntimeIds.Contains(candidateRuntimeId))
            {
                if (string.Equals(candidateRuntimeId, request.PreferredRuntimeId, StringComparison.OrdinalIgnoreCase))
                {
                    selectionDiagnostics.Add(
                        new ViewerDiagnostic(
                            "runtime-selection-preferred-runtime-missing",
                            ViewerDiagnosticSeverity.Warning,
                            "The preferred runtime is not available.",
                            nameof(RuntimeSelectionService),
                            $"No runtime adapter is registered for '{candidateRuntimeId}'.",
                            "Choose a different runtime or install the missing adapter."));
                }

                continue;
            }

            ISpineRuntimeAdapter adapter = _runtimeCatalog.GetRequiredAdapter(candidateRuntimeId);
            SpineRuntimeProbeResult probeResult = await adapter
                .ProbeAsync(request.AssetFileSet, cancellationToken)
                .ConfigureAwait(false);

            if (!probeResult.CanLoad)
            {
                if (string.Equals(candidateRuntimeId, request.PreferredRuntimeId, StringComparison.OrdinalIgnoreCase))
                {
                    selectionDiagnostics.AddRange(probeResult.Diagnostics);
                }

                continue;
            }

            selectedProbeResult = probeResult;
            selectionDiagnostics.AddRange(probeResult.Diagnostics);
            break;
        }

        if (selectedProbeResult is null)
        {
            selectionDiagnostics.Add(
                new ViewerDiagnostic(
                    "runtime-selection-no-compatible-runtime",
                    ViewerDiagnosticSeverity.Error,
                    "No compatible runtime is available for this Spine project.",
                    nameof(RuntimeSelectionService),
                    $"The project '{request.ProjectReference.DisplayName}' could not be matched to a registered runtime.",
                    "Verify the detected Spine version or install another runtime adapter."));

            return new RuntimeSelectionResult(
                null,
                false,
                true,
                selectionDiagnostics);
        }

        bool isExactVersionMatch = request.VersionMatch.IsExactMatch &&
            string.Equals(
                request.VersionMatch.SuggestedRuntimeId,
                selectedProbeResult.Runtime.RuntimeId,
                StringComparison.OrdinalIgnoreCase);

        if (!isExactVersionMatch)
        {
            selectionDiagnostics.Add(
                new ViewerDiagnostic(
                    "runtime-selection-compatibility-warning",
                    ViewerDiagnosticSeverity.Warning,
                    "The selected runtime is compatible, but it is not an exact version match.",
                    nameof(RuntimeSelectionService),
                    $"Selected runtime: {selectedProbeResult.Runtime.DisplayName}. Detected export version: {request.VersionMatch.DetectedExportVersion ?? "unknown"}.",
                    "Review diagnostics before loading the project."));
        }

        bool requiresUserConfirmation = !isExactVersionMatch ||
            selectedProbeResult.SupportStatus == SpineRuntimeSupportStatus.SupportedWithWarnings ||
            selectionDiagnostics.Any(static diagnostic => diagnostic.Severity != ViewerDiagnosticSeverity.Information);

        return new RuntimeSelectionResult(
            selectedProbeResult,
            isExactVersionMatch,
            requiresUserConfirmation,
            selectionDiagnostics);
    }

    private static IReadOnlyList<string> BuildCandidateRuntimeIds(
        RuntimeSelectionRequest request,
        IEnumerable<SpineRuntimeDescriptor> availableRuntimes)
    {
        List<string> candidateRuntimeIds = [];

        if (!string.IsNullOrWhiteSpace(request.PreferredRuntimeId))
        {
            candidateRuntimeIds.Add(request.PreferredRuntimeId);
        }

        if (!string.IsNullOrWhiteSpace(request.VersionMatch.SuggestedRuntimeId))
        {
            candidateRuntimeIds.Add(request.VersionMatch.SuggestedRuntimeId);
        }

        candidateRuntimeIds.AddRange(request.VersionMatch.CompatibleRuntimeIds);
        candidateRuntimeIds.AddRange(availableRuntimes.Select(static runtime => runtime.RuntimeId));

        return candidateRuntimeIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
