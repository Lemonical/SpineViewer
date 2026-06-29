using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Rendering;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Measures world-space bounds from the active Spine runtime preview implementation.
/// </summary>
public sealed class SpineRuntimePreviewBoundsService : ISpineRuntimePreviewBoundsService, IDisposable
{
    private ISpineRuntimePreviewDriver? _activeDriver;
    private string? _activeAtlasPath;
    private string? _activeRuntimeId;
    private string? _activeSkeletonPath;

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeActiveDriver();
    }

    /// <inheritdoc />
    public ViewportContentBounds? MeasureContentBounds(SpineProjectSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        try
        {
            EnsureDriver(session);
            _activeDriver!.Synchronize(session.Playback, session.SelectedSkinName);
            return _activeDriver.GetContentBounds();
        }
        catch
        {
            DisposeActiveDriver();
            return null;
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
