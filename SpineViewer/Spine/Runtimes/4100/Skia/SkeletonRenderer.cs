/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated September 24, 2021. Replaces all prior versions.
 *
 * Copyright (c) 2013-2021, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace Spine_4100.Skia;

public class SkeletonRenderer
{
    private readonly int[] _quadTriangles = [0, 1, 2, 2, 3, 0];

    public void Draw(Skeleton skeleton, SKCanvas canvas)
    {
        var drawOrder = skeleton.DrawOrder;
        var drawOrderItems = skeleton.DrawOrder.Items;
        float skeletonR = skeleton.R,
            skeletonG = skeleton.G,
            skeletonB = skeleton.B,
            skeletonA = skeleton.A;
        var clipper = new SkeletonClipping();

        SKBitmap? rendererObject = null;
        SKShader? sKShader = null;
        using var blur = SKImageFilter.CreateBlur(15f, 15f);

        for (int i = 0, n = drawOrder.Count; i < n; i++)
        {
            var slot = drawOrderItems[i];
            var attachment = slot.Attachment;

            float attachmentColorR = 0,
                attachmentColorG = 0,
                attachmentColorB = 0,
                attachmentColorA = 0;
            int verticesCount = 0,
                indicesCount = 0;
            int[] indices = [];
            float[] uvs = [],
                vertices = new float[8];
            bool premul;

            void SetAttachmentColors(float r, float g, float b, float a)
            {
                attachmentColorR = r;
                attachmentColorG = g;
                attachmentColorB = b;
                attachmentColorA = a;
            }

            void SetRendererObject(SKBitmap bitmap)
            {
                if (rendererObject == bitmap) return;
                rendererObject = bitmap;
                sKShader?.Dispose();
                sKShader = rendererObject.ToShader();
            }

            // Attachments
            switch (attachment)
            {
                case RegionAttachment regionAttachment when GetRendererObject(regionAttachment.Region) is SKBitmap rendererObj:
                    SetAttachmentColors(regionAttachment.R, regionAttachment.G, regionAttachment.B, regionAttachment.A);
                    SetRendererObject(rendererObj);
                    verticesCount = 4;
                    regionAttachment.ComputeWorldVertices(slot, vertices, 0);
                    indicesCount = 6;
                    indices = _quadTriangles;
                    uvs = regionAttachment.UVs;
                    premul = rendererObject?.AlphaType == SKAlphaType.Premul;
                    break;
                case MeshAttachment meshAttachment when GetRendererObject(meshAttachment.Region) is SKBitmap rendererObj:
                    SetAttachmentColors(meshAttachment.R, meshAttachment.G, meshAttachment.B, meshAttachment.A);
                    SetRendererObject(rendererObj);
                    var vertexCount = meshAttachment.WorldVerticesLength;

                    if (vertices.Length < vertexCount)
                        vertices = new float[vertexCount];

                    verticesCount = vertexCount >> 1;
                    meshAttachment.ComputeWorldVertices(slot, vertices);
                    indicesCount = meshAttachment.Triangles.Length;
                    indices = meshAttachment.Triangles;
                    uvs = meshAttachment.UVs;
                    premul = rendererObject?.AlphaType == SKAlphaType.Premul;
                    break;
                case ClippingAttachment clippingAttachment:
                    clipper.ClipStart(slot, clippingAttachment);
                    continue;
                default: continue;
            }

            // Blend mode
            SKBlendMode blendMode = SKBlendMode.SrcOver;
            switch (slot.Data.BlendMode)
            {
                case BlendMode.Additive:
                    blendMode = SKBlendMode.Plus;
                    break;
                case BlendMode.Multiply:
                    blendMode = SKBlendMode.Multiply;
                    break;
                case BlendMode.Screen:
                    blendMode = SKBlendMode.Screen;
                    break;
            }

            // Calculate color
            float a = skeletonA * slot.A * attachmentColorA;
            SKColorF color = new(
                skeletonR * slot.R * attachmentColorR * (premul ? a : 1),
                skeletonG * slot.G * attachmentColorG * (premul ? a : 1),
                skeletonB * slot.B * attachmentColorB * (premul ? a : 1),
                a),
                dark = slot.HasSecondColor
                ? new(slot.R2 * a, slot.G2 * a, slot.B2 * a, premul ? 1 : 0)
                : new(0, 0, 0, premul ? 1 : 0);

            // Clip
            if (clipper.IsClipping)
            {
                clipper.ClipTriangles(vertices, verticesCount << 1, indices, indicesCount, uvs);
                vertices = clipper.ClippedVertices.Items;
                verticesCount = clipper.ClippedVertices.Count >> 1;
                indices = clipper.ClippedTriangles.Items;
                indicesCount = clipper.ClippedTriangles.Count;
                uvs = clipper.ClippedUVs.Items;
            }

            if (verticesCount == 0 || indicesCount == 0)
                continue;

            List<SKPoint> positions = new(vertices.Length / 2),
                textures = new(uvs.Length / 2);

            for (var index = 0; index < vertices.Length; index += 2)
            {
                positions.Add(new(vertices[index], vertices[index + 1]));
                textures.Add(new(rendererObject!.Width * uvs[index], rendererObject.Height * uvs[index + 1]));
            };

            // TODO: Conditional blur setting
            using var paint = new SKPaint();
            using var colorFilter = SKColorFilter.CreateLighting((SKColor)color, (SKColor)dark);
            paint.ColorFilter = colorFilter;
            paint.BlendMode = blendMode;
            paint.ColorF = color;
            paint.FilterQuality = SKFilterQuality.High;
            paint.IsAntialias = true;
            paint.Shader = sKShader;
            if (blendMode == SKBlendMode.Plus)
                paint.ImageFilter = blur;

            canvas.DrawVertices(SKVertices.CreateCopy(SKVertexMode.Triangles,
                [.. positions], [.. textures], null, [.. indices.Select(x => (ushort)x)]), SKBlendMode.SrcOver, paint);

            clipper.ClipEnd(slot);
        }
        clipper.ClipEnd();
        sKShader?.Dispose();
    }

    private SKBitmap? GetRendererObject(TextureRegion region)
    {
        if (region is AtlasRegion atlasRegion &&
            atlasRegion.page.rendererObject is SKBitmap rendererObject)
            return rendererObject;
        return null;
    }
}