using SkiaSharp;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Models;
using Spine_3895;
using Spine_3895.Skia;

namespace SpineViewer.Features.Viewport.Rendering;

/// <summary>
/// Drives real-time Spine 3.8.95 preview rendering for the viewport.
/// </summary>
internal sealed class Spine38RuntimePreviewDriver : ISpineRuntimePreviewDriver
{
    private readonly Atlas _atlas;
    private readonly AnimationStateData _animationStateData;
    private readonly SkeletonData _skeletonData;
    private readonly SkeletonRenderer _skeletonRenderer;
    private AnimationState _animationState;
    private Skeleton _skeleton;
    private string _appliedTrackSignature = string.Empty;
    private string? _appliedSkinName;
    private TimeSpan _appliedTime = TimeSpan.Zero;
    private float[] _boundsVertexBuffer = [];
    private bool _isSynchronized;

    /// <summary>
    /// Initializes a new instance of the <see cref="Spine38RuntimePreviewDriver"/> class.
    /// </summary>
    /// <param name="atlasPath">The atlas file path.</param>
    /// <param name="skeletonPath">The skeleton file path.</param>
    public Spine38RuntimePreviewDriver(
        string atlasPath,
        string skeletonPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atlasPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(skeletonPath);

        Bone.yDown = true;

        _atlas = new Atlas(atlasPath, new SpineTextureLoader());
        AtlasAttachmentLoader attachmentLoader = new(_atlas);
        _skeletonData = LoadSkeletonData(attachmentLoader, skeletonPath);
        _animationStateData = new AnimationStateData(_skeletonData)
        {
            DefaultMix = 0.3f,
        };
        _animationState = new AnimationState(_animationStateData);
        _skeleton = new Skeleton(_skeletonData);
        _skeletonRenderer = new SkeletonRenderer();
        _skeleton.UpdateWorldTransform();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _atlas.Dispose();
    }

    /// <inheritdoc />
    public void Draw(SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        _skeletonRenderer.Draw(_skeleton, canvas);
    }

    /// <inheritdoc />
    public ViewportContentBounds? GetContentBounds()
    {
        _skeleton.GetBounds(
            out float x,
            out float y,
            out float width,
            out float height,
            ref _boundsVertexBuffer);

        if (float.IsNaN(x) ||
            float.IsNaN(y) ||
            float.IsNaN(width) ||
            float.IsNaN(height) ||
            float.IsInfinity(x) ||
            float.IsInfinity(y) ||
            float.IsInfinity(width) ||
            float.IsInfinity(height) ||
            width < 0.0f ||
            height < 0.0f)
        {
            return null;
        }

        return new ViewportContentBounds(x, y, x + width, y + height);
    }

    /// <inheritdoc />
    public SpineRuntimePreviewOverlaySnapshot GetOverlaySnapshot()
    {
        List<ViewportOverlayLine> boneLines = CreateBoneLines();
        ViewportContentBounds? contentBounds = GetContentBounds();
        List<ViewportOverlayLine> meshLines = [];
        List<ViewportOverlayLine> slotOutlineLines = [];
        List<ViewportOverlayText> labelText = CreateBoneLabelText();

        foreach (Slot slot in _skeleton.DrawOrder)
        {
            Attachment? attachment = slot.Attachment;
            if (attachment is null)
            {
                continue;
            }

            switch (attachment)
            {
                case RegionAttachment regionAttachment:
                    float[] quadVertices = new float[8];
                    regionAttachment.ComputeWorldVertices(slot.Bone, quadVertices, 0);
                    slotOutlineLines.AddRange(CreateQuadLines(quadVertices, RuntimePreviewOverlayPalette.SlotColor, 1.6));
                    labelText.Add(CreateAttachmentLabelText(slot.Data.Name, attachment.Name, quadVertices));
                    break;

                case MeshAttachment meshAttachment:
                    float[] meshVertices = new float[meshAttachment.WorldVerticesLength];
                    meshAttachment.ComputeWorldVertices(slot, meshVertices);
                    IReadOnlyList<ViewportOverlayLine> meshWireframe = CreateTriangleEdgeLines(
                        meshVertices,
                        meshAttachment.Triangles,
                        RuntimePreviewOverlayPalette.MeshColor,
                        1.4);
                    meshLines.AddRange(meshWireframe);
                    slotOutlineLines.AddRange(
                        CreateTriangleEdgeLines(
                            meshVertices,
                            meshAttachment.Triangles,
                            RuntimePreviewOverlayPalette.SlotColor,
                            1.6));
                    labelText.Add(CreateAttachmentLabelText(slot.Data.Name, attachment.Name, meshVertices));
                    break;

                case VertexAttachment vertexAttachment:
                    float[] polygonVertices = new float[vertexAttachment.WorldVerticesLength];
                    vertexAttachment.ComputeWorldVertices(slot, polygonVertices);
                    slotOutlineLines.AddRange(CreateLoopLines(polygonVertices, RuntimePreviewOverlayPalette.SlotColor, 1.6));
                    labelText.Add(CreateAttachmentLabelText(slot.Data.Name, attachment.Name, polygonVertices));
                    break;

                default:
                    labelText.Add(
                        new ViewportOverlayText(
                            $"{slot.Data.Name}: {attachment.Name}",
                            slot.Bone.WorldX,
                            slot.Bone.WorldY,
                            RuntimePreviewOverlayPalette.LabelColor,
                            11.0,
                            true));
                    break;
            }
        }

        return new SpineRuntimePreviewOverlaySnapshot(
            boneLines,
            contentBounds is null
                ? Array.Empty<ViewportOverlayLine>()
                : RuntimePreviewOverlayPalette.CreateBoundsLines(contentBounds),
            meshLines,
            slotOutlineLines,
            labelText,
            contentBounds);
    }

    /// <inheritdoc />
    public void Synchronize(
        PlaybackState playbackState,
        string? selectedSkinName)
    {
        ArgumentNullException.ThrowIfNull(playbackState);

        string normalizedSkinName = string.IsNullOrWhiteSpace(selectedSkinName)
            ? string.Empty
            : selectedSkinName;
        string trackSignature = BuildTrackSignature(playbackState.Tracks);
        bool requiresReset =
            !_isSynchronized ||
            !string.Equals(_appliedSkinName, normalizedSkinName, StringComparison.Ordinal) ||
            !string.Equals(_appliedTrackSignature, trackSignature, StringComparison.Ordinal) ||
            playbackState.CurrentTime < _appliedTime;

        if (requiresReset)
        {
            ResetRuntimeState(normalizedSkinName, playbackState.Tracks);
            AdvanceTo(playbackState.CurrentTime);
        }
        else if (playbackState.CurrentTime > _appliedTime)
        {
            AdvanceBy(playbackState.CurrentTime - _appliedTime);
        }

        _appliedSkinName = normalizedSkinName;
        _appliedTrackSignature = trackSignature;
        _appliedTime = playbackState.CurrentTime;
        _isSynchronized = true;
    }

    private static string BuildTrackSignature(IReadOnlyList<AnimationTrackState> tracks)
    {
        return string.Join(
            '|',
            tracks
                .OrderBy(static track => track.TrackIndex)
                .Select(
                    track => FormattableString.Invariant(
                        $"{track.TrackId:N}:{track.TrackIndex}:{track.AnimationName}:{track.IsLooping}:{track.TimeScale:0.####}:{track.MixDuration.TotalMilliseconds:0.####}:{track.IsEnabled}")));
    }

    private static SkeletonData LoadSkeletonData(
        AtlasAttachmentLoader attachmentLoader,
        string skeletonPath)
    {
        if (string.Equals(Path.GetExtension(skeletonPath), ".json", StringComparison.OrdinalIgnoreCase))
        {
            SkeletonJson skeletonJson = new(attachmentLoader)
            {
                Scale = 1.0f,
            };
            return skeletonJson.ReadSkeletonData(skeletonPath);
        }

        SkeletonBinary skeletonBinary = new(attachmentLoader)
        {
            Scale = 1.0f,
        };
        return skeletonBinary.ReadSkeletonData(skeletonPath);
    }

    private void AdvanceBy(TimeSpan delta)
    {
        _animationState.Update((float)delta.TotalSeconds);
        _animationState.Apply(_skeleton);
        _skeleton.UpdateWorldTransform();
    }

    private void AdvanceTo(TimeSpan requestedTime)
    {
        if (requestedTime > TimeSpan.Zero)
        {
            AdvanceBy(requestedTime);
            return;
        }

        _animationState.Update(0.0f);
        _animationState.Apply(_skeleton);
        _skeleton.UpdateWorldTransform();
    }

    private void ApplySelectedSkin(string normalizedSkinName)
    {
        if (string.IsNullOrEmpty(normalizedSkinName))
        {
            _skeleton.SetToSetupPose();
            return;
        }

        Skin? skin = _skeletonData.FindSkin(normalizedSkinName);
        if (skin is null)
        {
            _skeleton.SetToSetupPose();
            return;
        }

        _skeleton.SetSkin(skin);
        _skeleton.SetSlotsToSetupPose();
        _skeleton.SetBonesToSetupPose();
    }

    private void ResetRuntimeState(
        string normalizedSkinName,
        IReadOnlyList<AnimationTrackState> tracks)
    {
        _animationState = new AnimationState(_animationStateData);
        _skeleton = new Skeleton(_skeletonData);
        ApplySelectedSkin(normalizedSkinName);

        foreach (AnimationTrackState track in tracks
                     .Where(static track => track.IsEnabled)
                     .OrderBy(static track => track.TrackIndex))
        {
            if (_skeletonData.FindAnimation(track.AnimationName) is null)
            {
                continue;
            }

            TrackEntry trackEntry = _animationState.SetAnimation(
                track.TrackIndex,
                track.AnimationName,
                track.IsLooping);
            trackEntry.TimeScale = (float)track.TimeScale;
            trackEntry.MixDuration = (float)track.MixDuration.TotalSeconds;
        }

        _skeleton.UpdateWorldTransform();
    }

    private static ViewportOverlayText CreateAttachmentLabelText(
        string slotName,
        string attachmentName,
        float[] vertices)
    {
        (double centerX, double centerY) = ComputeCenter(vertices);
        return new ViewportOverlayText(
            $"{slotName}: {attachmentName}",
            centerX,
            centerY,
            RuntimePreviewOverlayPalette.LabelColor,
            11.0,
            true);
    }

    private List<ViewportOverlayText> CreateBoneLabelText()
    {
        List<ViewportOverlayText> labelText = [];

        foreach (Bone bone in _skeleton.Bones)
        {
            if (!bone.Active)
            {
                continue;
            }

            (double endX, double endY) = GetBoneEndPoint(bone);
            labelText.Add(
                new ViewportOverlayText(
                    bone.Data.Name,
                    endX + 6.0,
                    endY - 10.0,
                    RuntimePreviewOverlayPalette.LabelColor,
                    12.0,
                    true));
        }

        return labelText;
    }

    private List<ViewportOverlayLine> CreateBoneLines()
    {
        List<ViewportOverlayLine> lines = [];

        foreach (Bone bone in _skeleton.Bones)
        {
            if (!bone.Active)
            {
                continue;
            }

            (double endX, double endY) = GetBoneEndPoint(bone);
            lines.Add(
                new ViewportOverlayLine(
                    bone.WorldX,
                    bone.WorldY,
                    endX,
                    endY,
                    RuntimePreviewOverlayPalette.BoneColor,
                    2.1));
        }

        return lines;
    }

    private static IReadOnlyList<ViewportOverlayLine> CreateLoopLines(
        float[] vertices,
        Avalonia.Media.Color color,
        double thickness)
    {
        int vertexCount = vertices.Length / 2;
        if (vertexCount < 2)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        List<ViewportOverlayLine> lines = [];
        for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        {
            int nextVertexIndex = (vertexIndex + 1) % vertexCount;
            lines.Add(
                CreateIndexedLine(
                    vertices,
                    vertexIndex,
                    nextVertexIndex,
                    color,
                    thickness));
        }

        return lines;
    }

    private static IReadOnlyList<ViewportOverlayLine> CreateQuadLines(
        float[] vertices,
        Avalonia.Media.Color color,
        double thickness)
    {
        if (vertices.Length < 8)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return
        [
            CreateIndexedLine(vertices, 0, 1, color, thickness),
            CreateIndexedLine(vertices, 1, 2, color, thickness),
            CreateIndexedLine(vertices, 2, 3, color, thickness),
            CreateIndexedLine(vertices, 3, 0, color, thickness),
        ];
    }

    private static IReadOnlyList<ViewportOverlayLine> CreateTriangleEdgeLines(
        float[] vertices,
        int[]? triangles,
        Avalonia.Media.Color color,
        double thickness)
    {
        if (triangles is null || triangles.Length < 3)
        {
            return CreateLoopLines(vertices, color, thickness);
        }

        HashSet<long> seenEdges = [];
        List<ViewportOverlayLine> lines = [];

        for (int triangleIndex = 0; triangleIndex <= triangles.Length - 3; triangleIndex += 3)
        {
            AddEdge(triangles[triangleIndex], triangles[triangleIndex + 1]);
            AddEdge(triangles[triangleIndex + 1], triangles[triangleIndex + 2]);
            AddEdge(triangles[triangleIndex + 2], triangles[triangleIndex]);
        }

        return lines;

        void AddEdge(int firstVertexIndex, int secondVertexIndex)
        {
            if (firstVertexIndex < 0 ||
                secondVertexIndex < 0 ||
                (firstVertexIndex * 2) + 1 >= vertices.Length ||
                (secondVertexIndex * 2) + 1 >= vertices.Length)
            {
                return;
            }

            int minimumVertexIndex = Math.Min(firstVertexIndex, secondVertexIndex);
            int maximumVertexIndex = Math.Max(firstVertexIndex, secondVertexIndex);
            long edgeKey = ((long)minimumVertexIndex << 32) | (uint)maximumVertexIndex;
            if (!seenEdges.Add(edgeKey))
            {
                return;
            }

            lines.Add(CreateIndexedLine(vertices, firstVertexIndex, secondVertexIndex, color, thickness));
        }
    }

    private static ViewportOverlayLine CreateIndexedLine(
        float[] vertices,
        int startVertexIndex,
        int endVertexIndex,
        Avalonia.Media.Color color,
        double thickness)
    {
        int startOffset = startVertexIndex * 2;
        int endOffset = endVertexIndex * 2;

        return new ViewportOverlayLine(
            vertices[startOffset],
            vertices[startOffset + 1],
            vertices[endOffset],
            vertices[endOffset + 1],
            color,
            thickness);
    }

    private static (double CenterX, double CenterY) ComputeCenter(float[] vertices)
    {
        if (vertices.Length < 2)
        {
            return (0.0, 0.0);
        }

        double sumX = 0.0;
        double sumY = 0.0;
        int vertexCount = vertices.Length / 2;

        for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex += 2)
        {
            sumX += vertices[vertexIndex];
            sumY += vertices[vertexIndex + 1];
        }

        return (sumX / vertexCount, sumY / vertexCount);
    }

    private static (double EndX, double EndY) GetBoneEndPoint(Bone bone)
    {
        double boneLength = bone.Data.Length;
        return (bone.WorldX + (bone.A * boneLength), bone.WorldY + (bone.C * boneLength));
    }
}
