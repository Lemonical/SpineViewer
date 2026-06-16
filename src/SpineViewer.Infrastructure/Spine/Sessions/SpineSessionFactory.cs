using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;

namespace SpineViewer.Infrastructure.Spine.Sessions;

/// <summary>
/// Creates durable session models from successful project loads and resets transient viewer state boundaries.
/// </summary>
public sealed class SpineSessionFactory : ISpineSessionFactory
{
    /// <inheritdoc />
    public SpineProjectSession Create(LoadSpineProjectResult loadResult, ViewerSettings viewerSettings)
    {
        ArgumentNullException.ThrowIfNull(loadResult);
        ArgumentNullException.ThrowIfNull(viewerSettings);

        if (!loadResult.IsSuccessful)
        {
            throw new ArgumentException(
                "A session can only be created from a successful load result.",
                nameof(loadResult));
        }

        PlaybackState playbackState = new(
            false,
            viewerSettings.LoopPlaybackByDefault,
            viewerSettings.DefaultPlaybackSpeed,
            TimeSpan.Zero,
            Array.Empty<AnimationTrackState>());

        ViewportState viewportState = new(
            1.0,
            0.0,
            0.0,
            viewerSettings.ShowGridByDefault,
            viewerSettings.ShowOriginByDefault,
            viewerSettings.ShowBonesByDefault,
            viewerSettings.ShowBoundsByDefault);

        return new SpineProjectSession(
            Guid.NewGuid(),
            loadResult.ProjectReference,
            loadResult.AssetFileSet,
            loadResult.SelectedRuntime,
            loadResult.VersionMatch,
            playbackState,
            viewportState,
            loadResult.Diagnostics.Distinct().ToArray());
    }
}
