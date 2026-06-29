using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Core.Services;

/// <summary>
/// Provides a shared in-memory viewer settings snapshot backed by the persisted settings repository.
/// </summary>
public sealed class ViewerSettingsService : IViewerSettingsService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly IUiDispatcher _uiDispatcher;
    private readonly ISettingsRepository _settingsRepository;
    private ViewerSettings _currentSettings = new();
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettingsService"/> class.
    /// </summary>
    /// <param name="settingsRepository">The persisted settings repository.</param>
    /// <param name="uiDispatcher">The UI dispatcher used to publish settings changes on the UI thread.</param>
    public ViewerSettingsService(
        ISettingsRepository settingsRepository,
        IUiDispatcher uiDispatcher)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _uiDispatcher = uiDispatcher ?? throw new ArgumentNullException(nameof(uiDispatcher));
    }

    /// <inheritdoc />
    public event EventHandler? SettingsChanged;

    /// <inheritdoc />
    public ViewerSettings CurrentSettings => _currentSettings;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_isInitialized)
            {
                return;
            }

            _currentSettings = await _settingsRepository.LoadAsync(cancellationToken).ConfigureAwait(false);
            _isInitialized = true;
        }
        finally
        {
            _gate.Release();
        }

        RaiseSettingsChanged();
    }

    /// <inheritdoc />
    public async Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _settingsRepository.SaveAsync(settings, cancellationToken).ConfigureAwait(false);
            _currentSettings = settings;
            _isInitialized = true;
        }
        finally
        {
            _gate.Release();
        }

        RaiseSettingsChanged();
    }

    /// <inheritdoc />
    public Task ResetAsync(CancellationToken cancellationToken)
    {
        return SaveAsync(new ViewerSettings(), cancellationToken);
    }

    private void RaiseSettingsChanged()
    {
        _uiDispatcher.Invoke(() => SettingsChanged?.Invoke(this, EventArgs.Empty));
    }
}
