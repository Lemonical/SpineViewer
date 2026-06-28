using SpineViewer.Core.Models;
using SpineViewer.Features.Settings.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class ViewerSettingsViewModelTests
{
    [Fact]
    public async Task EditingProperties_SavesSettingsAndAppliesThemeAsync()
    {
        TestViewerSettingsService viewerSettingsService = new(new ViewerSettings());
        TestApplicationThemeService applicationThemeService = new();
        ViewerSettingsViewModel viewModel = new(viewerSettingsService, applicationThemeService);

        viewModel.SelectedTheme = ViewerTheme.Dark;
        viewModel.ShowMeshWireframeByDefault = true;
        viewModel.DefaultTrackMixDurationSeconds = 0.4;

        await WaitForConditionAsync(static service => service.SaveCount >= 3, viewerSettingsService);

        Assert.Equal(ViewerTheme.Dark, viewerSettingsService.CurrentSettings.Theme);
        Assert.True(viewerSettingsService.CurrentSettings.ShowMeshWireframeByDefault);
        Assert.Equal(TimeSpan.FromSeconds(0.4), viewerSettingsService.CurrentSettings.DefaultTrackMixDuration);
        Assert.Equal(ViewerTheme.Dark, applicationThemeService.AppliedThemes.Last());
    }

    [Fact]
    public async Task ResetSettingsCommand_ResetsDefaultsAndAppliesDefaultThemeAsync()
    {
        TestViewerSettingsService viewerSettingsService = new(
            new ViewerSettings
            {
                Theme = ViewerTheme.Dark,
                ShowLabelsByDefault = true,
            });
        TestApplicationThemeService applicationThemeService = new();
        ViewerSettingsViewModel viewModel = new(viewerSettingsService, applicationThemeService);

        await viewModel.ResetSettingsCommand.ExecuteAsync(null);

        Assert.Equal(1, viewerSettingsService.ResetCount);
        Assert.Equal(ViewerTheme.FollowSystem, viewerSettingsService.CurrentSettings.Theme);
        Assert.False(viewerSettingsService.CurrentSettings.ShowLabelsByDefault);
        Assert.Equal(ViewerTheme.FollowSystem, applicationThemeService.AppliedThemes.Last());
    }

    [Fact]
    public async Task PersistWindowStateAsync_SavesWindowPlacementAsync()
    {
        TestViewerSettingsService viewerSettingsService = new(new ViewerSettings());
        ViewerSettingsViewModel viewModel = new(viewerSettingsService, new TestApplicationThemeService());
        ViewerWindowState windowState = new(1280, 720, 100, 120, true);

        await viewModel.PersistWindowStateAsync(windowState, CancellationToken.None);

        Assert.Equal(windowState, viewerSettingsService.CurrentSettings.LastWindowState);
        Assert.Contains("1280 x 720", viewModel.WindowStateSummaryText);
    }

    private static async Task WaitForConditionAsync<TState>(
        Func<TState, bool> predicate,
        TState state)
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (predicate(state))
            {
                return;
            }

            await Task.Delay(10);
        }

        Assert.True(predicate(state), "The expected asynchronous condition was not met.");
    }
}
