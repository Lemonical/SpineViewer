using SpineViewer.Core.Models;

namespace SpineViewer.Features.Viewport.Rendering;

/// <summary>
/// Creates runtime-specific preview drivers for the active Spine session.
/// </summary>
internal static class SpineRuntimePreviewDriverFactory
{
    /// <summary>
    /// Creates a preview driver for the supplied session.
    /// </summary>
    /// <param name="session">The active Spine session.</param>
    /// <returns>The runtime-specific preview driver.</returns>
    internal static ISpineRuntimePreviewDriver Create(SpineProjectSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.Runtime.RuntimeId switch
        {
            "spine-3.8.95" => new Spine38RuntimePreviewDriver(
                session.AssetFileSet.AtlasPath,
                session.AssetFileSet.SkeletonPath),
            "spine-4.1.00" => new Spine41RuntimePreviewDriver(
                session.AssetFileSet.AtlasPath,
                session.AssetFileSet.SkeletonPath),
            _ => throw new NotSupportedException(
                $"Viewport rendering is not available for runtime '{session.Runtime.RuntimeId}'."),
        };
    }
}
