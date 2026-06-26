using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Core.Services;

/// <summary>
/// Provides a shared in-memory viewer settings snapshot backed by the persisted settings repository.
/// </summary>
public sealed class ViewerSettingsService : IViewerSettingsService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ISettingsRepository _settingsRepository;
    private ViewerSettings _currentSettings = new();
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewerSettingsService"/> class.
    /// </summary>
    /// <param name="settingsRepository">The persisted settings repository.</param>
    public ViewerSettingsService(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
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

        SettingsChanged?.Invoke(this, EventArgs.Empty);
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

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public Task ResetAsync(CancellationToken cancellationToken)
    {
        return SaveAsync(new ViewerSettings(), cancellationToken);
    }
}
