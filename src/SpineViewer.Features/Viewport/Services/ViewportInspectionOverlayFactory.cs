using Avalonia.Media;
using SpineViewer.Core.Models;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Builds setup-pose technical overlays from the current inspected session.
/// </summary>
public sealed class ViewportInspectionOverlayFactory : IViewportInspectionOverlayFactory
{
    private static readonly Color PlaceholderColor = Color.FromArgb(0xAA, 0x8B, 0xC1, 0xD6);
    private static readonly Color BoneColor = Color.FromRgb(0xF2, 0xCC, 0x8F);
    private static readonly Color BoundsColor = Color.FromRgb(0x9A, 0xD1, 0x8B);
    private static readonly Color MeshColor = Color.FromRgb(0xF4, 0x7F, 0x6B);
    private static readonly Color SlotColor = Color.FromRgb(0x74, 0xC6, 0xE5);
    private static readonly Color LabelColor = Color.FromRgb(0xE7, 0xE0, 0xD3);
    private static readonly Color MissingColor = Color.FromRgb(0xFF, 0xC8, 0x5C);
    private static readonly Color UnsupportedColor = Color.FromRgb(0xFF, 0x8C, 0x7A);

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreatePlaceholderLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.HasActiveSession)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        Projection projection = Project(context);
        if (projection.Attachments.Count == 0)
        {
            return
            [
                new ViewportOverlayLine(-90, -130, 90, -130, PlaceholderColor, 2.0),
                new ViewportOverlayLine(90, -130, 90, 130, PlaceholderColor, 2.0),
                new ViewportOverlayLine(90, 130, -90, 130, PlaceholderColor, 2.0),
                new ViewportOverlayLine(-90, 130, -90, -130, PlaceholderColor, 2.0),
                new ViewportOverlayLine(-90, -130, 90, 130, PlaceholderColor, 1.2),
                new ViewportOverlayLine(90, -130, -90, 130, PlaceholderColor, 1.2),
            ];
        }

        List<ViewportOverlayLine> lines = [];
        foreach (AttachmentPose attachment in projection.Attachments)
        {
            lines.AddRange(CreateRectangleLines(attachment, PlaceholderColor, 1.4, true));
        }

        return lines;
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateBoneLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Projection projection = Project(context);
        if (projection.Bones.Count == 0)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        return projection.Bones
            .Select(
                bone => new ViewportOverlayLine(
                    bone.StartX,
                    bone.StartY,
                    bone.EndX,
                    bone.EndY,
                    BoneColor,
                    2.1))
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateBoundsLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Projection projection = Project(context);
        if (projection.Bounds is null)
        {
            return Array.Empty<ViewportOverlayLine>();
        }

        ViewportContentBounds bounds = projection.Bounds;
        return
        [
            new ViewportOverlayLine(bounds.MinimumX, bounds.MinimumY, bounds.MaximumX, bounds.MinimumY, BoundsColor, 2.2),
            new ViewportOverlayLine(bounds.MaximumX, bounds.MinimumY, bounds.MaximumX, bounds.MaximumY, BoundsColor, 2.2),
            new ViewportOverlayLine(bounds.MaximumX, bounds.MaximumY, bounds.MinimumX, bounds.MaximumY, BoundsColor, 2.2),
            new ViewportOverlayLine(bounds.MinimumX, bounds.MaximumY, bounds.MinimumX, bounds.MinimumY, BoundsColor, 2.2),
        ];
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateMeshLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Projection projection = Project(context);
        List<ViewportOverlayLine> lines = [];

        foreach (AttachmentPose attachment in projection.Attachments.Where(static attachment => attachment.IsMeshLike))
        {
            lines.AddRange(CreateRectangleLines(attachment, MeshColor, 1.8, true));
            lines.Add(new ViewportOverlayLine(attachment.LeftTop.X, attachment.LeftTop.Y, attachment.RightBottom.X, attachment.RightBottom.Y, MeshColor, 1.0));
            lines.Add(new ViewportOverlayLine(attachment.RightTop.X, attachment.RightTop.Y, attachment.LeftBottom.X, attachment.LeftBottom.Y, MeshColor, 1.0));
        }

        return lines;
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayLine> CreateSlotOutlineLines(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Projection projection = Project(context);
        List<ViewportOverlayLine> lines = [];

        foreach (AttachmentPose attachment in projection.Attachments)
        {
            lines.AddRange(CreateRectangleLines(attachment, SlotColor, 1.6, true));
        }

        return lines;
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateLabelText(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Projection projection = Project(context);
        List<ViewportOverlayText> text = [];

        foreach (BonePose bone in projection.Bones)
        {
            text.Add(new ViewportOverlayText(bone.Name, bone.EndX + 6.0, bone.EndY - 10.0, LabelColor, 12.0, true));
        }

        foreach (AttachmentPose attachment in projection.Attachments)
        {
            text.Add(new ViewportOverlayText($"{attachment.SlotName}: {attachment.Name}", attachment.CenterX, attachment.CenterY, LabelColor, 11.0, true));
        }

        return text;
    }

    /// <inheritdoc />
    public IReadOnlyList<ViewportOverlayText> CreateDiagnosticText(ViewportOverlayContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.CurrentSession is null)
        {
            return Array.Empty<ViewportOverlayText>();
        }

        List<ViewportOverlayText> text = [];
        double y = 16.0;

        if (context.ViewportState.ShowMissingResourceIndicators)
        {
            foreach (ViewerDiagnostic diagnostic in context.CurrentSession.Diagnostics
                         .Where(
                             diagnostic =>
                                 diagnostic.Code.Contains("missing", StringComparison.OrdinalIgnoreCase) ||
                                 diagnostic.Message.Contains("missing", StringComparison.OrdinalIgnoreCase) ||
                                 diagnostic.Message.Contains("texture", StringComparison.OrdinalIgnoreCase))
                         .Take(3))
            {
                text.Add(new ViewportOverlayText($"Missing: {diagnostic.Message}", 16.0, y, MissingColor, 13.0, false));
                y += 18.0;
            }
        }

        if (context.ViewportState.ShowUnsupportedFeatureIndicators)
        {
            foreach (UnsupportedSpineFeature feature in context.CurrentSession.UnsupportedFeatures.Take(3))
            {
                text.Add(new ViewportOverlayText($"Warning: {feature.DisplayName}", 16.0, y, UnsupportedColor, 13.0, false));
                y += 18.0;
            }
        }

        return text;
    }

    private static IReadOnlyList<ViewportOverlayLine> CreateRectangleLines(
        AttachmentPose attachment,
        Color color,
        double thickness,
        bool includeInBounds)
    {
        return
        [
            new ViewportOverlayLine(attachment.LeftTop.X, attachment.LeftTop.Y, attachment.RightTop.X, attachment.RightTop.Y, color, thickness, includeInBounds),
            new ViewportOverlayLine(attachment.RightTop.X, attachment.RightTop.Y, attachment.RightBottom.X, attachment.RightBottom.Y, color, thickness, includeInBounds),
            new ViewportOverlayLine(attachment.RightBottom.X, attachment.RightBottom.Y, attachment.LeftBottom.X, attachment.LeftBottom.Y, color, thickness, includeInBounds),
            new ViewportOverlayLine(attachment.LeftBottom.X, attachment.LeftBottom.Y, attachment.LeftTop.X, attachment.LeftTop.Y, color, thickness, includeInBounds),
        ];
    }

    private static Projection Project(ViewportOverlayContext context)
    {
        if (context.CurrentSession is null)
        {
            return Projection.Empty;
        }

        SpineProjectInspection inspection = context.CurrentSession.Inspection;
        Dictionary<string, BonePose> bonesByName = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, SpineBoneInfo> inspectionBones = inspection.Bones.ToDictionary(static bone => bone.Name, StringComparer.OrdinalIgnoreCase);

        foreach (SpineBoneInfo bone in inspection.Bones)
        {
            ResolveBonePose(bone);
        }

        IReadOnlyList<AttachmentPose> attachments = ResolveAttachmentPoses(context.CurrentSession, bonesByName);
        ViewportContentBounds? bounds = ResolveBounds(bonesByName.Values, attachments);

        return new Projection(bonesByName.Values.OrderBy(static bone => bone.Name, StringComparer.OrdinalIgnoreCase).ToArray(), attachments, bounds);

        BonePose ResolveBonePose(SpineBoneInfo bone)
        {
            if (bonesByName.TryGetValue(bone.Name, out BonePose? cachedPose))
            {
                return cachedPose;
            }

            BonePose? parentPose = null;
            if (!string.IsNullOrWhiteSpace(bone.ParentName) &&
                inspectionBones.TryGetValue(bone.ParentName, out SpineBoneInfo? parentBone))
            {
                parentPose = ResolveBonePose(parentBone);
            }

            double parentRotation = parentPose?.WorldRotationDegrees ?? 0.0;
            double parentScaleX = parentPose?.WorldScaleX ?? 1.0;
            double parentScaleY = parentPose?.WorldScaleY ?? 1.0;
            double parentOriginX = parentPose?.OriginX ?? 0.0;
            double parentOriginY = parentPose?.OriginY ?? 0.0;
            (double offsetX, double offsetY) = RotateAndScale(bone.X, bone.Y, parentRotation, parentScaleX, parentScaleY);

            double originX = parentOriginX + offsetX;
            double originY = parentOriginY + offsetY;
            double worldRotation = parentRotation + bone.Rotation;
            double worldScaleX = parentScaleX * bone.ScaleX;
            double worldScaleY = parentScaleY * bone.ScaleY;
            double radians = DegreesToRadians(worldRotation);
            double endX = originX + (Math.Cos(radians) * bone.Length * worldScaleX);
            double endY = originY + (Math.Sin(radians) * bone.Length * worldScaleY);

            BonePose pose = new(
                bone.Name,
                originX,
                originY,
                endX,
                endY,
                worldRotation,
                worldScaleX,
                worldScaleY);
            bonesByName[bone.Name] = pose;
            return pose;
        }
    }

    private static IReadOnlyList<AttachmentPose> ResolveAttachmentPoses(
        SpineProjectSession session,
        IReadOnlyDictionary<string, BonePose> bonesByName)
    {
        string? selectedSkinName = session.SelectedSkinName ?? session.Inspection.Skins.FirstOrDefault(static skin => skin.IsDefault)?.Name;
        if (string.IsNullOrWhiteSpace(selectedSkinName))
        {
            return Array.Empty<AttachmentPose>();
        }

        List<AttachmentPose> attachments = [];

        foreach (SpineSlotInfo slot in session.Inspection.Slots)
        {
            if (!bonesByName.TryGetValue(slot.BoneName, out BonePose? bonePose))
            {
                continue;
            }

            SpineAttachmentInfo? attachment = session.Inspection.Attachments.FirstOrDefault(
                currentAttachment =>
                    string.Equals(currentAttachment.SkinName, selectedSkinName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(currentAttachment.SlotName, slot.Name, StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(currentAttachment.Name, slot.AttachmentName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(slot.AttachmentName)));

            attachment ??= session.Inspection.Attachments.FirstOrDefault(
                currentAttachment =>
                    string.Equals(currentAttachment.SkinName, selectedSkinName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(currentAttachment.SlotName, slot.Name, StringComparison.OrdinalIgnoreCase));

            if (attachment is null)
            {
                continue;
            }

            attachments.Add(ProjectAttachment(slot, attachment, bonePose));
        }

        return attachments;
    }

    private static AttachmentPose ProjectAttachment(
        SpineSlotInfo slot,
        SpineAttachmentInfo attachment,
        BonePose bonePose)
    {
        (double centerOffsetX, double centerOffsetY) = RotateAndScale(
            attachment.X,
            attachment.Y,
            bonePose.WorldRotationDegrees,
            bonePose.WorldScaleX,
            bonePose.WorldScaleY);
        double centerX = bonePose.OriginX + centerOffsetX;
        double centerY = bonePose.OriginY + centerOffsetY;
        double rotation = bonePose.WorldRotationDegrees + attachment.Rotation;
        double halfWidth = Math.Abs((attachment.Width ?? 34.0) * attachment.ScaleX * bonePose.WorldScaleX / 2.0);
        double halfHeight = Math.Abs((attachment.Height ?? 34.0) * attachment.ScaleY * bonePose.WorldScaleY / 2.0);

        return new AttachmentPose(
            slot.Name,
            attachment.Name,
            attachment.AttachmentType,
            centerX,
            centerY,
            halfWidth,
            halfHeight,
            rotation,
            attachment.VertexCount > 0 || string.Equals(attachment.AttachmentType, "mesh", StringComparison.OrdinalIgnoreCase));
    }

    private static ViewportContentBounds? ResolveBounds(
        IEnumerable<BonePose> bones,
        IEnumerable<AttachmentPose> attachments)
    {
        double minX = double.PositiveInfinity;
        double minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity;
        double maxY = double.NegativeInfinity;

        foreach (BonePose bone in bones)
        {
            minX = Math.Min(minX, Math.Min(bone.StartX, bone.EndX));
            minY = Math.Min(minY, Math.Min(bone.StartY, bone.EndY));
            maxX = Math.Max(maxX, Math.Max(bone.StartX, bone.EndX));
            maxY = Math.Max(maxY, Math.Max(bone.StartY, bone.EndY));
        }

        foreach (AttachmentPose attachment in attachments)
        {
            minX = Math.Min(minX, Math.Min(Math.Min(attachment.LeftTop.X, attachment.RightTop.X), Math.Min(attachment.LeftBottom.X, attachment.RightBottom.X)));
            minY = Math.Min(minY, Math.Min(Math.Min(attachment.LeftTop.Y, attachment.RightTop.Y), Math.Min(attachment.LeftBottom.Y, attachment.RightBottom.Y)));
            maxX = Math.Max(maxX, Math.Max(Math.Max(attachment.LeftTop.X, attachment.RightTop.X), Math.Max(attachment.LeftBottom.X, attachment.RightBottom.X)));
            maxY = Math.Max(maxY, Math.Max(Math.Max(attachment.LeftTop.Y, attachment.RightTop.Y), Math.Max(attachment.LeftBottom.Y, attachment.RightBottom.Y)));
        }

        if (double.IsInfinity(minX) || double.IsInfinity(minY) || double.IsInfinity(maxX) || double.IsInfinity(maxY))
        {
            return null;
        }

        return new ViewportContentBounds(minX, minY, maxX, maxY);
    }

    private static (double X, double Y) RotateAndScale(
        double x,
        double y,
        double degrees,
        double scaleX,
        double scaleY)
    {
        double radians = DegreesToRadians(degrees);
        double scaledX = x * scaleX;
        double scaledY = y * scaleY;

        return
        (
            (scaledX * Math.Cos(radians)) - (scaledY * Math.Sin(radians)),
            (scaledX * Math.Sin(radians)) + (scaledY * Math.Cos(radians))
        );
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    private sealed record AttachmentPose(
        string SlotName,
        string Name,
        string AttachmentType,
        double CenterX,
        double CenterY,
        double HalfWidth,
        double HalfHeight,
        double RotationDegrees,
        bool IsMeshLike)
    {
        public (double X, double Y) LeftTop => RotateCorner(-HalfWidth, -HalfHeight);

        public (double X, double Y) RightTop => RotateCorner(HalfWidth, -HalfHeight);

        public (double X, double Y) RightBottom => RotateCorner(HalfWidth, HalfHeight);

        public (double X, double Y) LeftBottom => RotateCorner(-HalfWidth, HalfHeight);

        private (double X, double Y) RotateCorner(double localX, double localY)
        {
            double radians = DegreesToRadians(RotationDegrees);
            double rotatedX = (localX * Math.Cos(radians)) - (localY * Math.Sin(radians));
            double rotatedY = (localX * Math.Sin(radians)) + (localY * Math.Cos(radians));
            return (CenterX + rotatedX, CenterY + rotatedY);
        }
    }

    private sealed record BonePose(
        string Name,
        double StartX,
        double StartY,
        double EndX,
        double EndY,
        double WorldRotationDegrees,
        double WorldScaleX,
        double WorldScaleY)
    {
        public double OriginX => StartX;

        public double OriginY => StartY;
    }

    private sealed record Projection(
        IReadOnlyList<BonePose> Bones,
        IReadOnlyList<AttachmentPose> Attachments,
        ViewportContentBounds? Bounds)
    {
        public static Projection Empty { get; } = new(
            Array.Empty<BonePose>(),
            Array.Empty<AttachmentPose>(),
            null);
    }
}
