using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Views;

/// <summary>
/// Draws immutable viewport render scenes without owning scheduling, camera, or overlay logic.
/// </summary>
public sealed class SpineRenderHost : Control
{
    /// <summary>
    /// Identifies the <see cref="Scene"/> property.
    /// </summary>
    public static readonly StyledProperty<ViewportRenderScene?> SceneProperty =
        AvaloniaProperty.Register<SpineRenderHost, ViewportRenderScene?>(nameof(Scene));

    static SpineRenderHost()
    {
        AffectsRender<SpineRenderHost>(SceneProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpineRenderHost"/> class.
    /// </summary>
    public SpineRenderHost()
    {
        ClipToBounds = true;
    }

    /// <summary>
    /// Gets or sets the immutable render scene to draw.
    /// </summary>
    public ViewportRenderScene? Scene
    {
        get => GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        ViewportRenderScene scene = Scene ?? ViewportRenderScene.Empty;
        Rect bounds = new(0.0, 0.0, Bounds.Width, Bounds.Height);

        context.FillRectangle(new SolidColorBrush(scene.BackgroundColor), bounds);

        foreach (ViewportOverlayLine worldLine in scene.WorldLines)
        {
            Point start = TransformPoint(scene.Transform, worldLine.StartX, worldLine.StartY);
            Point end = TransformPoint(scene.Transform, worldLine.EndX, worldLine.EndY);
            context.DrawLine(new Pen(new SolidColorBrush(worldLine.Color), worldLine.Thickness), start, end);
        }
    }

    private static Point TransformPoint(
        ViewportRenderTransform transform,
        double worldX,
        double worldY)
    {
        return new Point(
            transform.TranslateX + (worldX * transform.Scale),
            transform.TranslateY + (worldY * transform.Scale));
    }
}
