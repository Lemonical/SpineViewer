using SpineViewer.Core.Models;

namespace SpineViewer.Features.Tests;

internal static class FeatureTestFactory
{
    internal static SpineProjectInspection CreateInspection(
        IEnumerable<SpineAnimationInfo>? animations = null,
        IEnumerable<SpineSkinInfo>? skins = null,
        IEnumerable<SpineBoneInfo>? bones = null,
        IEnumerable<SpineSlotInfo>? slots = null,
        IEnumerable<SpineAttachmentInfo>? attachments = null,
        IEnumerable<SpineAtlasPageInfo>? atlasPages = null,
        IEnumerable<SpineAtlasRegionInfo>? atlasRegions = null,
        IEnumerable<ViewerDiagnostic>? diagnostics = null,
        SpineExportMetadata? exportMetadata = null)
    {
        SpineAnimationInfo[] animationEntries = animations?.ToArray() ?? [];
        SpineSkinInfo[] skinEntries = skins?.ToArray() ?? [];
        SpineBoneInfo[] boneEntries = bones?.ToArray() ?? [];
        SpineSlotInfo[] slotEntries = slots?.ToArray() ?? [];
        SpineAttachmentInfo[] attachmentEntries = attachments?.ToArray() ?? [];
        SpineAtlasPageInfo[] atlasPageEntries = atlasPages?.ToArray() ?? [];
        SpineAtlasRegionInfo[] atlasRegionEntries = atlasRegions?.ToArray() ?? [];
        ViewerDiagnostic[] diagnosticEntries = diagnostics?.ToArray() ?? [];

        exportMetadata ??= new SpineExportMetadata(
            "4.1.00",
            "./images",
            "./audio",
            512,
            256,
            30,
            boneEntries.Length,
            slotEntries.Length,
            skinEntries.Length,
            animationEntries.Length,
            atlasPageEntries.Length,
            atlasRegionEntries.Length);

        return new SpineProjectInspection(
            exportMetadata,
            animationEntries,
            skinEntries,
            boneEntries,
            slotEntries,
            attachmentEntries,
            atlasPageEntries,
            atlasRegionEntries,
            diagnosticEntries);
    }

    internal static SpineRuntimeDescriptor CreateRuntime(bool supportsMultipleTracks = true)
    {
        return new SpineRuntimeDescriptor(
            "spine-4.1.00",
            "Spine 4.1.00",
            "4.1.x",
            new SpineRuntimeCapabilities(
            [
                new SpineRuntimeFeatureSupport(SpineRuntimeFeature.JsonSkeleton, true),
                new SpineRuntimeFeatureSupport(SpineRuntimeFeature.MultipleTracks, supportsMultipleTracks),
                new SpineRuntimeFeatureSupport(SpineRuntimeFeature.Meshes, true),
            ]));
    }

    internal static SpineProjectSession CreateSession(
        string displayName = "Hero",
        PlaybackState? playbackState = null,
        ViewportState? viewportState = null,
        SpineProjectInspection? inspection = null,
        IEnumerable<ViewerDiagnostic>? diagnostics = null,
        IEnumerable<UnsupportedSpineFeature>? unsupportedFeatures = null,
        string? selectedSkinName = null,
        SpineRuntimeDescriptor? runtime = null)
    {
        SpineProjectInspection resolvedInspection = inspection ?? SpineProjectInspection.Empty;
        PlaybackState resolvedPlaybackState = playbackState ?? new PlaybackState();
        ViewportState resolvedViewportState = viewportState ?? new ViewportState();
        ViewerDiagnostic[] diagnosticEntries = diagnostics?.ToArray() ?? [];
        UnsupportedSpineFeature[] unsupportedFeatureEntries = unsupportedFeatures?.ToArray() ?? [];
        runtime ??= CreateRuntime();

        string lowerDisplayName = displayName.ToLowerInvariant();
        return new SpineProjectSession(
            Guid.NewGuid(),
            new SpineProjectReference(displayName, $"{lowerDisplayName}.json", $"{lowerDisplayName}.atlas"),
            new SpineAssetFileSet($"{lowerDisplayName}.json", $"{lowerDisplayName}.atlas", [$"{lowerDisplayName}.png"]),
            runtime,
            new SpineVersionMatch("4.1.00", runtime.RuntimeId, true, Array.Empty<ViewerDiagnostic>()),
            resolvedPlaybackState,
            resolvedViewportState,
            resolvedInspection,
            unsupportedFeatureEntries,
            selectedSkinName,
            diagnosticEntries);
    }

    internal static WorkspaceState CreateWorkspaceState(
        SpineProjectSession? currentSession = null,
        IEnumerable<SpineProjectReference>? recentFiles = null,
        IEnumerable<ViewerDiagnostic>? diagnostics = null,
        string? statusText = null,
        bool isBusy = false)
    {
        SpineProjectReference[] recentFileEntries = recentFiles?.ToArray() ?? [];
        ViewerDiagnostic[] diagnosticEntries = diagnostics?.ToArray() ?? [];

        return new WorkspaceState(
            currentSession,
            recentFileEntries,
            diagnosticEntries,
            statusText ?? (currentSession is null ? "Ready." : $"Opened {currentSession.Project.DisplayName}."),
            isBusy);
    }
}
