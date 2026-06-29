namespace SpineViewer.Features.Viewport.Models;

/// <summary>
/// Captures the current runtime-driven overlay primitives for one synchronized Spine preview pose.
/// </summary>
public sealed record SpineRuntimePreviewOverlaySnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRuntimePreviewOverlaySnapshot"/> class.
    /// </summary>
    /// <param name="boneLines">The current animated bone-guide lines.</param>
    /// <param name="boundsLines">The current animated bounds outline lines.</param>
    /// <param name="meshLines">The current animated mesh-wireframe lines.</param>
    /// <param name="slotOutlineLines">The current animated slot-outline lines.</param>
    /// <param name="labelText">The current animated labels.</param>
    /// <param name="contentBounds">The current animated content bounds, if any.</param>
    public SpineRuntimePreviewOverlaySnapshot(
        IEnumerable<ViewportOverlayLine> boneLines,
        IEnumerable<ViewportOverlayLine> boundsLines,
        IEnumerable<ViewportOverlayLine> meshLines,
        IEnumerable<ViewportOverlayLine> slotOutlineLines,
        IEnumerable<ViewportOverlayText> labelText,
        ViewportContentBounds? contentBounds)
    {
        ArgumentNullException.ThrowIfNull(boneLines);
        ArgumentNullException.ThrowIfNull(boundsLines);
        ArgumentNullException.ThrowIfNull(meshLines);
        ArgumentNullException.ThrowIfNull(slotOutlineLines);
        ArgumentNullException.ThrowIfNull(labelText);

        BoneLines = boneLines.ToArray();
        BoundsLines = boundsLines.ToArray();
        MeshLines = meshLines.ToArray();
        SlotOutlineLines = slotOutlineLines.ToArray();
        LabelText = labelText.ToArray();
        ContentBounds = contentBounds;
    }

    /// <summary>
    /// Gets the current animated bone-guide lines.
    /// </summary>
    public IReadOnlyList<ViewportOverlayLine> BoneLines { get; init; }

    /// <summary>
    /// Gets the current animated bounds outline lines.
    /// </summary>
    public IReadOnlyList<ViewportOverlayLine> BoundsLines { get; init; }

    /// <summary>
    /// Gets the current animated mesh-wireframe lines.
    /// </summary>
    public IReadOnlyList<ViewportOverlayLine> MeshLines { get; init; }

    /// <summary>
    /// Gets the current animated slot-outline lines.
    /// </summary>
    public IReadOnlyList<ViewportOverlayLine> SlotOutlineLines { get; init; }

    /// <summary>
    /// Gets the current animated labels.
    /// </summary>
    public IReadOnlyList<ViewportOverlayText> LabelText { get; init; }

    /// <summary>
    /// Gets the current animated content bounds, if any.
    /// </summary>
    public ViewportContentBounds? ContentBounds { get; init; }
}
