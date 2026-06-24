using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using SpineViewer.Features.Viewport.ViewModels;

namespace SpineViewer.Features.Viewport.Views;

/// <summary>
/// Hosts the rewrite-side viewport render surface and performs view-only lifecycle wiring.
/// </summary>
public partial class ViewportView : UserControl
{
    private readonly SpineRenderHost _renderHost;
    private ViewportViewModel? _activeViewModel;
    private bool _isAttachedToVisualTree;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportView"/> class.
    /// </summary>
    public ViewportView()
    {
        AvaloniaXamlLoader.Load(this);

        _renderHost = this.FindControl<SpineRenderHost>("RenderHost")
            ?? throw new InvalidOperationException("The viewport render host could not be located.");

        _renderHost.PointerPressed += OnRenderHostPointerPressed;
        _renderHost.PointerMoved += OnRenderHostPointerMoved;
        _renderHost.PointerReleased += OnRenderHostPointerReleased;
        _renderHost.PointerCaptureLost += OnRenderHostPointerCaptureLost;
        _renderHost.PointerWheelChanged += OnRenderHostPointerWheelChanged;
        _renderHost.SizeChanged += OnRenderHostSizeChanged;
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        DataContextChanged += OnDataContextChanged;
    }

    private void ActivateCurrentViewModel()
    {
        if (DataContext is not ViewportViewModel viewModel)
        {
            return;
        }

        _activeViewModel = viewModel;
        viewModel.UpdateHostLayout(_renderHost.Bounds.Width, _renderHost.Bounds.Height);
        viewModel.Activate();
    }

    private void DeactivateCurrentViewModel()
    {
        if (_activeViewModel is null)
        {
            return;
        }

        _activeViewModel.Deactivate();
        _activeViewModel = null;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = true;
        ActivateCurrentViewModel();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        DeactivateCurrentViewModel();

        if (_isAttachedToVisualTree)
        {
            ActivateCurrentViewModel();
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        DeactivateCurrentViewModel();
    }

    private void OnRenderHostPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (DataContext is ViewportViewModel viewModel)
        {
            viewModel.EndPan();
        }
    }

    private void OnRenderHostPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not ViewportViewModel viewModel)
        {
            return;
        }

        Point pointerPosition = e.GetPosition(_renderHost);
        viewModel.UpdatePan(pointerPosition.X, pointerPosition.Y);
    }

    private void OnRenderHostPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not ViewportViewModel viewModel)
        {
            return;
        }

        Point pointerPosition = e.GetPosition(_renderHost);
        PointerPointProperties properties = e.GetCurrentPoint(_renderHost).Properties;
        if (!properties.IsLeftButtonPressed && !properties.IsMiddleButtonPressed)
        {
            return;
        }

        _renderHost.Focus();
        viewModel.BeginPan(pointerPosition.X, pointerPosition.Y);
        e.Pointer.Capture(_renderHost);
        e.Handled = true;
    }

    private void OnRenderHostPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is ViewportViewModel viewModel)
        {
            viewModel.EndPan();
        }

        if (Equals(e.Pointer.Captured, _renderHost))
        {
            e.Pointer.Capture(null);
        }

        e.Handled = true;
    }

    private void OnRenderHostPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not ViewportViewModel viewModel)
        {
            return;
        }

        Point pointerPosition = e.GetPosition(_renderHost);
        viewModel.HandlePointerWheel(pointerPosition.X, pointerPosition.Y, e.Delta.Y);
        e.Handled = true;
    }

    private void OnRenderHostSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is ViewportViewModel viewModel)
        {
            viewModel.UpdateHostLayout(e.NewSize.Width, e.NewSize.Height);
        }
    }
}
