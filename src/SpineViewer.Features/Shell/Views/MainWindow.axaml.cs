using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SpineViewer.Core.Models;
using SpineViewer.Features.Shell.ViewModels;

namespace SpineViewer.Features.Shell.Views;

/// <summary>
/// Hosts the initial shell window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly Border _detailsPane;
    private readonly Grid _shellContentGrid;
    private readonly Border _viewportPane;
    private readonly Border _workspacePane;
    private bool _isApplyingPersistedWindowState;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _shellContentGrid = this.FindControl<Grid>("ShellContentGrid")
            ?? throw new InvalidOperationException("The shell content grid could not be located.");
        _workspacePane = this.FindControl<Border>("WorkspacePane")
            ?? throw new InvalidOperationException("The workspace pane could not be located.");
        _viewportPane = this.FindControl<Border>("ViewportPane")
            ?? throw new InvalidOperationException("The viewport pane could not be located.");
        _detailsPane = this.FindControl<Border>("DetailsPane")
            ?? throw new InvalidOperationException("The details pane could not be located.");

        DragDrop.SetAllowDrop(this, true);
        DragDrop.AddDragOverHandler(this, OnDragOver);
        DragDrop.AddDropHandler(this, OnDrop);
        Closing += OnClosing;
        Opened += OnOpened;
        SizeChanged += OnWindowSizeChanged;
        DataContextChanged += OnDataContextChanged;
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

    private void ApplyResponsiveLayout()
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        viewModel.UpdateLayoutWidth(Bounds.Width);

        if (viewModel.IsCompactLayout)
        {
            _shellContentGrid.ColumnDefinitions = new ColumnDefinitions("*");
            _shellContentGrid.RowDefinitions = new RowDefinitions("*,Auto,Auto");
            _shellContentGrid.ColumnSpacing = 0;
            _shellContentGrid.RowSpacing = 12;

            Grid.SetColumn(_viewportPane, 0);
            Grid.SetRow(_viewportPane, 0);
            Grid.SetColumn(_workspacePane, 0);
            Grid.SetRow(_workspacePane, 1);
            Grid.SetColumn(_detailsPane, 0);
            Grid.SetRow(_detailsPane, 2);
            return;
        }

        _shellContentGrid.ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto");
        _shellContentGrid.RowDefinitions = new RowDefinitions("*");
        _shellContentGrid.ColumnSpacing = 16;
        _shellContentGrid.RowSpacing = 0;

        Grid.SetColumn(_workspacePane, 0);
        Grid.SetRow(_workspacePane, 0);
        Grid.SetColumn(_viewportPane, 1);
        Grid.SetRow(_viewportPane, 0);
        Grid.SetColumn(_detailsPane, 2);
        Grid.SetRow(_detailsPane, 0);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        ApplyPersistedWindowState();
        ApplyResponsiveLayout();
    }

    private async void OnOpenModelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || !viewModel.CanStartOpenFlow)
        {
            return;
        }

        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        IStorageProvider? storageProvider = topLevel?.StorageProvider;
        if (storageProvider?.CanOpen != true)
        {
            return;
        }

        IReadOnlyList<IStorageFile> selectedFiles = await storageProvider
            .OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = true,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Spine Files")
                        {
                            Patterns = ["*.atlas", "*.json", "*.skel", "*.bytes"],
                        },
                    ],
                    Title = "Open Spine Files",
                })
            .ConfigureAwait(true);

        IReadOnlyList<string> selectedPaths = GetLocalFilePaths(selectedFiles);
        if (selectedPaths.Count == 0)
        {
            return;
        }

        await viewModel.OpenFilesAsync(selectedPaths, CancellationToken.None).ConfigureAwait(true);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = HasLocalFilePaths(e.DataTransfer)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || !viewModel.CanStartOpenFlow)
        {
            return;
        }

        IReadOnlyList<string> selectedPaths = GetLocalFilePaths(e.DataTransfer.TryGetFiles());
        if (selectedPaths.Count == 0)
        {
            return;
        }

        await viewModel.OpenFilesAsync(selectedPaths, CancellationToken.None).ConfigureAwait(true);
        e.Handled = true;
    }

    private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
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

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        ViewerWindowState persistedWindowState = new(
            Bounds.Width,
            Bounds.Height,
            Position.X,
            Position.Y,
            WindowState == WindowState.Maximized);
        viewModel.Settings.PersistWindowStateAsync(persistedWindowState, CancellationToken.None).GetAwaiter().GetResult();
    }

    private static IReadOnlyList<string> GetLocalFilePaths(IEnumerable<IStorageItem>? storageItems)
    {
        if (storageItems is null)
        {
            return Array.Empty<string>();
        }

        List<string> selectedPaths = [];

        foreach (IStorageItem storageItem in storageItems)
        {
            using (storageItem)
            {
                if (storageItem is not IStorageFile)
                {
                    continue;
                }

                string? localPath = storageItem.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(localPath))
                {
                    selectedPaths.Add(localPath);
                }
            }
        }

        return selectedPaths;
    }

    private static bool HasLocalFilePaths(IDataTransfer dataTransfer)
    {
        IEnumerable<IStorageItem>? storageItems = dataTransfer.TryGetFiles();
        return storageItems is not null && storageItems.Any(static storageItem => storageItem is IStorageFile);
    }
}
