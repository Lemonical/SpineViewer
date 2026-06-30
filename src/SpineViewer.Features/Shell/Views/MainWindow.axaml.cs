using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using SpineViewer.Core.Models;
using SpineViewer.Features.Settings.ViewModels;
using SpineViewer.Features.Shell.ViewModels;
using System.Diagnostics;
using System.ComponentModel;

namespace SpineViewer.Features.Shell.Views;

/// <summary>
/// Hosts the initial shell window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly IReadOnlyList<(
        Control Grip,
        WindowDecorationsElementRole Role,
        WindowEdge Edge)> _customChromeResizeGrips;
    private bool _isCloseConfirmed;
    private bool _isPersistingClose;
    private bool _isApplyingPersistedWindowState;
    private ViewerSettingsViewModel? _trackedSettingsViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _customChromeResizeGrips =
        [
            (FindRequiredControl("ResizeNorthGrip"), WindowDecorationsElementRole.ResizeN, WindowEdge.North),
            (FindRequiredControl("ResizeSouthGrip"), WindowDecorationsElementRole.ResizeS, WindowEdge.South),
            (FindRequiredControl("ResizeEastGrip"), WindowDecorationsElementRole.ResizeE, WindowEdge.East),
            (FindRequiredControl("ResizeWestGrip"), WindowDecorationsElementRole.ResizeW, WindowEdge.West),
            (FindRequiredControl("ResizeNorthWestGrip"), WindowDecorationsElementRole.ResizeNW, WindowEdge.NorthWest),
            (FindRequiredControl("ResizeNorthEastGrip"), WindowDecorationsElementRole.ResizeNE, WindowEdge.NorthEast),
            (FindRequiredControl("ResizeSouthWestGrip"), WindowDecorationsElementRole.ResizeSW, WindowEdge.SouthWest),
            (FindRequiredControl("ResizeSouthEastGrip"), WindowDecorationsElementRole.ResizeSE, WindowEdge.SouthEast),
        ];

        foreach ((Control grip, _, _) in _customChromeResizeGrips)
        {
            grip.PointerPressed += OnCustomChromeResizeGripPointerPressed;
        }

        Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://SpineViewer.Features/Assets/favicon.ico")));
        Closing += OnClosing;
        DataContextChanged += OnDataContextChanged;
        Opened += OnOpened;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        ApplyPersistedWindowState();
        ApplyWindowChromePreference();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class with its shell view model.
    /// </summary>
    /// <param name="viewModel">The shell view model for the window.</param>
    public MainWindow(MainWindowViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }

    private void ApplyPersistedWindowState()
    {
        if (_isApplyingPersistedWindowState ||
            DataContext is not MainWindowViewModel viewModel ||
            viewModel.Settings.PersistedWindowState is not ViewerWindowState persistedWindowState)
        {
            return;
        }

        _isApplyingPersistedWindowState = true;

        try
        {
            Width = persistedWindowState.Width;
            Height = persistedWindowState.Height;

            if (persistedWindowState.PositionX.HasValue && persistedWindowState.PositionY.HasValue)
            {
                Position = new PixelPoint(persistedWindowState.PositionX.Value, persistedWindowState.PositionY.Value);
            }

            if (persistedWindowState.IsMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }
        finally
        {
            _isApplyingPersistedWindowState = false;
        }
    }

    private void ApplyWindowChromePreference()
    {
        bool useCustomTitleBar = DataContext is MainWindowViewModel { Settings.UseCustomTitleBar: true };
        ExtendClientAreaToDecorationsHint = useCustomTitleBar;
        ExtendClientAreaTitleBarHeightHint = useCustomTitleBar ? 44 : -1;
        WindowDecorations = useCustomTitleBar
            ? WindowDecorations.None
            : WindowDecorations.Full;
        RefreshCustomChromeResizeGrips(useCustomTitleBar && CanResize);
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isCloseConfirmed)
        {
            UpdateTrackedSettingsViewModel(null);
            return;
        }

        if (_isPersistingClose)
        {
            e.Cancel = true;
            return;
        }

        e.Cancel = true;
        _isPersistingClose = true;
        _ = PersistWindowStateAndCloseAsync();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateTrackedSettingsViewModel((DataContext as MainWindowViewModel)?.Settings);
        ApplyPersistedWindowState();
        ApplyWindowChromePreference();
    }

    private void OnTrackedSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null || e.PropertyName == nameof(ViewerSettingsViewModel.UseCustomTitleBar))
        {
            ApplyWindowChromePreference();
        }

        if (e.PropertyName is null || e.PropertyName == nameof(ViewerSettingsViewModel.PersistedWindowState))
        {
            ApplyPersistedWindowState();
        }
    }

    private void UpdateTrackedSettingsViewModel(ViewerSettingsViewModel? settingsViewModel)
    {
        if (ReferenceEquals(_trackedSettingsViewModel, settingsViewModel))
        {
            return;
        }

        if (_trackedSettingsViewModel is not null)
        {
            _trackedSettingsViewModel.PropertyChanged -= OnTrackedSettingsPropertyChanged;
        }

        _trackedSettingsViewModel = settingsViewModel;

        if (_trackedSettingsViewModel is not null)
        {
            _trackedSettingsViewModel.PropertyChanged += OnTrackedSettingsPropertyChanged;
        }
    }

    private static string GetWindowStatePersistenceErrorMessage(Exception exception)
    {
        return $"Failed to persist the window state during shutdown: {exception}";
    }

    private ViewerWindowState CreatePersistedWindowState()
    {
        return new ViewerWindowState(
            Bounds.Width,
            Bounds.Height,
            Position.X,
            Position.Y,
            WindowState == WindowState.Maximized);
    }

    private Control FindRequiredControl(string controlName)
    {
        return this.FindControl<Control>(controlName)
            ?? throw new InvalidOperationException($"The {controlName} control could not be located.");
    }

    private async Task PersistWindowStateAndCloseAsync()
    {
        try
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.Settings
                    .PersistWindowStateAsync(CreatePersistedWindowState(), CancellationToken.None)
                    .ConfigureAwait(true);
            }
        }
        catch (Exception exception)
        {
            Trace.TraceError(GetWindowStatePersistenceErrorMessage(exception));
        }
        finally
        {
            _isCloseConfirmed = true;
            _isPersistingClose = false;
            await Dispatcher.UIThread.InvokeAsync(Close);
        }
    }

    private void RefreshCustomChromeResizeGrips(bool enableCustomResizeGrips)
    {
        foreach ((Control grip, WindowDecorationsElementRole role, _) in _customChromeResizeGrips)
        {
            grip.IsVisible = enableCustomResizeGrips;
            WindowDecorationProperties.SetElementRole(
                grip,
                enableCustomResizeGrips
                    ? role
                    : WindowDecorationsElementRole.None);
        }
    }

    private void OnCustomChromeResizeGripPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control grip ||
            !e.GetCurrentPoint(grip).Properties.IsLeftButtonPressed ||
            !ShouldUseCustomResizeGrips() ||
            WindowState != WindowState.Normal)
        {
            return;
        }

        WindowEdge? edge = _customChromeResizeGrips
            .Where(tuple => ReferenceEquals(tuple.Grip, grip))
            .Select(tuple => (WindowEdge?)tuple.Edge)
            .FirstOrDefault();
        if (!edge.HasValue)
        {
            return;
        }

        BeginResizeDrag(edge.Value, e);
        e.Handled = true;
    }

    private bool ShouldUseCustomResizeGrips()
    {
        return CanResize &&
            DataContext is MainWindowViewModel { Settings.UseCustomTitleBar: true };
    }
}
