using SpineViewer.Core.Abstractions;

namespace SpineViewer.App.Startup;

/// <summary>
/// Loads persisted viewer settings and applies the current theme before the shell is shown.
/// </summary>
public sealed class ApplyViewerThemeStartupTask : IApplicationStartupTask
{
    private readonly IApplicationThemeService _applicationThemeService;
    private readonly IViewerSettingsService _viewerSettingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplyViewerThemeStartupTask"/> class.
    /// </summary>
    /// <param name="viewerSettingsService">The shared viewer settings service.</param>
    /// <param name="applicationThemeService">The application theme service.</param>
    public ApplyViewerThemeStartupTask(
        IViewerSettingsService viewerSettingsService,
        IApplicationThemeService applicationThemeService)
    {
        _viewerSettingsService = viewerSettingsService ?? throw new ArgumentNullException(nameof(viewerSettingsService));
        _applicationThemeService = applicationThemeService ?? throw new ArgumentNullException(nameof(applicationThemeService));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _viewerSettingsService.InitializeAsync(cancellationToken).ConfigureAwait(false);
        _applicationThemeService.ApplyTheme(_viewerSettingsService.CurrentSettings.Theme);
    }
}
