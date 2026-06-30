using SpineViewer.Core.Models;

namespace SpineViewer.Infrastructure.Spine.Loading;

internal sealed record SpineRuntimeSkeletonInspectionData(
    string? ExportVersion,
    string? ImagesPath,
    string? AudioPath,
    double? Width,
    double? Height,
    double? FramesPerSecond,
    IReadOnlyList<SpineAnimationInfo> Animations,
    IReadOnlyList<SpineSkinInfo> Skins,
    IReadOnlyList<SpineBoneInfo> Bones,
    IReadOnlyList<SpineSlotInfo> Slots,
    IReadOnlyList<SpineAttachmentInfo> Attachments);
