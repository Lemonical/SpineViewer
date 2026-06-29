using SkiaSharp;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;
using SpineViewer.Features.Viewport.Rendering;
using SpineViewer.Features.Viewport.Services;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class SpineRuntimePreviewRendererTests : IDisposable
{
    private readonly string _temporaryDirectoryPath;

    public SpineRuntimePreviewRendererTests()
    {
        _temporaryDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "SpineViewer",
            "tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_temporaryDirectoryPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectoryPath))
        {
            Directory.Delete(_temporaryDirectoryPath, true);
        }
    }

    [Fact]
    public void TryRender_WithValidSession_DrawsAtlasTextureContent()
    {
        string atlasPath = Path.Combine(_temporaryDirectoryPath, "hero.atlas");
        string skeletonPath = Path.Combine(_temporaryDirectoryPath, "hero.json");
        string texturePath = Path.Combine(_temporaryDirectoryPath, "hero.png");

        WriteTexture(texturePath);
        File.WriteAllText(atlasPath, CreateAtlasText());
        File.WriteAllText(skeletonPath, CreateSkeletonJson());

        SpineProjectSession session = CreateSession(skeletonPath, atlasPath, texturePath);

        using SpineRuntimePreviewRenderer renderer = new();
        using SKBitmap bitmap = new(160, 160, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas canvas = new(bitmap);

        canvas.Clear(SKColors.Transparent);
        bool rendered = renderer.TryRender(
            canvas,
            session,
            new ViewportRenderTransform(1.0, 80.0, 80.0));

        SKColor centerPixel = bitmap.GetPixel(80, 80);

        Assert.True(rendered, renderer.LastErrorMessage);
        Assert.True(centerPixel.Red > 0);
        Assert.True(centerPixel.Alpha > 0);
    }

    [Fact]
    public void MeasureContentBounds_WithValidSession_ReturnsPositiveBounds()
    {
        string atlasPath = Path.Combine(_temporaryDirectoryPath, "hero.atlas");
        string skeletonPath = Path.Combine(_temporaryDirectoryPath, "hero.json");
        string texturePath = Path.Combine(_temporaryDirectoryPath, "hero.png");

        WriteTexture(texturePath);
        File.WriteAllText(atlasPath, CreateAtlasText());
        File.WriteAllText(skeletonPath, CreateSkeletonJson());

        SpineProjectSession session = CreateSession(skeletonPath, atlasPath, texturePath);

        using SpineRuntimePreviewBoundsService boundsService = new();
        ViewportContentBounds? contentBounds = boundsService.MeasureContentBounds(session);

        Assert.NotNull(contentBounds);
        Assert.True(Assert.IsType<ViewportContentBounds>(contentBounds).Width > 0.0);
        Assert.True(Assert.IsType<ViewportContentBounds>(contentBounds).Height > 0.0);
    }

    private static string CreateAtlasText()
    {
        return """
hero.png
size: 64, 64
format: RGBA8888
filter: Linear,Linear
repeat: none
hero
  bounds: 0,0,64,64
  offsets: 0,0,64,64
  rotate: false
  index: -1
""";
    }

    private static string CreateSkeletonJson()
    {
        return """
{
  "skeleton": {
    "hash": "fixture",
    "spine": "4.1.00"
  },
  "bones": [
    { "name": "root" }
  ],
  "slots": [
    { "name": "body", "bone": "root", "attachment": "hero" }
  ],
  "skins": [
    {
      "name": "default",
      "attachments": {
        "body": {
          "hero": {
            "type": "region",
            "path": "hero",
            "x": 0,
            "y": 0,
            "width": 64,
            "height": 64
          }
        }
      }
    }
  ],
  "animations": {
    "idle": {
      "bones": {
        "root": {
          "rotate": [
            { "time": 0, "angle": 0 }
          ]
        }
      }
    }
  }
}
""";
    }

    private static SpineProjectSession CreateSession(
        string skeletonPath,
        string atlasPath,
        string texturePath)
    {
        return new SpineProjectSession(
            Guid.NewGuid(),
            new SpineProjectReference("Hero", skeletonPath, atlasPath),
            new SpineAssetFileSet(skeletonPath, atlasPath, [texturePath]),
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities(
                [
                    new(SpineRuntimeFeature.JsonSkeleton, true),
                    new(SpineRuntimeFeature.Meshes, true),
                    new(SpineRuntimeFeature.MultipleTracks, true),
                    new(SpineRuntimeFeature.Clipping, true),
                ])),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            new PlaybackState(
                PlaybackTransportStatus.Stopped,
                true,
                1.0,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1.0),
                [
                    new AnimationTrackState(
                        Guid.NewGuid(),
                        0,
                        "idle",
                        true,
                        1.0,
                        TimeSpan.Zero,
                        true),
                ]),
            new ViewportState(),
            SpineProjectInspection.Empty,
            Array.Empty<UnsupportedSpineFeature>(),
            "default",
            Array.Empty<ViewerDiagnostic>());
    }

    private static void WriteTexture(string texturePath)
    {
        using SKBitmap bitmap = new(64, 64, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKCanvas canvas = new(bitmap);

        canvas.Clear(new SKColor(0xD4, 0x91, 0x5A, 0xFF));

        using FileStream stream = File.Create(texturePath);
        using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(stream);
    }
}
