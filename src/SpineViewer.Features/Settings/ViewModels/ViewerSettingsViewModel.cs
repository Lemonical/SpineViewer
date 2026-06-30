using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Settings.ViewModels;

/// <summary>
/// Presents persisted viewer preferences and saves changes automatically.
/// </summary>
public sealed partial class ViewerSettingsViewModel : ObservableObject, IDisposable
{
    private readonly IApplicationThemeService _applicationThemeService;
    private readonly ViewportBackgroundStyle[] _backgroundOptions =
        Enum.GetValues<ViewportBackgroundStyle>();
    private readonly ViewerTheme[] _themeOptions = Enum.GetValues<ViewerTheme>();
    private readonly IViewerSettingsService _viewerSettingsService;
    private bool _isSynchronizing;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettingsViewModel"/> class.
    /// </summary>
    public ViewerSettingsViewModel(
        IViewerSettingsService viewerSettingsService,
        IApplicationThemeService applicationThemeService)
    {
        _viewerSettingsService = viewerSettingsService ?? throw new ArgumentNullException(nameof(viewerSettingsService));
        _applicationThemeService = applicationThemeService ?? throw new ArgumentNullException(nameof(applicationThemeService));

        ApplySettings(_viewerSettingsService.CurrentSettings);
        _viewerSettingsService.SettingsChanged += OnSettingsChanged;
    }

    /// <summary>
    /// Gets the available theme options.
    /// </summary>
    public IReadOnlyList<ViewerTheme> ThemeOptions => _themeOptions;

    /// <summary>
    /// Gets the available viewport background options.
    /// </summary>
    public IReadOnlyList<ViewportBackgroundStyle> BackgroundOptions => _backgroundOptions;

    /// <summary>
    /// Gets the currently selected theme.
    /// </summary>
    [ObservableProperty]
    private ViewerTheme selectedTheme;

    /// <summary>
    /// Gets a value indicating whether the grid overlay starts enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showGridByDefault;

    /// <summary>
    /// Gets a value indicating whether the origin overlay starts enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showOriginByDefault;

    /// <summary>
    /// Gets a value indicating whether the bones overlay starts enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showBonesByDefault;

    /// <summary>
    /// Gets a value indicating whether the bounds overlay starts enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showBoundsByDefault;

    /// <summary>
    /// Gets a value indicating whether the mesh or wireframe overlay starts enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showMeshWireframeByDefault;

    /// <summary>
    /// Gets a value indicating whether the slot-outline overlay starts enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showSlotOutlinesByDefault;

    /// <summary>
    /// Gets a value indicating whether labels start enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showLabelsByDefault;

    /// <summary>
    /// Gets a value indicating whether missing-resource indicators start enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showMissingResourceIndicatorsByDefault;

    /// <summary>
    /// Gets a value indicating whether unsupported-feature indicators start enabled by default.
    /// </summary>
    [ObservableProperty]
    private bool showUnsupportedFeatureIndicatorsByDefault;

    /// <summary>
    /// Gets the default playback loop preference.
    /// </summary>
    [ObservableProperty]
    private bool loopPlaybackByDefault;

    /// <summary>
    /// Gets the default playback speed multiplier.
    /// </summary>
    [ObservableProperty]
    private double defaultPlaybackSpeed = 1.0;

    /// <summary>
    /// Gets the default track mix duration, in seconds.
    /// </summary>
    [ObservableProperty]
    private double defaultTrackMixDurationSeconds = 0.15;

    /// <summary>
    /// Gets the default per-track time scale.
    /// </summary>
    [ObservableProperty]
    private double defaultTrackTimeScale = 1.0;

    /// <summary>
    /// Gets the default viewport background style.
    /// </summary>
    [ObservableProperty]
    private ViewportBackgroundStyle defaultViewportBackgroundStyle;

    /// <summary>
    /// Gets the recent-files limit.
    /// </summary>
    [ObservableProperty]
    private int recentFilesLimit = 10;

    /// <summary>
    /// Gets a value indicating whether the last session restores on startup.
    /// </summary>
    [ObservableProperty]
    private bool restoreLastSessionOnStartup = true;

    /// <summary>
    /// Gets a value indicating whether the desktop shell uses the custom in-app title bar.
    /// </summary>
    [ObservableProperty]
    private bool useCustomTitleBar = true;

    /// <summary>
    /// Gets the last persisted window state, if available.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowStateSummaryText))]
    private ViewerWindowState? persistedWindowState;

    /// <summary>
    /// Gets the current settings status text.
    /// </summary>
    [ObservableProperty]
    private string statusText = "Preferences save automatically.";

    /// <summary>
    /// Releases the settings-change subscription held by this view model.
    /// </summary>
    public void Dispose()
    {
        _viewerSettingsService.SettingsChanged -= OnSettingsChanged;
    }

    /// <summary>
    /// Gets the persisted window-state summary.
    /// </summary>
    public string WindowStateSummaryText => PersistedWindowState is null
        ? "The current window size and position will be saved when the shell closes."
        : $"Last window: {PersistedWindowState.Width:0} x {PersistedWindowState.Height:0}" +
          (PersistedWindowState.IsMaximized ? " | Maximized" : string.Empty);

    partial void OnDefaultPlaybackSpeedChanged(double value)
    {
        PersistEditableSettings();
    }

    partial void OnDefaultTrackMixDurationSecondsChanged(double value)
    {
        PersistEditableSettings();
    }

    partial void OnDefaultTrackTimeScaleChanged(double value)
    {
        PersistEditableSettings();
    }

    partial void OnDefaultViewportBackgroundStyleChanged(ViewportBackgroundStyle value)
    {
        PersistEditableSettings();
    }

    partial void OnLoopPlaybackByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnRecentFilesLimitChanged(int value)
    {
        PersistEditableSettings();
    }

    partial void OnRestoreLastSessionOnStartupChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnUseCustomTitleBarChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnSelectedThemeChanged(ViewerTheme value)
    {
        PersistEditableSettings();
    }

    partial void OnShowBonesByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowBoundsByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowGridByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowLabelsByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowMeshWireframeByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowMissingResourceIndicatorsByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowOriginByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowSlotOutlinesByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    partial void OnShowUnsupportedFeatureIndicatorsByDefaultChanged(bool value)
    {
        PersistEditableSettings();
    }

    /// <summary>
    /// Persists the supplied window state into the shared settings snapshot.
    /// </summary>
    /// <param name="windowState">The window state to persist.</param>
    /// <param name="cancellationToken">A token that cancels the save operation.</param>
    /// <returns>A task that completes when the state is saved.</returns>
    public async Task PersistWindowStateAsync(
        ViewerWindowState windowState,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(windowState);

        ViewerSettings updatedSettings = _viewerSettingsService.CurrentSettings with
        {
            LastWindowState = windowState,
        };

        await _viewerSettingsService.SaveAsync(updatedSettings, cancellationToken);
        StatusText = "Window state saved.";
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        await _viewerSettingsService.ResetAsync(CancellationToken.None);
        _applicationThemeService.ApplyTheme(_viewerSettingsService.CurrentSettings.Theme);
        StatusText = "Preferences reset to defaults.";
    }

    private void ApplySettings(ViewerSettings settings)
    {
        _isSynchronizing = true;

        try
        {
            SelectedTheme = settings.Theme;
            ShowGridByDefault = settings.ShowGridByDefault;
            ShowOriginByDefault = settings.ShowOriginByDefault;
            ShowBonesByDefault = settings.ShowBonesByDefault;
            ShowBoundsByDefault = settings.ShowBoundsByDefault;
            ShowMeshWireframeByDefault = settings.ShowMeshWireframeByDefault;
            ShowSlotOutlinesByDefault = settings.ShowSlotOutlinesByDefault;
            ShowLabelsByDefault = settings.ShowLabelsByDefault;
            ShowMissingResourceIndicatorsByDefault = settings.ShowMissingResourceIndicatorsByDefault;
            ShowUnsupportedFeatureIndicatorsByDefault = settings.ShowUnsupportedFeatureIndicatorsByDefault;
            LoopPlaybackByDefault = settings.LoopPlaybackByDefault;
            DefaultPlaybackSpeed = settings.DefaultPlaybackSpeed;
            DefaultTrackMixDurationSeconds = settings.DefaultTrackMixDuration.TotalSeconds;
            DefaultTrackTimeScale = settings.DefaultTrackTimeScale;
            DefaultViewportBackgroundStyle = settings.DefaultViewportBackgroundStyle;
            RecentFilesLimit = settings.RecentFilesLimit;
            RestoreLastSessionOnStartup = settings.RestoreLastSessionOnStartup;
            UseCustomTitleBar = settings.UseCustomTitleBar;
            PersistedWindowState = settings.LastWindowState;
        }
        finally
        {
            _isSynchronizing = false;
        }
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ApplySettings(_viewerSettingsService.CurrentSettings);
    }

    private void PersistEditableSettings()
    {
        if (_isSynchronizing)
        {
            return;
        }

        if (DefaultPlaybackSpeed <= 0 ||
            DefaultTrackMixDurationSeconds < 0 ||
            DefaultTrackTimeScale <= 0 ||
            RecentFilesLimit <= 0)
        {
            StatusText = "Preference values must stay within their valid ranges.";
            return;
        }

        ViewerSettings currentSettings = _viewerSettingsService.CurrentSettings;
        ViewerSettings updatedSettings = currentSettings with
        {
            Theme = SelectedTheme,
            ShowGridByDefault = ShowGridByDefault,
            ShowOriginByDefault = ShowOriginByDefault,
            ShowBonesByDefault = ShowBonesByDefault,
            ShowBoundsByDefault = ShowBoundsByDefault,
            ShowMeshWireframeByDefault = ShowMeshWireframeByDefault,
            ShowSlotOutlinesByDefault = ShowSlotOutlinesByDefault,
            ShowLabelsByDefault = ShowLabelsByDefault,
            ShowMissingResourceIndicatorsByDefault = ShowMissingResourceIndicatorsByDefault,
            ShowUnsupportedFeatureIndicatorsByDefault = ShowUnsupportedFeatureIndicatorsByDefault,
            LoopPlaybackByDefault = LoopPlaybackByDefault,
            DefaultPlaybackSpeed = DefaultPlaybackSpeed,
            DefaultTrackMixDuration = TimeSpan.FromSeconds(DefaultTrackMixDurationSeconds),
            DefaultTrackTimeScale = DefaultTrackTimeScale,
            DefaultViewportBackgroundStyle = DefaultViewportBackgroundStyle,
            RecentFilesLimit = RecentFilesLimit,
            RestoreLastSessionOnStartup = RestoreLastSessionOnStartup,
            UseCustomTitleBar = UseCustomTitleBar,
        };

        _ = SaveEditableSettingsAsync(updatedSettings);
    }

    private async Task SaveEditableSettingsAsync(ViewerSettings updatedSettings)
    {
        try
        {
            await _viewerSettingsService.SaveAsync(updatedSettings, CancellationToken.None);
            _applicationThemeService.ApplyTheme(updatedSettings.Theme);
            StatusText = "Preferences saved.";
        }
        catch (Exception exception)
        {
            StatusText = $"Couldn't save preferences: {exception.Message}";
        }
    }
}
