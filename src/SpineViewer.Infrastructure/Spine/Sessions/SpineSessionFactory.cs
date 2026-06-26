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

        AnimationTrackState[] initialTracks = CreateInitialTracks(loadResult.Inspection, viewerSettings);
        TimeSpan initialDuration = DetermineInitialDuration(loadResult.Inspection, initialTracks);

        PlaybackState playbackState = new(
            PlaybackTransportStatus.Stopped,
            viewerSettings.LoopPlaybackByDefault,
            viewerSettings.DefaultPlaybackSpeed,
            TimeSpan.Zero,
            initialDuration,
            initialTracks);

        ViewportState viewportState = new(
            1.0,
            0.0,
            0.0,
            viewerSettings.ShowGridByDefault,
            viewerSettings.ShowOriginByDefault,
            viewerSettings.ShowBonesByDefault,
            viewerSettings.ShowBoundsByDefault,
            viewerSettings.ShowMeshWireframeByDefault,
            viewerSettings.ShowSlotOutlinesByDefault,
            viewerSettings.ShowLabelsByDefault,
            viewerSettings.ShowMissingResourceIndicatorsByDefault,
            viewerSettings.ShowUnsupportedFeatureIndicatorsByDefault,
            viewerSettings.DefaultViewportBackgroundStyle);

        string? selectedSkinName = loadResult.Inspection.Skins
            .FirstOrDefault(static skin => skin.IsDefault)?
            .Name ?? loadResult.Inspection.Skins.FirstOrDefault()?.Name;

        return new SpineProjectSession(
            Guid.NewGuid(),
            loadResult.ProjectReference,
            loadResult.AssetFileSet,
            loadResult.SelectedRuntime,
            loadResult.VersionMatch,
            playbackState,
            viewportState,
            loadResult.Inspection,
            loadResult.UnsupportedFeatures,
            selectedSkinName,
            loadResult.Diagnostics.Distinct().ToArray());
    }

    private static AnimationTrackState[] CreateInitialTracks(
        SpineProjectInspection inspection,
        ViewerSettings viewerSettings)
    {
        SpineAnimationInfo? firstAnimation = inspection.Animations.FirstOrDefault();
        if (firstAnimation is null)
        {
            return Array.Empty<AnimationTrackState>();
        }

        return
        [
            new AnimationTrackState(
                Guid.NewGuid(),
                0,
                firstAnimation.Name,
                viewerSettings.LoopPlaybackByDefault,
                viewerSettings.DefaultTrackTimeScale,
                viewerSettings.DefaultTrackMixDuration,
                true),
        ];
    }

    private static TimeSpan DetermineInitialDuration(
        SpineProjectInspection inspection,
        IReadOnlyList<AnimationTrackState> tracks)
    {
        if (tracks.Count == 0)
        {
            return TimeSpan.FromSeconds(5);
        }

        TimeSpan duration = tracks
            .Select(track => inspection.Animations.FirstOrDefault(animation => animation.Name == track.AnimationName)?.Duration ?? TimeSpan.Zero)
            .DefaultIfEmpty(TimeSpan.Zero)
            .Max();

        return duration > TimeSpan.Zero
            ? duration
            : TimeSpan.FromSeconds(5);
    }
}
