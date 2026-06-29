using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class GridOverlaySourceTests
{
    [Fact]
    public void CreateWorldLines_CoversVisibleViewportSpan()
    {
        GridOverlaySource source = new();
        ViewportState viewportState = new(
            2.0,
            100.0,
            -50.0,
            true,
            true,
            false,
            false);
        WorkspaceState workspaceState = new(
            null,
            Array.Empty<SpineProjectReference>(),
            Array.Empty<ViewerDiagnostic>(),
            "Ready.",
            false);
        ViewportHostLayout hostLayout = new(800.0, 600.0);
        ViewportOverlayContext context = new(workspaceState, viewportState, hostLayout);

        IReadOnlyList<ViewportOverlayLine> lines = source.CreateWorldLines(context);
        double viewportCenterX = (hostLayout.Width / 2.0) + viewportState.OffsetX;
        double viewportCenterY = (hostLayout.Height / 2.0) + viewportState.OffsetY;
        double visibleMinX = (0.0 - viewportCenterX) / viewportState.Zoom;
        double visibleMaxX = (hostLayout.Width - viewportCenterX) / viewportState.Zoom;
        double visibleMinY = (0.0 - viewportCenterY) / viewportState.Zoom;
        double visibleMaxY = (hostLayout.Height - viewportCenterY) / viewportState.Zoom;
        double worldMinX = lines.Min(static line => Math.Min(line.StartX, line.EndX));
        double worldMaxX = lines.Max(static line => Math.Max(line.StartX, line.EndX));
        double worldMinY = lines.Min(static line => Math.Min(line.StartY, line.EndY));
        double worldMaxY = lines.Max(static line => Math.Max(line.StartY, line.EndY));

        Assert.NotEmpty(lines);
        Assert.True(worldMinX <= visibleMinX);
        Assert.True(worldMaxX >= visibleMaxX);
        Assert.True(worldMinY <= visibleMinY);
        Assert.True(worldMaxY >= visibleMaxY);
    }
}
