using SkiaSharp;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Rendering;

/// <summary>
/// Caches the active Spine runtime preview driver for the viewport render host.
/// </summary>
internal sealed class SpineRuntimePreviewRenderer : IDisposable
{
    private ISpineRuntimePreviewDriver? _activeDriver;
    private string? _activeAtlasPath;
    private string? _activeRuntimeId;
    private string? _activeSkeletonPath;

    /// <summary>
    /// Gets the last render failure message, if any.
    /// </summary>
    internal string? LastErrorMessage { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeActiveDriver();
    }

    /// <summary>
    /// Renders the supplied session into the viewport using the active camera transform.
    /// </summary>
    /// <param name="canvas">The Skia canvas for the current frame.</param>
    /// <param name="session">The active Spine session.</param>
    /// <param name="transform">The world-to-screen transform for the current viewport frame.</param>
    /// <returns><see langword="true" /> when a runtime preview was drawn; otherwise, <see langword="false" />.</returns>
    public bool TryRender(
        SKCanvas canvas,
        SpineProjectSession session,
        ViewportRenderTransform transform)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(transform);

        try
        {
            EnsureDriver(session);
            _activeDriver!.Synchronize(session.Playback, session.SelectedSkinName);

            using SKAutoCanvasRestore _ = new(canvas, true);
            SKMatrix matrix = SKMatrix.CreateScaleTranslation(
                (float)transform.Scale,
                (float)transform.Scale,
                (float)transform.TranslateX,
                (float)transform.TranslateY);
            canvas.Concat(in matrix);
            _activeDriver.Draw(canvas);
            LastErrorMessage = null;
            return true;
        }
        catch (Exception exception)
        {
            LastErrorMessage = exception.ToString();
            DisposeActiveDriver();
            return false;
        }
    }

    private void DisposeActiveDriver()
    {
        _activeDriver?.Dispose();
        _activeDriver = null;
        _activeRuntimeId = null;
        _activeSkeletonPath = null;
        _activeAtlasPath = null;
    }

    private void EnsureDriver(SpineProjectSession session)
    {
        if (_activeDriver is not null &&
            string.Equals(_activeRuntimeId, session.Runtime.RuntimeId, StringComparison.Ordinal) &&
            string.Equals(_activeSkeletonPath, session.AssetFileSet.SkeletonPath, StringComparison.Ordinal) &&
            string.Equals(_activeAtlasPath, session.AssetFileSet.AtlasPath, StringComparison.Ordinal))
        {
            return;
        }

        DisposeActiveDriver();
        _activeDriver = SpineRuntimePreviewDriverFactory.Create(session);
        _activeRuntimeId = session.Runtime.RuntimeId;
        _activeSkeletonPath = session.AssetFileSet.SkeletonPath;
        _activeAtlasPath = session.AssetFileSet.AtlasPath;
    }
}
