using Avalonia;
using SkiaSharp;
using System;
using System.IO;

namespace Spine_4100.Skia;

public class SpineTextureLoader : TextureLoader
{
    public void Load(AtlasPage page, string path)
    {
        using var stream = File.OpenRead(path);
        var bitmap = SKBitmap.Decode(stream) 
            ?? throw new InvalidOperationException("Failed to load bitmap: " + path);

        // Dispose?
        page.rendererObject = bitmap;

        // Very old atlas files expected the texture's actual size to be used at runtime.
        if (page.width == 0 || page.height == 0)
        {
            page.width = bitmap.Width;
            page.height = bitmap.Height;
        }
    }

    public void Unload(object texture)
    {
        if (texture is SKImage image)
            image.Dispose();
    }
}