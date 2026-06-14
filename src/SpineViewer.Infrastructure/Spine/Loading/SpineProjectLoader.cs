using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Loads a resolved Spine project through the selected runtime adapter.
/// </summary>
public sealed class SpineProjectLoader : ISpineProjectLoader
{
    private readonly IAppLogger<SpineProjectLoader> _logger;
    private readonly ISpineRuntimeCatalog _runtimeCatalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpineProjectLoader"/> class.
    /// </summary>
    /// <param name="runtimeCatalog">The runtime catalog used for adapter lookup.</param>
    /// <param name="logger">The logger used for unexpected load failures.</param>
    public SpineProjectLoader(
        ISpineRuntimeCatalog runtimeCatalog,
        IAppLogger<SpineProjectLoader> logger)
    {
        _runtimeCatalog = runtimeCatalog ?? throw new ArgumentNullException(nameof(runtimeCatalog));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<LoadSpineProjectResult> LoadAsync(
        LoadSpineProjectRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        request.Progress?.Report(
            new SpineLoadProgress(
                SpineLoadStage.LoadingProject,
                $"Loading {request.ProjectReference.DisplayName} with {request.SelectedRuntime.DisplayName}.",
                0.0));

        try
        {
            ISpineRuntimeAdapter adapter = _runtimeCatalog.GetRequiredAdapter(request.SelectedRuntime.RuntimeId);
            SpineRuntimeProbeResult probeResult = await adapter
                .ProbeAsync(request.AssetFileSet, cancellationToken)
                .ConfigureAwait(false);

            SpineLoadRequest adapterRequest = new(
                request.ProjectReference,
                request.AssetFileSet,
                request.VersionMatch,
                probeResult,
                request.Progress);

            SpineLoadResult runtimeResult = await adapter
                .LoadAsync(adapterRequest, cancellationToken)
                .ConfigureAwait(false);

            List<ViewerDiagnostic> diagnostics =
            [
                .. request.VersionMatch.Diagnostics,
                .. runtimeResult.Diagnostics,
            ];

            request.Progress?.Report(
                new SpineLoadProgress(
                    SpineLoadStage.Completed,
                    runtimeResult.IsSuccessful
                        ? $"Loaded {request.ProjectReference.DisplayName}."
                        : $"Failed to load {request.ProjectReference.DisplayName}.",
                    1.0));

            return new LoadSpineProjectResult(
                runtimeResult.IsSuccessful,
                request.ProjectReference,
                request.AssetFileSet,
                runtimeResult.Runtime,
                request.VersionMatch,
                diagnostics);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (KeyNotFoundException exception)
        {
            ViewerDiagnostic diagnostic = new(
                "project-load-runtime-missing",
                ViewerDiagnosticSeverity.Error,
                "The selected runtime is not registered in the current application host.",
                nameof(SpineProjectLoader),
                exception.Message,
                "Choose another runtime or restart the application after installing the missing runtime.");

            return new LoadSpineProjectResult(
                false,
                request.ProjectReference,
                request.AssetFileSet,
                request.SelectedRuntime,
                request.VersionMatch,
                request.VersionMatch.Diagnostics.Concat([diagnostic]));
        }
        catch (Exception exception)
        {
            _logger.LogError("Unexpected Spine project load failure.", exception);

            ViewerDiagnostic diagnostic = new(
                "project-load-unhandled-exception",
                ViewerDiagnosticSeverity.Error,
                "The project load failed unexpectedly.",
                nameof(SpineProjectLoader),
                exception.Message,
                "Inspect the diagnostics and try opening the project again.");

            return new LoadSpineProjectResult(
                false,
                request.ProjectReference,
                request.AssetFileSet,
                request.SelectedRuntime,
                request.VersionMatch,
                request.VersionMatch.Diagnostics.Concat([diagnostic]));
        }
    }
}
