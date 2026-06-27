using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Tests;

internal sealed class TestViewerSettingsService : IViewerSettingsService
{
    internal TestViewerSettingsService(ViewerSettings currentSettings)
    {
        CurrentSettings = currentSettings;
    }

    public event EventHandler? SettingsChanged;

    public ViewerSettings CurrentSettings { get; private set; }

    internal int InitializeCount { get; private set; }

    internal int ResetCount { get; private set; }

    internal int SaveCount { get; private set; }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        InitializeCount++;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task ResetAsync(CancellationToken cancellationToken)
    {
        ResetCount++;
        return SaveAsync(new ViewerSettings(), cancellationToken);
    }

    public Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken)
    {
        CurrentSettings = settings;
        SaveCount++;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
