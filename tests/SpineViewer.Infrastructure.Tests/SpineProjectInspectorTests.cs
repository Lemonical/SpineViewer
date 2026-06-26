using SpineViewer.Core.Models;
using SpineViewer.Infrastructure.Spine.Loading;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class SpineProjectInspectorTests
{
    [Fact]
    public async Task InspectAsync_WithJsonSkeleton_ParsesStructuredMetadataAsync()
    {
        string tempDirectory = CreateTempDirectory();

        try
        {
            string skeletonPath = Path.Combine(tempDirectory, "hero.json");
            string atlasPath = Path.Combine(tempDirectory, "hero.atlas");
            await File.WriteAllTextAsync(
                skeletonPath,
                """
                {
                  "skeleton": {
                    "spine": "4.1.00",
                    "images": "./images",
                    "audio": "./audio",
                    "width": 512,
                    "height": 256,
                    "fps": 30
                  },
                  "bones": [
                    { "name": "root" },
                    { "name": "hip", "parent": "root", "length": 12, "x": 1, "y": 2, "rotation": 3 }
                  ],
                  "slots": [
                    { "name": "body", "bone": "hip", "attachment": "body-region", "blend": "normal" }
                  ],
                  "skins": [
                    {
                      "name": "default",
                      "attachments": {
                        "body": {
                          "body-region": {
                            "type": "region",
                            "path": "body-region",
                            "width": 64,
                            "height": 32
                          }
                        }
                      }
                    }
                  ],
                  "animations": {
                    "idle": {
                      "bones": {
                        "hip": {
                          "rotate": [
                            { "time": 0.0, "angle": 0 },
                            { "time": 1.0, "angle": 15 }
                          ]
                        }
                      }
                    }
                  }
                }
                """);
            await File.WriteAllTextAsync(
                atlasPath,
                """
                hero.png
                size: 512, 256
                format: RGBA8888
                filter: Linear,Linear
                repeat: none
                body-region
                rotate: false
                size: 64, 32
                orig: 64, 32
                
                """);

            SpineProjectInspector inspector = new();
            SpineProjectInspection inspection = await inspector.InspectAsync(
                new SpineAssetFileSet(skeletonPath, atlasPath, [Path.Combine(tempDirectory, "hero.png")]),
                CancellationToken.None);

            Assert.Equal("4.1.00", inspection.ExportMetadata.ExportVersion);
            Assert.Equal("./images", inspection.ExportMetadata.ImagesPath);
            Assert.Equal("./audio", inspection.ExportMetadata.AudioPath);
            Assert.Equal(512, inspection.ExportMetadata.Width);
            Assert.Equal(256, inspection.ExportMetadata.Height);
            Assert.Equal(30, inspection.ExportMetadata.FramesPerSecond);
            Assert.Equal("idle", Assert.Single(inspection.Animations).Name);
            Assert.Equal(TimeSpan.FromSeconds(1), Assert.Single(inspection.Animations).Duration);
            Assert.Equal("default", Assert.Single(inspection.Skins).Name);
            Assert.Equal("body-region", Assert.Single(inspection.Attachments).Name);
            Assert.Equal("hero.png", Assert.Single(inspection.AtlasPages).Name);
            Assert.Equal("body-region", Assert.Single(inspection.AtlasRegions).Name);
            Assert.Empty(inspection.Diagnostics);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task InspectAsync_WithBinarySkeleton_ReturnsLimitedInspectionDiagnosticAsync()
    {
        string tempDirectory = CreateTempDirectory();

        try
        {
            string skeletonPath = Path.Combine(tempDirectory, "hero.skel");
            string atlasPath = Path.Combine(tempDirectory, "hero.atlas");
            await File.WriteAllBytesAsync(skeletonPath, [0x01, 0x02, 0x03]);
            await File.WriteAllTextAsync(
                atlasPath,
                """
                hero.png
                size: 256, 128
                region
                rotate: false
                size: 32, 16
                orig: 32, 16
                
                """);

            SpineProjectInspector inspector = new();
            SpineProjectInspection inspection = await inspector.InspectAsync(
                new SpineAssetFileSet(skeletonPath, atlasPath, [Path.Combine(tempDirectory, "hero.png")]),
                CancellationToken.None);

            Assert.Contains(
                inspection.Diagnostics,
                static diagnostic => diagnostic.Code == "inspection-binary-skeleton-limited");
            Assert.Empty(inspection.Animations);
            Assert.Equal("hero.png", Assert.Single(inspection.AtlasPages).Name);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    private static string CreateTempDirectory()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), $"spineviewer-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }
}
