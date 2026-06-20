using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Publishes viewport redraw requests to render-scene consumers.
/// </summary>
public sealed class RenderInvalidationService : IRenderInvalidationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderInvalidationService"/> class.
    /// </summary>
    public RenderInvalidationService()
    {
    }

    /// <inheritdoc />
    public event EventHandler<RenderInvalidatedEventArgs>? RenderInvalidated;

    /// <inheritdoc />
    public void RequestInvalidation(RenderInvalidationReason reason)
    {
        RenderInvalidated?.Invoke(this, new RenderInvalidatedEventArgs(reason));
    }
}
