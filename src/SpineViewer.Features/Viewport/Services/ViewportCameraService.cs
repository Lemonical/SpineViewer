using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Applies viewport camera mutations and calculates host-centered render transforms.
/// </summary>
public sealed class ViewportCameraService : IViewportCameraService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportCameraService"/> class.
    /// </summary>
    public ViewportCameraService()
    {
    }

    /// <inheritdoc />
    public ViewportRenderTransform CreateTransform(
        ViewportState viewportState,
        ViewportHostLayout hostLayout)
    {
        ArgumentNullException.ThrowIfNull(viewportState);
        ArgumentNullException.ThrowIfNull(hostLayout);

        return new ViewportRenderTransform(
            viewportState.Zoom,
            (hostLayout.Width / 2.0) + viewportState.OffsetX,
            (hostLayout.Height / 2.0) + viewportState.OffsetY);
    }

    /// <inheritdoc />
    public ViewportState Pan(
        ViewportState viewportState,
        double deltaX,
        double deltaY)
    {
        ArgumentNullException.ThrowIfNull(viewportState);

        if (double.IsNaN(deltaX) || double.IsInfinity(deltaX))
        {
            throw new ArgumentOutOfRangeException(nameof(deltaX), "DeltaX must be a finite number.");
        }

        if (double.IsNaN(deltaY) || double.IsInfinity(deltaY))
        {
            throw new ArgumentOutOfRangeException(nameof(deltaY), "DeltaY must be a finite number.");
        }

        return viewportState with
        {
            OffsetX = viewportState.OffsetX + deltaX,
            OffsetY = viewportState.OffsetY + deltaY,
        };
    }

    /// <inheritdoc />
    public ViewportState Zoom(
        ViewportState viewportState,
        double zoomFactor)
    {
        ArgumentNullException.ThrowIfNull(viewportState);

        if (double.IsNaN(zoomFactor) || double.IsInfinity(zoomFactor) || zoomFactor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(zoomFactor), "Zoom factor must be a positive finite number.");
        }

        return viewportState with
        {
            Zoom = viewportState.Zoom * zoomFactor,
        };
    }
}
