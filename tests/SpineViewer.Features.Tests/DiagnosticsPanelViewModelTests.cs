using SpineViewer.Core.Models;
using SpineViewer.Features.Diagnostics.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class DiagnosticsPanelViewModelTests
{
    [Fact]
    public void Constructor_WithViewportFlagsAndUnsupportedFeatures_BuildsSummary()
    {
        ViewerDiagnostic diagnostic = new(
            "atlas-missing",
            ViewerDiagnosticSeverity.Error,
            "Atlas texture is missing.",
            "Resolver");
        UnsupportedSpineFeature unsupportedFeature = new("clipping", "Clipping", "Runtime fallback required.");
        SpineProjectSession session = FeatureTestFactory.CreateSession(
            viewportState: new ViewportState(
                1.0,
                0.0,
                0.0,
                true,
                false,
                true,
                false,
                true,
                false,
                true,
                true,
                true,
                ViewportBackgroundStyle.Black),
            diagnostics: [diagnostic],
            unsupportedFeatures: [unsupportedFeature]);
        TestWorkspaceSessionService workspaceSessionService = new(
            FeatureTestFactory.CreateWorkspaceState(
                session,
                diagnostics: [diagnostic]));

        DiagnosticsPanelViewModel viewModel = new(workspaceSessionService);

        Assert.True(viewModel.HasDiagnostics);
        Assert.True(viewModel.HasUnsupportedFeatures);
        Assert.Contains("Grid", viewModel.OverlaySummaryText);
        Assert.Contains("Bones", viewModel.OverlaySummaryText);
        Assert.Contains("Mesh", viewModel.OverlaySummaryText);
        Assert.Contains("Labels", viewModel.OverlaySummaryText);
        Assert.Contains("Missing", viewModel.OverlaySummaryText);
        Assert.Contains("Warnings", viewModel.OverlaySummaryText);
        Assert.Equal(diagnostic, viewModel.SelectedDiagnostic);
    }
}
