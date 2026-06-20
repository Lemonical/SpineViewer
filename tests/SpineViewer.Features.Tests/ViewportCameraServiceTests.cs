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
    public void Zoom_MultipliesCurrentZoomWithoutChangingOffsets()
    {
        ViewportCameraService service = new();
        ViewportState state = new(1.5, 12.0, -8.0, true, true, false, false);

        ViewportState updatedState = service.Zoom(state, 2.0);

        Assert.Equal(3.0, updatedState.Zoom);
        Assert.Equal(12.0, updatedState.OffsetX);
        Assert.Equal(-8.0, updatedState.OffsetY);
    }
}
