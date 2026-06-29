using Avalonia.Media;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Rendering;

internal static class RuntimePreviewOverlayPalette
{
    internal static Color BoneColor => Color.FromRgb(0xF2, 0xCC, 0x8F);

    internal static Color BoundsColor => Color.FromRgb(0x9A, 0xD1, 0x8B);

    internal static Color MeshColor => Color.FromRgb(0xF4, 0x7F, 0x6B);

    internal static Color SlotColor => Color.FromRgb(0x74, 0xC6, 0xE5);

    internal static Color LabelColor => Color.FromRgb(0xE7, 0xE0, 0xD3);

    internal static IReadOnlyList<ViewportOverlayLine> CreateBoundsLines(ViewportContentBounds bounds)
    {
        return
        [
            new ViewportOverlayLine(bounds.MinimumX, bounds.MinimumY, bounds.MaximumX, bounds.MinimumY, BoundsColor, 2.2),
            new ViewportOverlayLine(bounds.MaximumX, bounds.MinimumY, bounds.MaximumX, bounds.MaximumY, BoundsColor, 2.2),
            new ViewportOverlayLine(bounds.MaximumX, bounds.MaximumY, bounds.MinimumX, bounds.MaximumY, BoundsColor, 2.2),
            new ViewportOverlayLine(bounds.MinimumX, bounds.MaximumY, bounds.MinimumX, bounds.MinimumY, BoundsColor, 2.2),
        ];
    }
}
