using Avalonia.Threading;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Models;

namespace SpineViewer.Features.Viewport.Services;

/// <summary>
/// Schedules recurring viewport redraws on the UI dispatcher while playback is active.
/// </summary>
public sealed class ViewportFrameScheduler : IViewportFrameScheduler
{
    private static readonly TimeSpan DefaultFrameInterval = TimeSpan.FromMilliseconds(1000.0 / 60.0);
    private readonly IRenderInvalidationService _renderInvalidationService;
    private readonly DispatcherTimer _timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportFrameScheduler"/> class.
    /// </summary>
    /// <param name="renderInvalidationService">The invalidation service used to request new frames.</param>
    public ViewportFrameScheduler(IRenderInvalidationService renderInvalidationService)
    {
        _renderInvalidationService =
            renderInvalidationService ?? throw new ArgumentNullException(nameof(renderInvalidationService));

        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = DefaultFrameInterval,
        };
        _timer.Tick += OnTick;
    }

    /// <inheritdoc />
    public TimeSpan FrameInterval => DefaultFrameInterval;

    /// <inheritdoc />
    public bool IsRunning => _timer.IsEnabled;

    /// <inheritdoc />
    public void Start()
    {
        if (!_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _renderInvalidationService.RequestInvalidation(RenderInvalidationReason.FrameTick);
    }
}
