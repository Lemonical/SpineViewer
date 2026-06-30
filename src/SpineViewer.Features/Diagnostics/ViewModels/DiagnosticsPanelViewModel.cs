using CommunityToolkit.Mvvm.ComponentModel;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Diagnostics.ViewModels;

/// <summary>
/// Presents session diagnostics, unsupported features, and overlay-state summaries.
/// </summary>
public sealed partial class DiagnosticsPanelViewModel : ObservableObject, IDisposable
{
    private readonly IWorkspaceSessionService _workspaceSessionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsPanelViewModel"/> class.
    /// </summary>
    public DiagnosticsPanelViewModel(IWorkspaceSessionService workspaceSessionService)
    {
        _workspaceSessionService =
            workspaceSessionService ?? throw new ArgumentNullException(nameof(workspaceSessionService));

        ApplyState(_workspaceSessionService.State);
        _workspaceSessionService.StateChanged += OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Gets the current diagnostics list.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiagnostics))]
    [NotifyPropertyChangedFor(nameof(HasNoDiagnostics))]
    [NotifyPropertyChangedFor(nameof(DiagnosticSeveritySummaryText))]
    private IReadOnlyList<ViewerDiagnostic> diagnostics = Array.Empty<ViewerDiagnostic>();

    /// <summary>
    /// Gets the currently selected diagnostic.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDiagnosticTitle))]
    [NotifyPropertyChangedFor(nameof(SelectedDiagnosticSummaryText))]
    [NotifyPropertyChangedFor(nameof(HasSelectedDiagnosticSuggestedAction))]
    private ViewerDiagnostic? selectedDiagnostic;

    /// <summary>
    /// Gets the current unsupported-feature list.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsupportedFeatures))]
    [NotifyPropertyChangedFor(nameof(UnsupportedFeatureSummaryText))]
    private IReadOnlyList<UnsupportedSpineFeature> unsupportedFeatures = Array.Empty<UnsupportedSpineFeature>();

    /// <summary>
    /// Gets the current overlay summary.
    /// </summary>
    [ObservableProperty]
    private string overlaySummaryText = "Open a Spine project to inspect overlay states.";

    /// <summary>
    /// Releases the workspace-state subscription held by this view model.
    /// </summary>
    public void Dispose()
    {
        _workspaceSessionService.StateChanged -= OnWorkspaceStateChanged;
    }

    /// <summary>
    /// Gets a value indicating whether diagnostics are available.
    /// </summary>
    public bool HasDiagnostics => Diagnostics.Count > 0;

    /// <summary>
    /// Gets a value indicating whether no diagnostics are available.
    /// </summary>
    public bool HasNoDiagnostics => !HasDiagnostics;

    /// <summary>
    /// Gets a value indicating whether unsupported features are available.
    /// </summary>
    public bool HasUnsupportedFeatures => UnsupportedFeatures.Count > 0;

    /// <summary>
    /// Gets the current diagnostic severity summary.
    /// </summary>
    public string DiagnosticSeveritySummaryText
    {
        get
        {
            if (Diagnostics.Count == 0)
            {
                return "No diagnostics are active.";
            }

            int errorCount = Diagnostics.Count(static diagnostic => diagnostic.Severity == ViewerDiagnosticSeverity.Error);
            int warningCount = Diagnostics.Count(static diagnostic => diagnostic.Severity == ViewerDiagnosticSeverity.Warning);
            int infoCount = Diagnostics.Count - errorCount - warningCount;
            return $"{errorCount} error(s), {warningCount} warning(s), {infoCount} info item(s).";
        }
    }

    /// <summary>
    /// Gets the selected diagnostic title.
    /// </summary>
    public string SelectedDiagnosticTitle => SelectedDiagnostic is null
        ? "Select a diagnostic to inspect recovery details."
        : $"{SelectedDiagnostic.Severity} | {SelectedDiagnostic.Code}";

    /// <summary>
    /// Gets the selected diagnostic summary text.
    /// </summary>
    public string SelectedDiagnosticSummaryText => SelectedDiagnostic?.Message
        ?? "No diagnostic is currently selected.";

    /// <summary>
    /// Gets a value indicating whether the selected diagnostic includes recovery guidance.
    /// </summary>
    public bool HasSelectedDiagnosticSuggestedAction =>
        !string.IsNullOrWhiteSpace(SelectedDiagnostic?.SuggestedAction);

    /// <summary>
    /// Gets the unsupported-feature summary text.
    /// </summary>
    public string UnsupportedFeatureSummaryText => UnsupportedFeatures.Count == 0
        ? "No unsupported Spine features were detected in the current session."
        : $"{UnsupportedFeatures.Count} unsupported feature(s) need review.";

    private void ApplyState(WorkspaceState workspaceState)
    {
        Diagnostics = workspaceState.Diagnostics;
        SelectedDiagnostic = ChooseSelectedDiagnostic(workspaceState.Diagnostics);
        UnsupportedFeatures = workspaceState.CurrentSession?.UnsupportedFeatures ?? Array.Empty<UnsupportedSpineFeature>();
        OverlaySummaryText = BuildOverlaySummary(workspaceState.CurrentSession?.Viewport);
    }

    private static string BuildOverlaySummary(ViewportState? viewportState)
    {
        if (viewportState is null)
        {
            return "Open a Spine project to inspect overlay states.";
        }

        List<string> activeOverlays = [];

        if (viewportState.ShowGrid)
        {
            activeOverlays.Add("Grid");
        }

        if (viewportState.ShowOrigin)
        {
            activeOverlays.Add("Origin");
        }

        if (viewportState.ShowBones)
        {
            activeOverlays.Add("Bones");
        }

        if (viewportState.ShowBounds)
        {
            activeOverlays.Add("Bounds");
        }

        if (viewportState.ShowMeshWireframe)
        {
            activeOverlays.Add("Mesh");
        }

        if (viewportState.ShowSlotOutlines)
        {
            activeOverlays.Add("Slots");
        }

        if (viewportState.ShowLabels)
        {
            activeOverlays.Add("Labels");
        }

        if (viewportState.ShowMissingResourceIndicators)
        {
            activeOverlays.Add("Missing");
        }

        if (viewportState.ShowUnsupportedFeatureIndicators)
        {
            activeOverlays.Add("Warnings");
        }

        return activeOverlays.Count == 0
            ? "No technical overlays are active."
            : $"Active overlays: {string.Join(", ", activeOverlays)}";
    }

    private static ViewerDiagnostic? ChooseSelectedDiagnostic(IReadOnlyList<ViewerDiagnostic> diagnostics)
    {
        return diagnostics.Count > 0
            ? diagnostics[0]
            : null;
    }

    private void OnWorkspaceStateChanged(object? sender, EventArgs e)
    {
        ApplyState(_workspaceSessionService.State);
    }
}
