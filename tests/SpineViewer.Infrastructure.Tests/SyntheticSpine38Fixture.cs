using SpineViewer.Core.Models;

namespace SpineViewer.Infrastructure.Tests;

internal sealed class SyntheticSpine38Fixture : IDisposable
{
    private readonly string _temporaryDirectory;

    public SyntheticSpine38Fixture()
    {
        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            "SpineViewer.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_temporaryDirectory);

        SkeletonPath = WriteFile(
            "hero.json",
            """
            {
              "skeleton": {
                "hash": "",
                "spine": "3.8.95",
                "images": "./images",
                "audio": "./audio",
                "width": 256,
                "height": 128,
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
        AtlasPath = WriteFile(
            "hero.atlas",
            """
            hero.png
            size: 256, 128
            format: RGBA8888
            filter: Linear,Linear
            repeat: none
            body-region
              rotate: false
              xy: 0, 0
              size: 64, 32
              orig: 64, 32
              offset: 0, 0
              index: -1
            
            """);
        TexturePaths = [WriteFile("hero.png", "png")];
    }

    public string SkeletonPath { get; }

    public string AtlasPath { get; }

    public string[] TexturePaths { get; }

    public SpineAssetFileSet CreateAssetFileSet()
    {
        return new SpineAssetFileSet(SkeletonPath, AtlasPath, TexturePaths);
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, true);
        }
    }

    private string WriteFile(string relativePath, string content)
    {
        string fullPath = Path.Combine(_temporaryDirectory, relativePath);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }
}
