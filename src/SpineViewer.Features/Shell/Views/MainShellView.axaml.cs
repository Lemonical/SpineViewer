using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Avalonia.Visuals;
using System.ComponentModel;
using SpineViewer.Features.Settings.ViewModels;
using SpineViewer.Features.Shell.ViewModels;

namespace SpineViewer.Features.Shell.Views;

/// <summary>
/// Hosts the shared shell content for both desktop and browser application lifetimes.
/// </summary>
public partial class MainShellView : UserControl
{
    private readonly StackPanel _brandHost;
    private readonly Border _detailsPane;
    private readonly Grid _shellContentGrid;
    private readonly StackPanel _sessionSummaryHost;
    private readonly Border _titleBarBorder;
    private readonly Border _titleBarDragRegion;
    private readonly Border _viewportPane;
    private ViewerSettingsViewModel? _trackedSettingsViewModel;
    private readonly StackPanel _windowControlsHost;
    private readonly Border _workspacePane;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainShellView"/> class.
    /// </summary>
    public MainShellView()
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
        _titleBarBorder = this.FindControl<Border>("TitleBarBorder")
            ?? throw new InvalidOperationException("The title bar border could not be located.");
        _titleBarDragRegion = this.FindControl<Border>("TitleBarDragRegion")
            ?? throw new InvalidOperationException("The title bar drag region could not be located.");
        _brandHost = this.FindControl<StackPanel>("BrandHost")
            ?? throw new InvalidOperationException("The brand host could not be located.");
        _sessionSummaryHost = this.FindControl<StackPanel>("SessionSummaryHost")
            ?? throw new InvalidOperationException("The session summary host could not be located.");
        _windowControlsHost = this.FindControl<StackPanel>("WindowControlsHost")
            ?? throw new InvalidOperationException("The window controls host could not be located.");

        DragDrop.SetAllowDrop(this, true);
        DragDrop.AddDragOverHandler(this, OnDragOver);
        DragDrop.AddDropHandler(this, OnDrop);
        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
        DataContextChanged += OnDataContextChanged;
        KeyDown += OnShellKeyDown;
        SizeChanged += OnShellSizeChanged;
        _titleBarBorder.PointerPressed += OnTitleBarPointerPressed;
        _titleBarBorder.DoubleTapped += OnTitleBarDoubleTapped;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainShellView"/> class with its shell view model.
    /// </summary>
    /// <param name="viewModel">The shell view model for the shared shell surface.</param>
    public MainShellView(MainWindowViewModel viewModel)
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
            _shellContentGrid.RowSpacing = 0;

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
        _shellContentGrid.ColumnSpacing = 0;
        _shellContentGrid.RowSpacing = 0;

        Grid.SetColumn(_workspacePane, 0);
        Grid.SetRow(_workspacePane, 0);
        Grid.SetColumn(_viewportPane, 1);
        Grid.SetRow(_viewportPane, 0);
        Grid.SetColumn(_detailsPane, 2);
        Grid.SetRow(_detailsPane, 0);
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ApplyResponsiveLayout();
        RefreshWindowChromeState();
    }

    private void OnCloseWindowClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.Close();
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateTrackedSettingsViewModel(null);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        UpdateTrackedSettingsViewModel((DataContext as MainWindowViewModel)?.Settings);
        ApplyResponsiveLayout();
        RefreshWindowChromeState();
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || !viewModel.CanStartOpenFlow)
        {
            return;
        }

        IReadOnlyList<string> selectedPaths = await PrepareSelectedPathsAsync(
            e.DataTransfer.TryGetFiles(),
            CancellationToken.None).ConfigureAwait(true);
        if (selectedPaths.Count == 0)
        {
            return;
        }

        await viewModel.OpenFilesAsync(selectedPaths, CancellationToken.None).ConfigureAwait(true);
        e.Handled = true;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = HasFilePayload(e.DataTransfer)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void OnMaximizeOrRestoreWindowClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnMinimizeWindowClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            window.WindowState = WindowState.Minimized;
        }
    }

    private async void OnOpenModelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel || !viewModel.CanStartOpenFlow)
        {
            return;
        }

        await OpenModelAsync(viewModel).ConfigureAwait(true);
    }

    private async void OnRemoveRecentProjectClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (sender is not Control { Tag: SpineViewer.Core.Models.SpineProjectReference projectReference })
        {
            return;
        }

        if (viewModel.RemoveRecentProjectCommand.CanExecute(projectReference))
        {
            await viewModel.RemoveRecentProjectCommand.ExecuteAsync(projectReference).ConfigureAwait(true);
        }
    }

    private async void OnRecentDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (viewModel.OpenSelectedRecentProjectCommand.CanExecute(null))
        {
            await viewModel.OpenSelectedRecentProjectCommand.ExecuteAsync(null).ConfigureAwait(true);
        }
    }

    private async void OnShellKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        KeyModifiers modifiers = e.KeyModifiers;
        if (modifiers == KeyModifiers.Control && e.Key == Key.O)
        {
            await OpenModelAsync(viewModel).ConfigureAwait(true);
            e.Handled = true;
            return;
        }

        if (modifiers == (KeyModifiers.Control | KeyModifiers.Shift) &&
            e.Key == Key.R &&
            viewModel.RetryLastOpenCommand.CanExecute(null))
        {
            await viewModel.RetryLastOpenCommand.ExecuteAsync(null).ConfigureAwait(true);
            e.Handled = true;
        }
    }

    private void OnShellSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
        RefreshWindowChromeState();
    }

    private Task OpenModelAsync(MainWindowViewModel viewModel)
    {
        if (!viewModel.CanStartOpenFlow)
        {
            return Task.CompletedTask;
        }

        return OpenModelCoreAsync(viewModel);
    }

    private async Task OpenModelCoreAsync(MainWindowViewModel viewModel)
    {
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
                        new FilePickerFileType("Spine Workspace Files")
                        {
                            Patterns =
                            [
                                "*.atlas",
                                "*.json",
                                "*.skel",
                                "*.bytes",
                                "*.png",
                                "*.jpg",
                                "*.jpeg",
                                "*.webp",
                            ],
                        },
                    ],
                    Title = "Open Spine Workspace Files",
                })
            .ConfigureAwait(true);

        IReadOnlyList<string> selectedPaths = await PrepareSelectedPathsAsync(
            selectedFiles,
            CancellationToken.None).ConfigureAwait(true);
        if (selectedPaths.Count == 0)
        {
            return;
        }

        await viewModel.OpenFilesAsync(selectedPaths, CancellationToken.None).ConfigureAwait(true);
    }

    private void OnTrackedSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null || e.PropertyName == nameof(ViewerSettingsViewModel.UseCustomTitleBar))
        {
            RefreshWindowChromeState();
        }
    }

    private void OnTitleBarDoubleTapped(object? sender, TappedEventArgs e)
    {
        Window? window = GetCustomChromeWindow();
        if (window is null)
        {
            return;
        }

        if (!window.CanResize || IsInteractiveTitleBarSource(e.Source))
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        e.Handled = true;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Window? window = GetCustomChromeWindow();
        if (window is null)
        {
            return;
        }

        if (!e.GetCurrentPoint(_titleBarBorder).Properties.IsLeftButtonPressed ||
            IsInteractiveTitleBarSource(e.Source))
        {
            return;
        }

        window.BeginMoveDrag(e);
        e.Handled = true;
    }

    private void RefreshWindowChromeState()
    {
        bool canUseWindowChrome = TopLevel.GetTopLevel(this) is Window;
        bool useCustomTitleBar = DataContext is MainWindowViewModel { Settings.UseCustomTitleBar: true };

        _windowControlsHost.IsVisible = canUseWindowChrome && useCustomTitleBar;
        WindowDecorationsElementRole titleBarRole = canUseWindowChrome && useCustomTitleBar
            ? WindowDecorationsElementRole.TitleBar
            : WindowDecorationsElementRole.None;

        WindowDecorationProperties.SetElementRole(_titleBarBorder, titleBarRole);
        WindowDecorationProperties.SetElementRole(_titleBarDragRegion, titleBarRole);
        WindowDecorationProperties.SetElementRole(_brandHost, titleBarRole);
        WindowDecorationProperties.SetElementRole(_sessionSummaryHost, titleBarRole);
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

    private static string CreateImportedWorkspacePath()
    {
        string importDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "SpineViewer",
            "imports",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(importDirectoryPath);
        return importDirectoryPath;
    }

    private static bool HasFilePayload(IDataTransfer dataTransfer)
    {
        IEnumerable<IStorageItem>? storageItems = dataTransfer.TryGetFiles();
        return storageItems is not null && storageItems.Any(static storageItem => storageItem is IStorageFile);
    }

    private static async Task<IReadOnlyList<string>> PrepareSelectedPathsAsync(
        IEnumerable<IStorageItem>? storageItems,
        CancellationToken cancellationToken)
    {
        if (storageItems is null)
        {
            return Array.Empty<string>();
        }

        List<string> selectedPaths = [];
        string? importDirectoryPath = null;
        HashSet<string> usedTargetPaths = [];

        foreach (IStorageItem storageItem in storageItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (storageItem is not IStorageFile storageFile)
            {
                continue;
            }

            using (storageFile)
            {
                string? localPath = storageFile.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(localPath))
                {
                    selectedPaths.Add(localPath);
                    continue;
                }

                importDirectoryPath ??= CreateImportedWorkspacePath();
                string targetPath = GetUniqueImportedFilePath(importDirectoryPath, storageFile.Name, usedTargetPaths);

                await using Stream sourceStream = await storageFile.OpenReadAsync().ConfigureAwait(false);
                await using FileStream targetStream = File.Create(targetPath);
                await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
                selectedPaths.Add(targetPath);
            }
        }

        return selectedPaths;
    }

    private static string GetUniqueImportedFilePath(
        string importDirectoryPath,
        string fileName,
        ISet<string> usedTargetPaths)
    {
        string sanitizedFileName = string.IsNullOrWhiteSpace(fileName)
            ? "imported-file"
            : Path.GetFileName(fileName);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sanitizedFileName);
        string fileExtension = Path.GetExtension(sanitizedFileName);
        string targetPath = Path.Combine(importDirectoryPath, sanitizedFileName);
        int suffix = 1;

        while (!usedTargetPaths.Add(targetPath))
        {
            targetPath = Path.Combine(
                importDirectoryPath,
                $"{fileNameWithoutExtension}-{suffix}{fileExtension}");
            suffix++;
        }

        return targetPath;
    }

    private bool IsInteractiveTitleBarSource(object? source)
    {
        for (Visual? visual = source as Visual;
            visual is not null && !ReferenceEquals(visual, _titleBarBorder);
            visual = visual.GetVisualParent())
        {
            if (visual is Button)
            {
                return true;
            }

            WindowDecorationsElementRole role = WindowDecorationProperties.GetElementRole(visual);
            if (role is WindowDecorationsElementRole.User or
                WindowDecorationsElementRole.MinimizeButton or
                WindowDecorationsElementRole.MaximizeButton or
                WindowDecorationsElementRole.CloseButton)
            {
                return true;
            }
        }

        return false;
    }

    private Window? GetCustomChromeWindow()
    {
        Window? window = TopLevel.GetTopLevel(this) as Window;
        return window is not null &&
            DataContext is MainWindowViewModel { Settings.UseCustomTitleBar: true }
            ? window
            : null;
    }
}
