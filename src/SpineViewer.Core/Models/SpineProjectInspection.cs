using SpineViewer.Core.Utilities;

namespace SpineViewer.Core.Models;

/// <summary>
/// Represents the structured inspection snapshot extracted from one Spine project.
/// </summary>
public sealed record SpineProjectInspection
{
    /// <summary>
    /// Gets the empty inspection snapshot.
    /// </summary>
    public static SpineProjectInspection Empty { get; } = new(
        new SpineExportMetadata(null, null, null, null, null, null, 0, 0, 0, 0, 0, 0),
        Array.Empty<SpineAnimationInfo>(),
        Array.Empty<SpineSkinInfo>(),
        Array.Empty<SpineBoneInfo>(),
        Array.Empty<SpineSlotInfo>(),
        Array.Empty<SpineAttachmentInfo>(),
        Array.Empty<SpineAtlasPageInfo>(),
        Array.Empty<SpineAtlasRegionInfo>(),
        Array.Empty<ViewerDiagnostic>());

    /// <summary>
    /// Initializes a new instance of the <see cref="SpineProjectInspection"/> class.
    /// </summary>
    /// <param name="exportMetadata">The export metadata summary.</param>
    /// <param name="animations">The inspected animation entries.</param>
    /// <param name="skins">The inspected skin entries.</param>
    /// <param name="bones">The inspected bone entries.</param>
    /// <param name="slots">The inspected slot entries.</param>
    /// <param name="attachments">The inspected attachment entries.</param>
    /// <param name="atlasPages">The inspected atlas page entries.</param>
    /// <param name="atlasRegions">The inspected atlas region entries.</param>
    /// <param name="diagnostics">The diagnostics produced during inspection.</param>
    public SpineProjectInspection(
        SpineExportMetadata exportMetadata,
        IEnumerable<SpineAnimationInfo> animations,
        IEnumerable<SpineSkinInfo> skins,
        IEnumerable<SpineBoneInfo> bones,
        IEnumerable<SpineSlotInfo> slots,
        IEnumerable<SpineAttachmentInfo> attachments,
        IEnumerable<SpineAtlasPageInfo> atlasPages,
        IEnumerable<SpineAtlasRegionInfo> atlasRegions,
        IEnumerable<ViewerDiagnostic> diagnostics)
    {
        ExportMetadata = Guard.NotNull(exportMetadata, nameof(exportMetadata));
        Animations = Guard.MaterializeReadOnlyList(animations, nameof(animations));
        Skins = Guard.MaterializeReadOnlyList(skins, nameof(skins));
        Bones = Guard.MaterializeReadOnlyList(bones, nameof(bones));
        Slots = Guard.MaterializeReadOnlyList(slots, nameof(slots));
        Attachments = Guard.MaterializeReadOnlyList(attachments, nameof(attachments));
        AtlasPages = Guard.MaterializeReadOnlyList(atlasPages, nameof(atlasPages));
        AtlasRegions = Guard.MaterializeReadOnlyList(atlasRegions, nameof(atlasRegions));
        Diagnostics = Guard.MaterializeReadOnlyList(diagnostics, nameof(diagnostics));
    }

    /// <summary>
    /// Gets the export metadata summary.
    /// </summary>
    public SpineExportMetadata ExportMetadata { get; init; }

    /// <summary>
    /// Gets the inspected animation entries.
    /// </summary>
    public IReadOnlyList<SpineAnimationInfo> Animations { get; init; }

    /// <summary>
    /// Gets the inspected skin entries.
    /// </summary>
    public IReadOnlyList<SpineSkinInfo> Skins { get; init; }

    /// <summary>
    /// Gets the inspected bone entries.
    /// </summary>
    public IReadOnlyList<SpineBoneInfo> Bones { get; init; }

    /// <summary>
    /// Gets the inspected slot entries.
    /// </summary>
    public IReadOnlyList<SpineSlotInfo> Slots { get; init; }

    /// <summary>
    /// Gets the inspected attachment entries.
    /// </summary>
    public IReadOnlyList<SpineAttachmentInfo> Attachments { get; init; }

    /// <summary>
    /// Gets the inspected atlas page entries.
    /// </summary>
    public IReadOnlyList<SpineAtlasPageInfo> AtlasPages { get; init; }

    /// <summary>
    /// Gets the inspected atlas region entries.
    /// </summary>
    public IReadOnlyList<SpineAtlasRegionInfo> AtlasRegions { get; init; }

    /// <summary>
    /// Gets the diagnostics produced during inspection.
    /// </summary>
    public IReadOnlyList<ViewerDiagnostic> Diagnostics { get; init; }
}
