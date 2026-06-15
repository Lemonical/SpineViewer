using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents persisted viewer-level preferences that should survive between sessions.
/// </summary>
public sealed record ViewerSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettings"/> class with release-1 defaults.
    /// </summary>
    public ViewerSettings()
        : this(ViewerTheme.FollowSystem, true, true, false, false, true, 1.0, 10, true, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettings"/> class.
    /// </summary>
    /// <param name="theme">The persisted viewer theme preference.</param>
    /// <param name="showGridByDefault">Indicates whether the grid overlay starts enabled.</param>
    /// <param name="showOriginByDefault">Indicates whether the origin overlay starts enabled.</param>
    /// <param name="showBonesByDefault">Indicates whether the bones overlay starts enabled.</param>
    /// <param name="showBoundsByDefault">Indicates whether the bounds overlay starts enabled.</param>
    /// <param name="loopPlaybackByDefault">Indicates whether playback loops by default.</param>
    /// <param name="defaultPlaybackSpeed">The default playback speed multiplier.</param>
    /// <param name="recentFilesLimit">The maximum number of recent-file entries to persist.</param>
    public ViewerSettings(
        ViewerTheme theme,
        bool showGridByDefault,
        bool showOriginByDefault,
        bool showBonesByDefault,
        bool showBoundsByDefault,
        bool loopPlaybackByDefault,
        double defaultPlaybackSpeed,
        int recentFilesLimit)
        : this(
            theme,
            showGridByDefault,
            showOriginByDefault,
            showBonesByDefault,
            showBoundsByDefault,
            loopPlaybackByDefault,
            defaultPlaybackSpeed,
            recentFilesLimit,
            true,
            null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettings"/> class.
    /// </summary>
    /// <param name="theme">The persisted viewer theme preference.</param>
    /// <param name="showGridByDefault">Indicates whether the grid overlay starts enabled.</param>
    /// <param name="showOriginByDefault">Indicates whether the origin overlay starts enabled.</param>
    /// <param name="showBonesByDefault">Indicates whether the bones overlay starts enabled.</param>
    /// <param name="showBoundsByDefault">Indicates whether the bounds overlay starts enabled.</param>
    /// <param name="loopPlaybackByDefault">Indicates whether playback loops by default.</param>
    /// <param name="defaultPlaybackSpeed">The default playback speed multiplier.</param>
    /// <param name="recentFilesLimit">The maximum number of recent-file entries to persist.</param>
    /// <param name="restoreLastSessionOnStartup">Indicates whether the last open session should be restored on startup.</param>
    /// <param name="lastProjectReference">The last successfully opened project, if one should be restored later.</param>
    public ViewerSettings(
        ViewerTheme theme,
        bool showGridByDefault,
        bool showOriginByDefault,
        bool showBonesByDefault,
        bool showBoundsByDefault,
        bool loopPlaybackByDefault,
        double defaultPlaybackSpeed,
        int recentFilesLimit,
        bool restoreLastSessionOnStartup,
        SpineProjectReference? lastProjectReference)
    {
        Theme = theme;
        ShowGridByDefault = showGridByDefault;
        ShowOriginByDefault = showOriginByDefault;
        ShowBonesByDefault = showBonesByDefault;
        ShowBoundsByDefault = showBoundsByDefault;
        LoopPlaybackByDefault = loopPlaybackByDefault;
        DefaultPlaybackSpeed = Guard.PositiveFinite(defaultPlaybackSpeed, nameof(defaultPlaybackSpeed));
        RecentFilesLimit = Guard.Positive(recentFilesLimit, nameof(recentFilesLimit));
        RestoreLastSessionOnStartup = restoreLastSessionOnStartup;
        LastProjectReference = lastProjectReference;
    }

    /// <summary>
    /// Gets the persisted viewer theme preference.
    /// </summary>
    public ViewerTheme Theme { get; init; }

    /// <summary>
    /// Gets a value indicating whether the grid overlay starts enabled.
    /// </summary>
    public bool ShowGridByDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether the origin overlay starts enabled.
    /// </summary>
    public bool ShowOriginByDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bones overlay starts enabled.
    /// </summary>
    public bool ShowBonesByDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether the bounds overlay starts enabled.
    /// </summary>
    public bool ShowBoundsByDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether playback loops by default.
    /// </summary>
    public bool LoopPlaybackByDefault { get; init; }

    /// <summary>
    /// Gets the default playback speed multiplier.
    /// </summary>
    public double DefaultPlaybackSpeed { get; init; }

    /// <summary>
    /// Gets the maximum number of recent-file entries to persist.
    /// </summary>
    public int RecentFilesLimit { get; init; }

    /// <summary>
    /// Gets a value indicating whether the last open session should be restored on startup.
    /// </summary>
    public bool RestoreLastSessionOnStartup { get; init; }

    /// <summary>
    /// Gets the last successfully opened project, if one should be restored later.
    /// </summary>
    public SpineProjectReference? LastProjectReference { get; init; }
}
