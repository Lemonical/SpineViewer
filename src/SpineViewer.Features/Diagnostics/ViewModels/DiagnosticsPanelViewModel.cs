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
    private IReadOnlyList<ViewerDiagnostic> diagnostics = Array.Empty<ViewerDiagnostic>();

    /// <summary>
    /// Gets the currently selected diagnostic.
    /// </summary>
    [ObservableProperty]
    private ViewerDiagnostic? selectedDiagnostic;

    /// <summary>
    /// Gets the current unsupported-feature list.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnsupportedFeatures))]
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
