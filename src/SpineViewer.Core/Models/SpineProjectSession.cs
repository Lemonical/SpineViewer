using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the durable application-facing state for one loaded Spine session.
/// </summary>
public sealed record SpineProjectSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineProjectSession"/> class.
    /// </summary>
    /// <param name="sessionId">The stable identity of the session.</param>
    /// <param name="project">The project reference that opened the session.</param>
    /// <param name="assetFileSet">The resolved files loaded for the session.</param>
    /// <param name="runtime">The runtime selected to open the project.</param>
    /// <param name="versionMatch">The detected version-match result for the project.</param>
    /// <param name="playback">The current playback state.</param>
    /// <param name="viewport">The current viewport state.</param>
    /// <param name="diagnostics">The diagnostics associated with the session.</param>
    public SpineProjectSession(
        Guid sessionId,
        SpineProjectReference project,
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor runtime,
        SpineVersionMatch versionMatch,
        PlaybackState playback,
        ViewportState viewport,
        IEnumerable<ViewerDiagnostic> diagnostics)
        : this(
            sessionId,
            project,
            assetFileSet,
            runtime,
            versionMatch,
            playback,
            viewport,
            SpineProjectInspection.Empty,
            Array.Empty<UnsupportedSpineFeature>(),
            null,
            diagnostics)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpineProjectSession"/> class.
    /// </summary>
    /// <param name="sessionId">The stable identity of the session.</param>
    /// <param name="project">The project reference that opened the session.</param>
    /// <param name="assetFileSet">The resolved files loaded for the session.</param>
    /// <param name="runtime">The runtime selected to open the project.</param>
    /// <param name="versionMatch">The detected version-match result for the project.</param>
    /// <param name="playback">The current playback state.</param>
    /// <param name="viewport">The current viewport state.</param>
    /// <param name="inspection">The structured asset inspection snapshot.</param>
    /// <param name="unsupportedFeatures">The unsupported features reported for the loaded project.</param>
    /// <param name="selectedSkinName">The selected skin name, if any.</param>
    /// <param name="diagnostics">The diagnostics associated with the session.</param>
    public SpineProjectSession(
        Guid sessionId,
        SpineProjectReference project,
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor runtime,
        SpineVersionMatch versionMatch,
        PlaybackState playback,
        ViewportState viewport,
        SpineProjectInspection inspection,
        IEnumerable<UnsupportedSpineFeature> unsupportedFeatures,
        string? selectedSkinName,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        SessionId = Guard.NonEmpty(sessionId, nameof(sessionId));
        Project = Guard.NotNull(project, nameof(project));
        AssetFileSet = Guard.NotNull(assetFileSet, nameof(assetFileSet));
        Runtime = Guard.NotNull(runtime, nameof(runtime));
        VersionMatch = Guard.NotNull(versionMatch, nameof(versionMatch));
        Playback = Guard.NotNull(playback, nameof(playback));
        Viewport = Guard.NotNull(viewport, nameof(viewport));
        Inspection = Guard.NotNull(inspection, nameof(inspection));
        UnsupportedFeatures = Guard.MaterializeReadOnlyList(unsupportedFeatures, nameof(unsupportedFeatures));
        SelectedSkinName = Guard.NullIfWhiteSpace(selectedSkinName);
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets the stable identity of the session.
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// Gets the project reference that opened the session.
    /// </summary>
    public SpineProjectReference Project { get; init; }

    /// <summary>
    /// Gets the resolved files loaded for the session.
    /// </summary>
    public SpineAssetFileSet AssetFileSet { get; init; }

    /// <summary>
    /// Gets the runtime selected to open the project.
    /// </summary>
    public SpineRuntimeDescriptor Runtime { get; init; }

    /// <summary>
    /// Gets the detected version-match result for the project.
    /// </summary>
    public SpineVersionMatch VersionMatch { get; init; }

    /// <summary>
    /// Gets the current playback state.
    /// </summary>
    public PlaybackState Playback { get; init; }

    /// <summary>
    /// Gets the current viewport state.
    /// </summary>
    public ViewportState Viewport { get; init; }

    /// <summary>
    /// Gets the structured asset inspection snapshot.
    /// </summary>
    public SpineProjectInspection Inspection { get; init; }

    /// <summary>
    /// Gets the unsupported features reported for the loaded project.
    /// </summary>
    public IReadOnlyList<UnsupportedSpineFeature> UnsupportedFeatures { get; init; }

    /// <summary>
    /// Gets the selected skin name, if any.
    /// </summary>
    public string? SelectedSkinName { get; init; }

    /// <summary>
    /// Gets the diagnostics associated with the session.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
