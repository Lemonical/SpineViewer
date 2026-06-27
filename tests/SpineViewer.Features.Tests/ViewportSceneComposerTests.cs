using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class ViewportSceneComposerTests
{
    [Fact]
    public void Compose_WithActiveSessionAndEnabledOverlays_BuildsExpectedScene()
    {
        ViewportSceneComposer composer = new(
            new ViewportCameraService(),
            CreateOverlaySources());
        WorkspaceState workspaceState = new(
            CreateSession(
                "Hero",
                new PlaybackState(),
                new ViewportState(1.5, 20.0, -10.0, false, true, true, true)),
            Array.Empty<SpineProjectReference>(),
            Array.Empty<ViewerDiagnostic>(),
            "Opened Hero.",
            false);

        ViewportRenderScene scene = composer.Compose(workspaceState, new ViewportHostLayout(800.0, 600.0), 7);

        Assert.True(scene.HasActiveSession);
        Assert.Equal("Hero", scene.SessionName);
        Assert.Equal(7, scene.FrameVersion);
        Assert.Equal(1.5, scene.Transform.Scale);
        Assert.Equal(420.0, scene.Transform.TranslateX);
        Assert.Equal(290.0, scene.Transform.TranslateY);
        Assert.Equal(8, scene.WorldLines.Count);
        Assert.NotNull(scene.ContentBounds);
        Assert.Equal(-90.0, Assert.IsType<ViewportContentBounds>(scene.ContentBounds).MinimumX);
    }

    private static IReadOnlyList<IViewportOverlaySource> CreateOverlaySources()
    {
        ViewportInspectionOverlayFactory inspectionOverlayFactory = new();

        return
        [
            new GridOverlaySource(),
            new SessionPlaceholderOverlaySource(inspectionOverlayFactory),
            new BoneOverlaySource(inspectionOverlayFactory),
            new BoundsOverlaySource(inspectionOverlayFactory),
            new OriginOverlaySource(),
        ];
    }

    private static SpineProjectSession CreateSession(
        string displayName,
        PlaybackState playbackState,
        ViewportState viewportState)
    {
        return new SpineProjectSession(
            Guid.NewGuid(),
            new SpineProjectReference(
                displayName,
                $"{displayName.ToLowerInvariant()}.json",
                $"{displayName.ToLowerInvariant()}.atlas"),
            new SpineAssetFileSet(
                $"{displayName.ToLowerInvariant()}.json",
                $"{displayName.ToLowerInvariant()}.atlas",
                [$"{displayName.ToLowerInvariant()}.png"]),
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)])),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            playbackState,
            viewportState,
            Array.Empty<ViewerDiagnostic>());
    }
}
