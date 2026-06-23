using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class ViewportCameraServiceTests
{
    [Fact]
    public void CreateTransform_CentersHostAndAppliesOffsets()
    {
        ViewportCameraService service = new();
        ViewportState state = new(2.0, 30.0, -20.0, true, true, false, false);

        ViewportRenderTransform transform = service.CreateTransform(state, new ViewportHostLayout(800.0, 600.0));

        Assert.Equal(2.0, transform.Scale);
        Assert.Equal(430.0, transform.TranslateX);
        Assert.Equal(280.0, transform.TranslateY);
    }

    [Fact]
    public void FitToView_CentersContentAndChoosesReadableZoom()
    {
        ViewportCameraService service = new();
        ViewportState state = new();

        ViewportState updatedState = service.FitToView(
            state,
            new ViewportHostLayout(800.0, 600.0),
            new ViewportContentBounds(-90.0, -130.0, 90.0, 130.0));

        Assert.True(updatedState.Zoom > 1.0);
        Assert.Equal(0.0, updatedState.OffsetX);
        Assert.Equal(0.0, updatedState.OffsetY);
    }

    [Fact]
    public void ZoomAtPoint_PreservesAnchoredWorldPosition()
    {
        ViewportCameraService service = new();
        ViewportState state = new(1.0, 0.0, 0.0, true, true, false, false);

        ViewportState updatedState = service.ZoomAtPoint(
            state,
            new ViewportHostLayout(800.0, 600.0),
            600.0,
            300.0,
            2.0);

        Assert.Equal(2.0, updatedState.Zoom);
        Assert.Equal(-200.0, updatedState.OffsetX);
        Assert.Equal(0.0, updatedState.OffsetY);
    }
}
