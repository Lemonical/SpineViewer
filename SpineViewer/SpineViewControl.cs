using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using SpineViewer.Spine;
using System;

namespace SpineViewer;

public class SpineViewControl : Control
{
    public static readonly DirectProperty<SpineViewControl, SpineAnimation[]> AnimationTracksProperty =
        AvaloniaProperty.RegisterDirect<SpineViewControl, SpineAnimation[]>(nameof(AnimationTracks), c => c.AnimationTracks, (c, o) => c.SetTracks(o));

    public static readonly DirectProperty<SpineViewControl, uint> FpsProperty =
        AvaloniaProperty.RegisterDirect<SpineViewControl, uint>(nameof(Fps), c => c.Fps, (c, o) => c.Fps = o);

    public static readonly DirectProperty<SpineViewControl, ISpineRenderer> RendererProperty =
        AvaloniaProperty.RegisterDirect<SpineViewControl, ISpineRenderer>(nameof(Renderer), c => c.Renderer, (c, o) => c.SetRenderer(o));

    public static readonly DirectProperty<SpineViewControl, float> ScaleProperty =
        AvaloniaProperty.RegisterDirect<SpineViewControl, float>(nameof(Scale), c => c.Scale, (c, o) => c.ChangeScale(o, c.Bounds.Width / 2, c.Bounds.Height / 2, true), 1f);

    private SpineAnimation[] _animationTracks;
    private uint _fps = 60;
    private ISpineRenderer _renderer;
    private float _scale;

    private bool _dontRaiseAgain;

    private Matrix _matrix = Matrix.Identity;
    private Point _previousPoint;

    public SpineAnimation[] AnimationTracks
    {
        get => _animationTracks;
        set => SetAndRaise(AnimationTracksProperty, ref _animationTracks, value);
    }

    public uint Fps
    {
        get => _fps;
        set => SetAndRaise(FpsProperty, ref _fps, value);
    }

    public ISpineRenderer Renderer
    {
        get => _renderer;
        set => SetAndRaise(RendererProperty, ref _renderer, value);
    }

    public float Scale
    {
        get => _scale;
        set => SetAndRaise(ScaleProperty, ref _scale, value);
    }

    public SpineViewControl()
    {
        AffectsRender<SpineViewControl>(AnimationTracksProperty, RendererProperty);
        ClipToBounds = true;

        AddHandler(PointerPressedEvent, PointerPressedHandler);
        AddHandler(PointerMovedEvent, PointerMovedHandler);
        AddHandler(PointerWheelChangedEvent, PointerWheelChangedHandler);
    }

    public override void Render(DrawingContext context)
    {
        if (_matrix.IsIdentity)
            _matrix = Matrix.CreateTranslation(Bounds.Width / 2, Bounds.Height).Append(Matrix.CreateScale(Scale, Scale));
        if (_renderer == null) return;
        context.Custom(new SpriteDrawOp(_renderer, new Rect(0, 0, Bounds.Width, Bounds.Height), _matrix.ToSKMatrix()));
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            _renderer.NextFrame(1f / Fps);
            InvalidateVisual();
        }, DispatcherPriority.Background);
    }

    private void SetTracks(SpineAnimation[] animations)
    {
        _animationTracks = animations;
        if (_animationTracks == null || _renderer == null) return;
        _renderer.SetAnimations(animations);
    }

    private void SetRenderer(ISpineRenderer renderer)
    {
        var old = _renderer;
        _renderer = renderer;
        RaisePropertyChanged(RendererProperty, old, _renderer);
    }

    private void PointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        _previousPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _previousPoint;
        _previousPoint = currentPoint;
        _matrix = _matrix.Append(Matrix.CreateTranslation(delta.X, delta.Y));
    }

    private void PointerWheelChangedHandler(object? sender, PointerWheelEventArgs e)
    {
        var currentPoint = e.GetPosition(this);            
        var delta = e.Delta.Y;
        var scale = delta > 0 
            ? 1.2 
            : 1 / 1.2;
        ChangeScale(scale, currentPoint.X, currentPoint.Y);
    }

    private void ChangeScale(double scale, double x, double y, bool fromRaised = false)
    {
        if (_dontRaiseAgain) return;
        _dontRaiseAgain = true;

        var matrix = _matrix;
        if (fromRaised && Matrix.TryDecomposeTransform(_matrix, out var transform))
            matrix = Matrix.Identity
                .Append(Matrix.CreateTranslation(transform.Translate.X, transform.Translate.Y));
        if (scale is double.NaN or double.PositiveInfinity or double.NegativeInfinity or <= 0)
            scale = 1;

        _matrix = matrix.Append(Matrix.CreateTranslation(-x, -y))
            .Append(Matrix.CreateScale(scale, scale))
            .Append(Matrix.CreateTranslation(x, y));

        Scale = (float)_matrix.M11;

        _dontRaiseAgain = false;
    }
}

public class SpriteDrawOp(ISpineRenderer spineRenderer, Rect bounds, SKMatrix matrix) : ICustomDrawOperation
{
    private readonly ISpineRenderer _spineRenderer = spineRenderer;

    public Rect Bounds => bounds;
    public bool HitTest(Point p)
    {
        return p.X <= Bounds.Width && p.X >= 0 && p.Y <= Bounds.Height && p.Y >= 0;
    }
    public bool Equals(ICustomDrawOperation? other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        if (context.TryGetFeature<ISkiaSharpApiLeaseFeature>() is not ISkiaSharpApiLeaseFeature leaseFeature)
            return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        canvas.SetMatrix(matrix);
        canvas.Clear(SKColors.Black);

        _spineRenderer.Draw(canvas);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
