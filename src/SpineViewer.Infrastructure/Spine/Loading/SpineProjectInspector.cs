using System.Text.Json;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Infrastructure.Spine.Loading;

/// <summary>
/// Extracts structured inspection metadata from Spine skeleton JSON and atlas text files.
/// </summary>
public sealed class SpineProjectInspector : ISpineProjectInspector
{
    /// <inheritdoc />
    public async Task<SpineProjectInspection> InspectAsync(
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor selectedRuntime,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(assetFileSet);
        ArgumentNullException.ThrowIfNull(selectedRuntime);
        cancellationToken.ThrowIfCancellationRequested();

        List<ViewerDiagnostic> diagnostics = [];
        List<SpineAnimationInfo> animations = [];
        List<SpineSkinInfo> skins = [];
        List<SpineBoneInfo> bones = [];
        List<SpineSlotInfo> slots = [];
        List<SpineAttachmentInfo> attachments = [];
        List<SpineAtlasPageInfo> atlasPages = [];
        List<SpineAtlasRegionInfo> atlasRegions = [];
        string? exportVersion = null;
        string? imagesPath = null;
        string? audioPath = null;
        double? width = null;
        double? height = null;
        double? framesPerSecond = null;

        if (string.Equals(Path.GetExtension(assetFileSet.SkeletonPath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await using FileStream stream = File.OpenRead(assetFileSet.SkeletonPath);
                using JsonDocument document = await JsonDocument
                    .ParseAsync(stream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                JsonElement root = document.RootElement;
                ParseSkeletonMetadata(
                    root,
                    ref exportVersion,
                    ref imagesPath,
                    ref audioPath,
                    ref width,
                    ref height,
                    ref framesPerSecond);
                bones.AddRange(ParseBones(root));
                slots.AddRange(ParseSlots(root));
                ParseSkins(root, skins, attachments);
                animations.AddRange(ParseAnimations(root));
            }
            catch (JsonException exception)
            {
                diagnostics.Add(
                    new ViewerDiagnostic(
                        "inspection-json-parse-failed",
                        ViewerDiagnosticSeverity.Warning,
                        "Structured inspector details could not be extracted from the skeleton JSON.",
                        nameof(SpineProjectInspector),
                        exception.Message,
                        "Re-export the skeleton JSON if you need deep inspection data."));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                diagnostics.Add(
                    new ViewerDiagnostic(
                        "inspection-json-read-failed",
                        ViewerDiagnosticSeverity.Warning,
                        "Structured inspector details could not be read from the skeleton JSON.",
                        nameof(SpineProjectInspector),
                        exception.Message,
                        "Verify that the skeleton JSON still exists and is accessible."));
            }
        }
        else
        {
            try
            {
                SpineRuntimeSkeletonInspectionData binaryInspection = LegacyRuntimeAssetLoader
                    .InspectBinarySkeleton(selectedRuntime.RuntimeId, assetFileSet);

                exportVersion = binaryInspection.ExportVersion;
                imagesPath = binaryInspection.ImagesPath;
                audioPath = binaryInspection.AudioPath;
                width = binaryInspection.Width;
                height = binaryInspection.Height;
                framesPerSecond = binaryInspection.FramesPerSecond;
                animations.AddRange(binaryInspection.Animations);
                skins.AddRange(binaryInspection.Skins);
                bones.AddRange(binaryInspection.Bones);
                slots.AddRange(binaryInspection.Slots);
                attachments.AddRange(binaryInspection.Attachments);
            }
            catch (LegacyRuntimeAssetLoader.LoadException exception)
            {
                diagnostics.Add(CreateBinaryInspectionDiagnostic(assetFileSet, selectedRuntime, exception));
            }
            catch (ArgumentOutOfRangeException exception)
            {
                diagnostics.Add(
                    new ViewerDiagnostic(
                        "inspection-binary-runtime-unsupported",
                        ViewerDiagnosticSeverity.Warning,
                        "Structured inspector details are not available for the selected runtime.",
                        nameof(SpineProjectInspector),
                        exception.Message,
                        "Choose a supported runtime and open the project again."));
            }
        }

        try
        {
            string atlasText = await File.ReadAllTextAsync(assetFileSet.AtlasPath, cancellationToken).ConfigureAwait(false);
            ParseAtlas(atlasText, atlasPages, atlasRegions);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(
                new ViewerDiagnostic(
                    "inspection-atlas-read-failed",
                    ViewerDiagnosticSeverity.Warning,
                    "Atlas inspection details could not be read.",
                    nameof(SpineProjectInspector),
                    exception.Message,
                    "Verify that the atlas file still exists and is accessible."));
        }

        SpineExportMetadata exportMetadata = new(
            exportVersion,
            imagesPath,
            audioPath,
            width,
            height,
            framesPerSecond,
            bones.Count,
            slots.Count,
            skins.Count,
            animations.Count,
            atlasPages.Count,
            atlasRegions.Count);

        return new SpineProjectInspection(
            exportMetadata,
            animations.OrderBy(static animation => animation.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            skins.OrderBy(static skin => skin.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            bones.OrderBy(static bone => bone.Depth).ThenBy(static bone => bone.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            slots.OrderBy(static slot => slot.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            attachments
                .OrderBy(static attachment => attachment.SkinName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static attachment => attachment.SlotName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static attachment => attachment.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            atlasPages.OrderBy(static page => page.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
            atlasRegions
                .OrderBy(static region => region.PageName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static region => region.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            diagnostics);
    }

    private static ViewerDiagnostic CreateBinaryInspectionDiagnostic(
        SpineAssetFileSet assetFileSet,
        SpineRuntimeDescriptor selectedRuntime,
        LegacyRuntimeAssetLoader.LoadException exception)
    {
        string diagnosticCode = exception.Stage switch
        {
            LegacyRuntimeAssetLoader.LoadFailureStage.DependentTexture => "inspection-binary-texture-missing",
            LegacyRuntimeAssetLoader.LoadFailureStage.Atlas => "inspection-binary-atlas-load-failed",
            _ => "inspection-binary-skeleton-load-failed",
        };

        string message = exception.Stage switch
        {
            LegacyRuntimeAssetLoader.LoadFailureStage.DependentTexture => "Structured inspector details could not be extracted because a referenced texture is missing.",
            LegacyRuntimeAssetLoader.LoadFailureStage.Atlas => "Structured inspector details could not be extracted because the atlas could not be loaded.",
            _ => "Structured inspector details could not be extracted from the binary skeleton.",
        };

        string suggestedAction = exception.Stage switch
        {
            LegacyRuntimeAssetLoader.LoadFailureStage.DependentTexture => "Restore the missing atlas page textures and reopen the project.",
            LegacyRuntimeAssetLoader.LoadFailureStage.Atlas => "Verify that the atlas matches the skeleton export and reopen the project.",
            _ => $"Verify that '{assetFileSet.SkeletonPath}' matches the {selectedRuntime.DisplayName} runtime family.",
        };

        return new ViewerDiagnostic(
            diagnosticCode,
            ViewerDiagnosticSeverity.Warning,
            message,
            nameof(SpineProjectInspector),
            (exception.InnerException ?? exception).Message,
            suggestedAction);
    }

    private static IReadOnlyList<SpineAnimationInfo> ParseAnimations(JsonElement root)
    {
        if (!root.TryGetProperty("animations", out JsonElement animationsElement) ||
            animationsElement.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<SpineAnimationInfo>();
        }

        List<SpineAnimationInfo> animations = [];

        foreach (JsonProperty animationProperty in animationsElement.EnumerateObject())
        {
            (TimeSpan duration, int timelineCount, int keyframeCount) = InspectAnimation(animationProperty.Value);
            animations.Add(
                new SpineAnimationInfo(
                    animationProperty.Name,
                    duration,
                    timelineCount,
                    keyframeCount));
        }

        return animations;
    }

    private static IReadOnlyList<SpineBoneInfo> ParseBones(JsonElement root)
    {
        if (!root.TryGetProperty("bones", out JsonElement bonesElement) ||
            bonesElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<SpineBoneInfo>();
        }

        List<(string Name, string? ParentName, double Length, double X, double Y, double Rotation, double ScaleX, double ScaleY)> rawBones = [];

        foreach (JsonElement boneElement in bonesElement.EnumerateArray())
        {
            string? name = TryGetString(boneElement, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            rawBones.Add(
                (
                    name,
                    TryGetString(boneElement, "parent"),
                    TryGetDouble(boneElement, "length") ?? 0.0,
                    TryGetDouble(boneElement, "x") ?? 0.0,
                    TryGetDouble(boneElement, "y") ?? 0.0,
                    TryGetDouble(boneElement, "rotation") ?? 0.0,
                    TryGetDouble(boneElement, "scaleX") ?? 1.0,
                    TryGetDouble(boneElement, "scaleY") ?? 1.0));
        }

        Dictionary<string, int> depthCache = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string?> parentByBone = rawBones.ToDictionary(
            static bone => bone.Name,
            static bone => bone.ParentName,
            StringComparer.OrdinalIgnoreCase);

        return rawBones
            .Select(
                bone => new SpineBoneInfo(
                    bone.Name,
                    bone.ParentName,
                    bone.Length,
                    bone.X,
                    bone.Y,
                    bone.Rotation,
                    bone.ScaleX,
                    bone.ScaleY,
                    ResolveDepth(bone.Name, parentByBone, depthCache)))
            .ToArray();
    }

    private static IReadOnlyList<SpineSlotInfo> ParseSlots(JsonElement root)
    {
        if (!root.TryGetProperty("slots", out JsonElement slotsElement) ||
            slotsElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<SpineSlotInfo>();
        }

        List<SpineSlotInfo> slots = [];

        foreach (JsonElement slotElement in slotsElement.EnumerateArray())
        {
            string? name = TryGetString(slotElement, "name");
            string? boneName = TryGetString(slotElement, "bone");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(boneName))
            {
                continue;
            }

            slots.Add(
                new SpineSlotInfo(
                    name,
                    boneName,
                    TryGetString(slotElement, "attachment"),
                    TryGetString(slotElement, "blend")));
        }

        return slots;
    }

    private static void ParseSkins(
        JsonElement root,
        ICollection<SpineSkinInfo> skins,
        ICollection<SpineAttachmentInfo> attachments)
    {
        if (!root.TryGetProperty("skins", out JsonElement skinsElement))
        {
            return;
        }

        if (skinsElement.ValueKind == JsonValueKind.Array)
        {
            ParseSkinArray(skinsElement, skins, attachments);
            return;
        }

        if (skinsElement.ValueKind == JsonValueKind.Object)
        {
            ParseLegacySkinObject(skinsElement, skins, attachments);
        }
    }

    private static void ParseAtlas(
        string atlasText,
        ICollection<SpineAtlasPageInfo> atlasPages,
        ICollection<SpineAtlasRegionInfo> atlasRegions)
    {
        string[] lines = atlasText
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n');

        int index = 0;

        while (index < lines.Length)
        {
            SkipBlankLines(lines, ref index);
            if (index >= lines.Length)
            {
                break;
            }

            string pageName = lines[index].Trim();
            index++;

            Dictionary<string, string> pageMetadata = ReadMetadata(lines, ref index);
            List<SpineAtlasRegionInfo> pageRegions = [];

            while (index < lines.Length)
            {
                if (string.IsNullOrWhiteSpace(lines[index]))
                {
                    index++;
                    break;
                }

                string regionName = lines[index].Trim();
                index++;
                Dictionary<string, string> regionMetadata = ReadMetadata(lines, ref index);

                pageRegions.Add(
                    new SpineAtlasRegionInfo(
                        pageName,
                        regionName,
                        ParseRotate(regionMetadata),
                        ParsePair(regionMetadata, "size").First,
                        ParsePair(regionMetadata, "size").Second,
                        ParsePair(regionMetadata, "orig").First,
                        ParsePair(regionMetadata, "orig").Second));
            }

            (int? pageWidth, int? pageHeight) = ParsePair(pageMetadata, "size");
            pageMetadata.TryGetValue("format", out string? format);
            pageMetadata.TryGetValue("filter", out string? filter);
            pageMetadata.TryGetValue("repeat", out string? repeat);

            atlasPages.Add(
                new SpineAtlasPageInfo(
                    pageName,
                    pageWidth,
                    pageHeight,
                    format,
                    filter,
                    repeat,
                    pageRegions.Count));

            foreach (SpineAtlasRegionInfo region in pageRegions)
            {
                atlasRegions.Add(region);
            }
        }
    }

    private static (TimeSpan Duration, int TimelineCount, int KeyframeCount) InspectAnimation(JsonElement animationElement)
    {
        double maxTime = 0.0;
        int timelineCount = 0;
        int keyframeCount = 0;

        Visit(animationElement);

        return (TimeSpan.FromSeconds(maxTime), timelineCount, keyframeCount);

        void Visit(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        Visit(property.Value);
                    }

                    break;

                case JsonValueKind.Array:
                    JsonElement[] arrayElements = element.EnumerateArray().ToArray();
                    if (arrayElements.Length == 0)
                    {
                        return;
                    }

                    if (arrayElements.All(static item => item.ValueKind == JsonValueKind.Object) &&
                        arrayElements.Any(HasTimeProperty))
                    {
                        timelineCount++;
                        keyframeCount += arrayElements.Length;

                        foreach (JsonElement frameElement in arrayElements)
                        {
                            double? frameTime = TryGetDouble(frameElement, "time");
                            if (frameTime.HasValue)
                            {
                                maxTime = Math.Max(maxTime, frameTime.Value);
                            }
                        }

                        return;
                    }

                    foreach (JsonElement child in arrayElements)
                    {
                        Visit(child);
                    }

                    break;
            }
        }
    }

    private static bool HasTimeProperty(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty("time", out JsonElement timeElement) &&
               timeElement.ValueKind is JsonValueKind.Number or JsonValueKind.String;
    }

    private static void ParseSkeletonMetadata(
        JsonElement root,
        ref string? exportVersion,
        ref string? imagesPath,
        ref string? audioPath,
        ref double? width,
        ref double? height,
        ref double? framesPerSecond)
    {
        if (!root.TryGetProperty("skeleton", out JsonElement skeletonElement) ||
            skeletonElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        exportVersion = TryGetString(skeletonElement, "spine");
        imagesPath = TryGetString(skeletonElement, "images");
        audioPath = TryGetString(skeletonElement, "audio");
        width = TryGetDouble(skeletonElement, "width");
        height = TryGetDouble(skeletonElement, "height");
        framesPerSecond = TryGetDouble(skeletonElement, "fps");
    }

    private static void ParseSkinArray(
        JsonElement skinsElement,
        ICollection<SpineSkinInfo> skins,
        ICollection<SpineAttachmentInfo> attachments)
    {
        List<(string Name, int SlotCount, int AttachmentCount)> parsedSkins = [];

        foreach (JsonElement skinElement in skinsElement.EnumerateArray())
        {
            string skinName = TryGetString(skinElement, "name") ?? "default";
            if (!skinElement.TryGetProperty("attachments", out JsonElement attachmentRoot) ||
                attachmentRoot.ValueKind != JsonValueKind.Object)
            {
                parsedSkins.Add((skinName, 0, 0));
                continue;
            }

            HashSet<string> slotNames = new(StringComparer.OrdinalIgnoreCase);
            int attachmentCount = 0;

            foreach (JsonProperty slotProperty in attachmentRoot.EnumerateObject())
            {
                slotNames.Add(slotProperty.Name);

                if (slotProperty.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (JsonProperty attachmentProperty in slotProperty.Value.EnumerateObject())
                {
                    attachments.Add(
                        CreateAttachmentInfo(
                            skinName,
                            slotProperty.Name,
                            attachmentProperty.Name,
                            attachmentProperty.Value));
                    attachmentCount++;
                }
            }

            parsedSkins.Add((skinName, slotNames.Count, attachmentCount));
        }

        string? defaultSkinName = parsedSkins
            .FirstOrDefault(static skin => string.Equals(skin.Name, "default", StringComparison.OrdinalIgnoreCase))
            .Name;

        if (defaultSkinName is null && parsedSkins.Count > 0)
        {
            defaultSkinName = parsedSkins[0].Name;
        }

        foreach ((string name, int slotCount, int attachmentCount) in parsedSkins)
        {
            skins.Add(new SpineSkinInfo(name, slotCount, attachmentCount, string.Equals(name, defaultSkinName, StringComparison.OrdinalIgnoreCase)));
        }
    }

    private static void ParseLegacySkinObject(
        JsonElement skinsElement,
        ICollection<SpineSkinInfo> skins,
        ICollection<SpineAttachmentInfo> attachments)
    {
        List<(string Name, int SlotCount, int AttachmentCount)> parsedSkins = [];

        foreach (JsonProperty skinProperty in skinsElement.EnumerateObject())
        {
            if (skinProperty.Value.ValueKind != JsonValueKind.Object)
            {
                parsedSkins.Add((skinProperty.Name, 0, 0));
                continue;
            }

            HashSet<string> slotNames = new(StringComparer.OrdinalIgnoreCase);
            int attachmentCount = 0;

            foreach (JsonProperty slotProperty in skinProperty.Value.EnumerateObject())
            {
                slotNames.Add(slotProperty.Name);

                if (slotProperty.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (JsonProperty attachmentProperty in slotProperty.Value.EnumerateObject())
                {
                    attachments.Add(
                        CreateAttachmentInfo(
                            skinProperty.Name,
                            slotProperty.Name,
                            attachmentProperty.Name,
                            attachmentProperty.Value));
                    attachmentCount++;
                }
            }

            parsedSkins.Add((skinProperty.Name, slotNames.Count, attachmentCount));
        }

        string? defaultSkinName = parsedSkins
            .FirstOrDefault(static skin => string.Equals(skin.Name, "default", StringComparison.OrdinalIgnoreCase))
            .Name;

        if (defaultSkinName is null && parsedSkins.Count > 0)
        {
            defaultSkinName = parsedSkins[0].Name;
        }

        foreach ((string name, int slotCount, int attachmentCount) in parsedSkins)
        {
            skins.Add(new SpineSkinInfo(name, slotCount, attachmentCount, string.Equals(name, defaultSkinName, StringComparison.OrdinalIgnoreCase)));
        }
    }

    private static SpineAttachmentInfo CreateAttachmentInfo(
        string skinName,
        string slotName,
        string attachmentName,
        JsonElement attachmentElement)
    {
        string attachmentType = TryGetString(attachmentElement, "type") ?? "region";
        double? width = TryGetDouble(attachmentElement, "width");
        double? height = TryGetDouble(attachmentElement, "height");
        int vertexCount = 0;
        int triangleCount = 0;

        if (attachmentElement.TryGetProperty("vertices", out JsonElement verticesElement) &&
            verticesElement.ValueKind == JsonValueKind.Array)
        {
            vertexCount = verticesElement.GetArrayLength();
        }

        if (attachmentElement.TryGetProperty("triangles", out JsonElement trianglesElement) &&
            trianglesElement.ValueKind == JsonValueKind.Array)
        {
            triangleCount = trianglesElement.GetArrayLength() / 3;
        }

        return new SpineAttachmentInfo(
            skinName,
            slotName,
            attachmentName,
            attachmentType,
            TryGetString(attachmentElement, "path"),
            TryGetDouble(attachmentElement, "x") ?? 0.0,
            TryGetDouble(attachmentElement, "y") ?? 0.0,
            TryGetDouble(attachmentElement, "rotation") ?? 0.0,
            TryGetDouble(attachmentElement, "scaleX") ?? 1.0,
            TryGetDouble(attachmentElement, "scaleY") ?? 1.0,
            width,
            height,
            vertexCount,
            triangleCount);
    }

    private static Dictionary<string, string> ReadMetadata(string[] lines, ref int index)
    {
        Dictionary<string, string> metadata = new(StringComparer.OrdinalIgnoreCase);

        while (index < lines.Length)
        {
            string trimmedLine = lines[index].Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || !trimmedLine.Contains(':', StringComparison.Ordinal))
            {
                break;
            }

            int separatorIndex = trimmedLine.IndexOf(':', StringComparison.Ordinal);
            string key = trimmedLine[..separatorIndex].Trim();
            string value = trimmedLine[(separatorIndex + 1)..].Trim();
            metadata[key] = value;
            index++;
        }

        return metadata;
    }

    private static (int? First, int? Second) ParsePair(
        IReadOnlyDictionary<string, string> metadata,
        string key)
    {
        if (!metadata.TryGetValue(key, out string? value))
        {
            return (null, null);
        }

        string[] parts = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out int first) ||
            !int.TryParse(parts[1], out int second))
        {
            return (null, null);
        }

        return (first, second);
    }

    private static bool ParseRotate(IReadOnlyDictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue("rotate", out string? value))
        {
            return false;
        }

        if (bool.TryParse(value, out bool rotate))
        {
            return rotate;
        }

        return !string.Equals(value, "0", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

    private static int ResolveDepth(
        string boneName,
        IReadOnlyDictionary<string, string?> parentByBone,
        IDictionary<string, int> depthCache)
    {
        if (depthCache.TryGetValue(boneName, out int depth))
        {
            return depth;
        }

        if (!parentByBone.TryGetValue(boneName, out string? parentName) ||
            string.IsNullOrWhiteSpace(parentName) ||
            !parentByBone.ContainsKey(parentName))
        {
            depthCache[boneName] = 0;
            return 0;
        }

        depth = ResolveDepth(parentName, parentByBone, depthCache) + 1;
        depthCache[boneName] = depth;
        return depth;
    }

    private static void SkipBlankLines(string[] lines, ref int index)
    {
        while (index < lines.Length && string.IsNullOrWhiteSpace(lines[index]))
        {
            index++;
        }
    }

    private static double? TryGetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement propertyElement))
        {
            return null;
        }

        return propertyElement.ValueKind switch
        {
            JsonValueKind.Number when propertyElement.TryGetDouble(out double numberValue) => numberValue,
            JsonValueKind.String when double.TryParse(propertyElement.GetString(), out double stringValue) => stringValue,
            _ => null,
        };
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement propertyElement) ||
            propertyElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return propertyElement.GetString();
    }
}
