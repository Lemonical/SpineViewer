using System.Reflection;
using SpineViewer.Core.Models;
using Spine38 = Spine_3895;
using Spine41 = Spine_4100;

namespace SpineViewer.Infrastructure.Spine.Loading;

internal static class LegacyRuntimeAssetLoader
{
    internal enum LoadFailureStage
    {
        Atlas,
        DependentTexture,
        Skeleton,
    }

    internal sealed class LoadException : Exception
    {
        public LoadException(
            LoadFailureStage stage,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            Stage = stage;
        }

        public LoadFailureStage Stage { get; }
    }

    public static IReadOnlyList<UnsupportedSpineFeature> ValidateLoad(
        string runtimeId,
        SpineAssetFileSet assetFileSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeId);
        ArgumentNullException.ThrowIfNull(assetFileSet);

        return runtimeId switch
        {
            "spine-3.8.95" => ValidateLoad3895(assetFileSet),
            "spine-4.1.00" => ValidateLoad4100(assetFileSet),
            _ => throw new ArgumentOutOfRangeException(nameof(runtimeId), runtimeId, "Unsupported runtime identifier."),
        };
    }

    public static SpineRuntimeSkeletonInspectionData InspectBinarySkeleton(
        string runtimeId,
        SpineAssetFileSet assetFileSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeId);
        ArgumentNullException.ThrowIfNull(assetFileSet);

        return runtimeId switch
        {
            "spine-3.8.95" => InspectBinarySkeleton3895(assetFileSet),
            "spine-4.1.00" => InspectBinarySkeleton4100(assetFileSet),
            _ => throw new ArgumentOutOfRangeException(nameof(runtimeId), runtimeId, "Unsupported runtime identifier."),
        };
    }

    private static IReadOnlyList<UnsupportedSpineFeature> ValidateLoad3895(SpineAssetFileSet assetFileSet)
    {
        Spine38.Atlas atlas = CreateAtlas3895(assetFileSet.AtlasPath);
        try
        {
            Spine38.SkeletonData skeletonData = LoadSkeletonData3895(atlas, assetFileSet.SkeletonPath);
            return DetectUnsupportedFeatures3895(skeletonData);
        }
        finally
        {
            atlas.Dispose();
        }
    }

    private static IReadOnlyList<UnsupportedSpineFeature> ValidateLoad4100(SpineAssetFileSet assetFileSet)
    {
        Spine41.Atlas atlas = CreateAtlas4100(assetFileSet.AtlasPath);
        try
        {
            Spine41.SkeletonData skeletonData = LoadSkeletonData4100(atlas, assetFileSet.SkeletonPath);
            return Array.Empty<UnsupportedSpineFeature>();
        }
        finally
        {
            atlas.Dispose();
        }
    }

    private static SpineRuntimeSkeletonInspectionData InspectBinarySkeleton3895(SpineAssetFileSet assetFileSet)
    {
        Spine38.Atlas atlas = CreateAtlas3895(assetFileSet.AtlasPath);
        try
        {
            Spine38.SkeletonData skeletonData = LoadSkeletonData3895(atlas, assetFileSet.SkeletonPath);
            return CreateInspectionData3895(skeletonData);
        }
        finally
        {
            atlas.Dispose();
        }
    }

    private static SpineRuntimeSkeletonInspectionData InspectBinarySkeleton4100(SpineAssetFileSet assetFileSet)
    {
        Spine41.Atlas atlas = CreateAtlas4100(assetFileSet.AtlasPath);
        try
        {
            Spine41.SkeletonData skeletonData = LoadSkeletonData4100(atlas, assetFileSet.SkeletonPath);
            return CreateInspectionData4100(skeletonData);
        }
        finally
        {
            atlas.Dispose();
        }
    }

    private static Spine38.Atlas CreateAtlas3895(string atlasPath)
    {
        try
        {
            return new Spine38.Atlas(atlasPath, new ValidatingTextureLoader3895());
        }
        catch (LoadException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new LoadException(
                LoadFailureStage.Atlas,
                $"Failed to load atlas '{atlasPath}'.",
                exception);
        }
    }

    private static Spine41.Atlas CreateAtlas4100(string atlasPath)
    {
        try
        {
            return new Spine41.Atlas(atlasPath, new ValidatingTextureLoader4100());
        }
        catch (LoadException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new LoadException(
                LoadFailureStage.Atlas,
                $"Failed to load atlas '{atlasPath}'.",
                exception);
        }
    }

    private static Spine38.SkeletonData LoadSkeletonData3895(
        Spine38.Atlas atlas,
        string skeletonPath)
    {
        try
        {
            Spine38.AtlasAttachmentLoader attachmentLoader = new(atlas);

            if (string.Equals(Path.GetExtension(skeletonPath), ".json", StringComparison.OrdinalIgnoreCase))
            {
                Spine38.SkeletonJson skeletonJson = new(attachmentLoader)
                {
                    Scale = 1.0f,
                };
                return skeletonJson.ReadSkeletonData(skeletonPath);
            }

            Spine38.SkeletonBinary skeletonBinary = new(attachmentLoader)
            {
                Scale = 1.0f,
            };
            return skeletonBinary.ReadSkeletonData(skeletonPath);
        }
        catch (Exception exception)
        {
            throw new LoadException(
                LoadFailureStage.Skeleton,
                $"Failed to load skeleton '{skeletonPath}'.",
                exception);
        }
    }

    private static Spine41.SkeletonData LoadSkeletonData4100(
        Spine41.Atlas atlas,
        string skeletonPath)
    {
        try
        {
            Spine41.AtlasAttachmentLoader attachmentLoader = new(atlas);
            Spine41.SkeletonLoader loader = string.Equals(Path.GetExtension(skeletonPath), ".json", StringComparison.OrdinalIgnoreCase)
                ? new Spine41.SkeletonJson(attachmentLoader)
                : new Spine41.SkeletonBinary(attachmentLoader);
            loader.Scale = 1.0f;
            return loader.ReadSkeletonData(skeletonPath);
        }
        catch (Exception exception)
        {
            throw new LoadException(
                LoadFailureStage.Skeleton,
                $"Failed to load skeleton '{skeletonPath}'.",
                exception);
        }
    }

    private static IReadOnlyList<UnsupportedSpineFeature> DetectUnsupportedFeatures3895(Spine38.SkeletonData skeletonData)
    {
        foreach (Spine38.Skin skin in skeletonData.Skins)
        {
            foreach (Spine38.Skin.SkinEntry entry in skin.GetAttachments())
            {
                if (entry.Attachment is Spine38.ClippingAttachment)
                {
                    return
                    [
                        new UnsupportedSpineFeature(
                            "clipping",
                            "Clipping",
                            "This skeleton uses clipping attachments, which the Spine 3.8.95 runtime lane does not currently guarantee."),
                    ];
                }
            }
        }

        return Array.Empty<UnsupportedSpineFeature>();
    }

    private static SpineRuntimeSkeletonInspectionData CreateInspectionData3895(Spine38.SkeletonData skeletonData)
    {
        List<SpineAnimationInfo> animations = [];
        foreach (Spine38.Animation animation in skeletonData.Animations)
        {
            animations.Add(
                new SpineAnimationInfo(
                    animation.Name,
                    TimeSpan.FromSeconds(animation.Duration),
                    animation.Timelines.Count,
                    CountTimelineKeyframes(animation.Timelines)));
        }

        List<SpineBoneInfo> bones = [];
        foreach (Spine38.BoneData bone in skeletonData.Bones)
        {
            bones.Add(
                new SpineBoneInfo(
                    bone.Name,
                    bone.Parent?.Name,
                    bone.Length,
                    bone.X,
                    bone.Y,
                    bone.Rotation,
                    bone.ScaleX,
                    bone.ScaleY,
                    CalculateDepth3895(bone)));
        }

        List<SpineSlotInfo> slots = [];
        foreach (Spine38.SlotData slot in skeletonData.Slots)
        {
            slots.Add(
                new SpineSlotInfo(
                    slot.Name,
                    slot.BoneData.Name,
                    slot.AttachmentName,
                    slot.BlendMode.ToString()));
        }

        List<SpineSkinInfo> skins = [];
        List<SpineAttachmentInfo> attachments = [];

        foreach (Spine38.Skin skin in skeletonData.Skins)
        {
            ICollection<Spine38.Skin.SkinEntry> skinEntries = skin.GetAttachments();
            HashSet<int> slotIndexes = [];

            foreach (Spine38.Skin.SkinEntry entry in skinEntries)
            {
                slotIndexes.Add(entry.SlotIndex);
                attachments.Add(
                    CreateAttachmentInfo3895(
                        skin.Name,
                        GetSlotName3895(skeletonData, entry.SlotIndex),
                        entry.Name,
                        entry.Attachment));
            }

            skins.Add(
                new SpineSkinInfo(
                    skin.Name,
                    slotIndexes.Count,
                    skinEntries.Count,
                    ReferenceEquals(skin, skeletonData.DefaultSkin)));
        }

        return new SpineRuntimeSkeletonInspectionData(
            NormalizeOptionalText(skeletonData.Version),
            NormalizeOptionalText(skeletonData.ImagesPath),
            NormalizeOptionalText(skeletonData.AudioPath),
            NormalizeOptionalNumber(skeletonData.Width),
            NormalizeOptionalNumber(skeletonData.Height),
            NormalizeOptionalNumber(skeletonData.Fps),
            animations,
            skins,
            bones,
            slots,
            attachments);
    }

    private static SpineRuntimeSkeletonInspectionData CreateInspectionData4100(Spine41.SkeletonData skeletonData)
    {
        List<SpineAnimationInfo> animations = [];
        foreach (Spine41.Animation animation in skeletonData.Animations)
        {
            animations.Add(
                new SpineAnimationInfo(
                    animation.Name,
                    TimeSpan.FromSeconds(animation.Duration),
                    animation.Timelines.Count,
                    CountTimelineKeyframes(animation.Timelines)));
        }

        List<SpineBoneInfo> bones = [];
        foreach (Spine41.BoneData bone in skeletonData.Bones)
        {
            bones.Add(
                new SpineBoneInfo(
                    bone.Name,
                    bone.Parent?.Name,
                    bone.Length,
                    bone.X,
                    bone.Y,
                    bone.Rotation,
                    bone.ScaleX,
                    bone.ScaleY,
                    CalculateDepth4100(bone)));
        }

        List<SpineSlotInfo> slots = [];
        foreach (Spine41.SlotData slot in skeletonData.Slots)
        {
            slots.Add(
                new SpineSlotInfo(
                    slot.Name,
                    slot.BoneData.Name,
                    slot.AttachmentName,
                    slot.BlendMode.ToString()));
        }

        List<SpineSkinInfo> skins = [];
        List<SpineAttachmentInfo> attachments = [];

        foreach (Spine41.Skin skin in skeletonData.Skins)
        {
            ICollection<Spine41.Skin.SkinEntry> skinEntries = skin.Attachments;
            HashSet<int> slotIndexes = [];

            foreach (Spine41.Skin.SkinEntry entry in skinEntries)
            {
                slotIndexes.Add(entry.SlotIndex);
                attachments.Add(
                    CreateAttachmentInfo4100(
                        skin.Name,
                        GetSlotName4100(skeletonData, entry.SlotIndex),
                        entry.Name,
                        entry.Attachment));
            }

            skins.Add(
                new SpineSkinInfo(
                    skin.Name,
                    slotIndexes.Count,
                    skinEntries.Count,
                    ReferenceEquals(skin, skeletonData.DefaultSkin)));
        }

        return new SpineRuntimeSkeletonInspectionData(
            NormalizeOptionalText(skeletonData.Version),
            NormalizeOptionalText(skeletonData.ImagesPath),
            NormalizeOptionalText(skeletonData.AudioPath),
            NormalizeOptionalNumber(skeletonData.Width),
            NormalizeOptionalNumber(skeletonData.Height),
            NormalizeOptionalNumber(skeletonData.Fps),
            animations,
            skins,
            bones,
            slots,
            attachments);
    }

    private static string GetSlotName3895(Spine38.SkeletonData skeletonData, int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < skeletonData.Slots.Count
            ? skeletonData.Slots.Items[slotIndex].Name
            : $"slot-{slotIndex}";
    }

    private static string GetSlotName4100(Spine41.SkeletonData skeletonData, int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < skeletonData.Slots.Count
            ? skeletonData.Slots.Items[slotIndex].Name
            : $"slot-{slotIndex}";
    }

    private static SpineAttachmentInfo CreateAttachmentInfo3895(
        string skinName,
        string slotName,
        string attachmentName,
        Spine38.Attachment attachment)
    {
        return attachment switch
        {
            Spine38.RegionAttachment regionAttachment => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                "region",
                NormalizeOptionalText(regionAttachment.Path),
                regionAttachment.X,
                regionAttachment.Y,
                regionAttachment.Rotation,
                regionAttachment.ScaleX,
                regionAttachment.ScaleY,
                NormalizeOptionalNumber(regionAttachment.Width),
                NormalizeOptionalNumber(regionAttachment.Height),
                0,
                0),
            Spine38.MeshAttachment meshAttachment => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                "mesh",
                NormalizeOptionalText(meshAttachment.Path),
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                NormalizeOptionalNumber(meshAttachment.Width),
                NormalizeOptionalNumber(meshAttachment.Height),
                meshAttachment.WorldVerticesLength,
                meshAttachment.Triangles.Length / 3),
            Spine38.BoundingBoxAttachment boundingBoxAttachment => CreateVertexAttachmentInfo3895(
                skinName,
                slotName,
                attachmentName,
                "boundingbox",
                boundingBoxAttachment),
            Spine38.ClippingAttachment clippingAttachment => CreateVertexAttachmentInfo3895(
                skinName,
                slotName,
                attachmentName,
                "clipping",
                clippingAttachment),
            Spine38.PathAttachment pathAttachment => CreateVertexAttachmentInfo3895(
                skinName,
                slotName,
                attachmentName,
                "path",
                pathAttachment),
            Spine38.PointAttachment pointAttachment => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                "point",
                null,
                pointAttachment.X,
                pointAttachment.Y,
                pointAttachment.Rotation,
                1.0,
                1.0,
                null,
                null,
                0,
                0),
            _ => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                NormalizeAttachmentTypeName(attachment.GetType().Name),
                null,
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                null,
                null,
                0,
                0),
        };
    }

    private static SpineAttachmentInfo CreateAttachmentInfo4100(
        string skinName,
        string slotName,
        string attachmentName,
        Spine41.Attachment attachment)
    {
        return attachment switch
        {
            Spine41.RegionAttachment regionAttachment => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                "region",
                NormalizeOptionalText(regionAttachment.Path),
                regionAttachment.X,
                regionAttachment.Y,
                regionAttachment.Rotation,
                regionAttachment.ScaleX,
                regionAttachment.ScaleY,
                NormalizeOptionalNumber(regionAttachment.Width),
                NormalizeOptionalNumber(regionAttachment.Height),
                0,
                0),
            Spine41.MeshAttachment meshAttachment => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                "mesh",
                NormalizeOptionalText(meshAttachment.Path),
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                NormalizeOptionalNumber(meshAttachment.Width),
                NormalizeOptionalNumber(meshAttachment.Height),
                meshAttachment.WorldVerticesLength,
                meshAttachment.Triangles.Length / 3),
            Spine41.BoundingBoxAttachment boundingBoxAttachment => CreateVertexAttachmentInfo4100(
                skinName,
                slotName,
                attachmentName,
                "boundingbox",
                boundingBoxAttachment),
            Spine41.ClippingAttachment clippingAttachment => CreateVertexAttachmentInfo4100(
                skinName,
                slotName,
                attachmentName,
                "clipping",
                clippingAttachment),
            Spine41.PathAttachment pathAttachment => CreateVertexAttachmentInfo4100(
                skinName,
                slotName,
                attachmentName,
                "path",
                pathAttachment),
            Spine41.PointAttachment pointAttachment => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                "point",
                null,
                pointAttachment.X,
                pointAttachment.Y,
                pointAttachment.Rotation,
                1.0,
                1.0,
                null,
                null,
                0,
                0),
            _ => new SpineAttachmentInfo(
                skinName,
                slotName,
                attachmentName,
                NormalizeAttachmentTypeName(attachment.GetType().Name),
                null,
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                null,
                null,
                0,
                0),
        };
    }

    private static SpineAttachmentInfo CreateVertexAttachmentInfo3895(
        string skinName,
        string slotName,
        string attachmentName,
        string attachmentType,
        Spine38.VertexAttachment attachment)
    {
        return new SpineAttachmentInfo(
            skinName,
            slotName,
            attachmentName,
            attachmentType,
            null,
            0.0,
            0.0,
            0.0,
            1.0,
            1.0,
            null,
            null,
            attachment.WorldVerticesLength,
            0);
    }

    private static SpineAttachmentInfo CreateVertexAttachmentInfo4100(
        string skinName,
        string slotName,
        string attachmentName,
        string attachmentType,
        Spine41.VertexAttachment attachment)
    {
        return new SpineAttachmentInfo(
            skinName,
            slotName,
            attachmentName,
            attachmentType,
            null,
            0.0,
            0.0,
            0.0,
            1.0,
            1.0,
            null,
            null,
            attachment.WorldVerticesLength,
            0);
    }

    private static int CalculateDepth3895(Spine38.BoneData bone)
    {
        int depth = 0;
        Spine38.BoneData? current = bone.Parent;
        while (current is not null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    private static int CalculateDepth4100(Spine41.BoneData bone)
    {
        int depth = 0;
        Spine41.BoneData? current = bone.Parent;
        while (current is not null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    private static int CountTimelineKeyframes<TTimeline>(IEnumerable<TTimeline> timelines)
    {
        int keyframeCount = 0;

        foreach (TTimeline timeline in timelines)
        {
            if (timeline is null)
            {
                continue;
            }

            PropertyInfo? property = timeline.GetType().GetProperty(
                "FrameCount",
                BindingFlags.Instance | BindingFlags.Public);

            if (property?.PropertyType == typeof(int) &&
                property.GetValue(timeline) is int frameCount)
            {
                keyframeCount += frameCount;
            }
        }

        return keyframeCount;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }

    private static double? NormalizeOptionalNumber(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return null;
        }

        return value > 0.0f || value < 0.0f
            ? value
            : null;
    }

    private static string NormalizeAttachmentTypeName(string typeName)
    {
        return typeName.EndsWith("Attachment", StringComparison.Ordinal)
            ? typeName[..^"Attachment".Length].ToLowerInvariant()
            : typeName.ToLowerInvariant();
    }

    private sealed class ValidatingTextureLoader3895 : Spine38.TextureLoader
    {
        public void Load(Spine38.AtlasPage page, string path)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!File.Exists(path))
            {
                throw new LoadException(
                    LoadFailureStage.DependentTexture,
                    $"Texture '{path}' referenced by the atlas was not found.",
                    new FileNotFoundException("Texture file was not found.", path));
            }

            page.rendererObject = path;
            page.width = page.width <= 0 ? 1 : page.width;
            page.height = page.height <= 0 ? 1 : page.height;
        }

        public void Unload(object texture)
        {
        }
    }

    private sealed class ValidatingTextureLoader4100 : Spine41.TextureLoader
    {
        public void Load(Spine41.AtlasPage page, string path)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            if (!File.Exists(path))
            {
                throw new LoadException(
                    LoadFailureStage.DependentTexture,
                    $"Texture '{path}' referenced by the atlas was not found.",
                    new FileNotFoundException("Texture file was not found.", path));
            }

            page.rendererObject = path;
            page.width = page.width <= 0 ? 1 : page.width;
            page.height = page.height <= 0 ? 1 : page.height;
        }

        public void Unload(object texture)
        {
        }
    }
}
