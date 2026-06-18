using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Features.Shell.ViewModels;
using Xunit;

namespace SpineViewer.Features.Tests;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_ReflectsWorkspaceState()
    {
        StubWorkspaceSessionService workspaceSessionService = new(
            new WorkspaceState(
                CreateSession("Hero"),
                [new SpineProjectReference("Mage", "mage.json", "mage.atlas")],
                Array.Empty<ViewerDiagnostic>(),
                "Opened Hero.",
                false));
        MainWindowViewModel viewModel = new(workspaceSessionService);

        Assert.Equal("SpineViewer", viewModel.Title);
        Assert.Equal("Hero", viewModel.WelcomeMessage);
        Assert.Equal("Spine 4.1.00", viewModel.RuntimeSummary);
        Assert.Equal("Opened Hero.", viewModel.StatusText);
        Assert.True(viewModel.HasActiveSession);
    }

    private static SpineProjectSession CreateSession(string displayName)
    {
        return new SpineProjectSession(
            Guid.NewGuid(),
            new SpineProjectReference(displayName, $"{displayName.ToLowerInvariant()}.json", $"{displayName.ToLowerInvariant()}.atlas"),
            new SpineAssetFileSet(
                $"{displayName.ToLowerInvariant()}.json",
                $"{displayName.ToLowerInvariant()}.atlas",
                [$"{displayName.ToLowerInvariant()}.png"]),
            new SpineRuntimeDescriptor(
                "spine-4.1.00",
                "Spine 4.1.00",
                "4.1.x",
                new SpineRuntimeCapabilities([new(SpineRuntimeFeature.JsonSkeleton, true)])),
            new SpineVersionMatch("4.1.00", "spine-4.1.00", true, Array.Empty<ViewerDiagnostic>()),
            new PlaybackState(),
            new ViewportState(),
            Array.Empty<ViewerDiagnostic>());
    }

    private sealed class StubWorkspaceSessionService : IWorkspaceSessionService
    {
        public StubWorkspaceSessionService(WorkspaceState state)
        {
            State = state;
        }

        public event EventHandler? StateChanged;

        public WorkspaceState State { get; private set; }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public Task OpenAsync(string selectedPath, string? companionPath, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OpenAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task ReloadAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RestoreLastSessionAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void UpdatePlaybackState(PlaybackState playbackState)
        {
        }

        public void UpdateViewportState(ViewportState viewportState)
        {
        }
    }
}
