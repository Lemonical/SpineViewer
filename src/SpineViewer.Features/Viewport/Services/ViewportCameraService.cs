using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Applies viewport camera mutations and calculates host-centered render transforms.
/// </summary>
public sealed class ViewportCameraService : IViewportCameraService
{
    private const double MinimumZoom = 0.2;
    private const double MaximumZoom = 6.0;
    private const double FitPaddingRatio = 0.14;

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
    public ViewportState Reset(ViewportState viewportState)
    {
        ArgumentNullException.ThrowIfNull(viewportState);

        return viewportState with
        {
            Zoom = 1.0,
            OffsetX = 0.0,
            OffsetY = 0.0,
        };
    }

    /// <inheritdoc />
    public ViewportState FitToView(
        ViewportState viewportState,
        ViewportHostLayout hostLayout,
        ViewportContentBounds contentBounds)
    {
        ArgumentNullException.ThrowIfNull(viewportState);
        ArgumentNullException.ThrowIfNull(hostLayout);
        ArgumentNullException.ThrowIfNull(contentBounds);

        if (!hostLayout.HasSurface)
        {
            return viewportState;
        }

        double paddedWidth = Math.Max(hostLayout.Width * (1.0 - (FitPaddingRatio * 2.0)), 1.0);
        double paddedHeight = Math.Max(hostLayout.Height * (1.0 - (FitPaddingRatio * 2.0)), 1.0);
        double contentWidth = Math.Max(contentBounds.Width, 1.0);
        double contentHeight = Math.Max(contentBounds.Height, 1.0);
        double fitZoom = ClampZoom(Math.Min(paddedWidth / contentWidth, paddedHeight / contentHeight));

        return viewportState with
        {
            Zoom = fitZoom,
            OffsetX = -(contentBounds.CenterX * fitZoom),
            OffsetY = -(contentBounds.CenterY * fitZoom),
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
            Zoom = ClampZoom(viewportState.Zoom * zoomFactor),
        };
    }

    /// <inheritdoc />
    public ViewportState ZoomAtPoint(
        ViewportState viewportState,
        ViewportHostLayout hostLayout,
        double screenX,
        double screenY,
        double zoomFactor)
    {
        ArgumentNullException.ThrowIfNull(viewportState);
        ArgumentNullException.ThrowIfNull(hostLayout);

        if (double.IsNaN(screenX) || double.IsInfinity(screenX))
        {
            throw new ArgumentOutOfRangeException(nameof(screenX), "ScreenX must be a finite number.");
        }

        if (double.IsNaN(screenY) || double.IsInfinity(screenY))
        {
            throw new ArgumentOutOfRangeException(nameof(screenY), "ScreenY must be a finite number.");
        }

        if (double.IsNaN(zoomFactor) || double.IsInfinity(zoomFactor) || zoomFactor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(zoomFactor), "Zoom factor must be a positive finite number.");
        }

        if (!hostLayout.HasSurface)
        {
            return viewportState;
        }

        double newZoom = ClampZoom(viewportState.Zoom * zoomFactor);
        double anchoredWorldX = (screenX - ((hostLayout.Width / 2.0) + viewportState.OffsetX)) / viewportState.Zoom;
        double anchoredWorldY = (screenY - ((hostLayout.Height / 2.0) + viewportState.OffsetY)) / viewportState.Zoom;

        return viewportState with
        {
            Zoom = newZoom,
            OffsetX = screenX - (hostLayout.Width / 2.0) - (anchoredWorldX * newZoom),
            OffsetY = screenY - (hostLayout.Height / 2.0) - (anchoredWorldY * newZoom),
        };
    }

    private static double ClampZoom(double zoom)
    {
        return Math.Clamp(zoom, MinimumZoom, MaximumZoom);
    }
}
