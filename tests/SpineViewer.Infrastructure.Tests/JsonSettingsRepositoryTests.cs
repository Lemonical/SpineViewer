using SpineViewer.Core.Models;
using SpineViewer.Infrastructure.Persistence;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class JsonSettingsRepositoryTests
{
    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTripsLastSessionStateAsync()
    {
        string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string settingsPath = Path.Combine(tempDirectoryPath, "settings.json");

        try
        {
            JsonSettingsRepository repository = new(settingsPath);
            ViewerSettings expectedSettings = new(
                ViewerTheme.Dark,
                false,
                false,
                true,
                true,
                false,
                1.25,
                8,
                true,
                new SpineProjectReference("Hero", "hero.json", "hero.atlas"));

            await repository.SaveAsync(expectedSettings, CancellationToken.None);
            ViewerSettings loadedSettings = await repository.LoadAsync(CancellationToken.None);

            Assert.Equal(expectedSettings, loadedSettings);
        }
        finally
        {
            if (Directory.Exists(tempDirectoryPath))
            {
                Directory.Delete(tempDirectoryPath, true);
            }
        }
    }

    [Fact]
    public async Task LoadAsync_WhenJsonIsInvalid_ReturnsDefaultsAsync()
    {
        string tempDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string settingsPath = Path.Combine(tempDirectoryPath, "settings.json");
        Directory.CreateDirectory(tempDirectoryPath);
        await File.WriteAllTextAsync(settingsPath, "{ invalid json", CancellationToken.None);

        try
        {
            JsonSettingsRepository repository = new(settingsPath);

            ViewerSettings loadedSettings = await repository.LoadAsync(CancellationToken.None);

            Assert.Equal(new ViewerSettings(), loadedSettings);
        }
        finally
        {
            if (Directory.Exists(tempDirectoryPath))
            {
                Directory.Delete(tempDirectoryPath, true);
            }
        }
    }
}
