using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;

namespace SpineViewer.Features.Tests;

internal sealed class TestWorkspaceSessionService : IWorkspaceSessionService
{
    internal TestWorkspaceSessionService(WorkspaceState state)
    {
        State = state;
    }

    public event EventHandler? StateChanged;

    public WorkspaceState State { get; private set; }

    public bool CanRetryLastOpen { get; set; }

    internal SpineProjectReference? LastOpenedProjectReference { get; private set; }

    internal IReadOnlyList<string> LastOpenedSelectedPaths { get; private set; } = Array.Empty<string>();

    internal int ReloadCount { get; private set; }

    internal int RetryCount { get; private set; }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task ClearRecentProjectsAsync(CancellationToken cancellationToken)
    {
        UpdateState(
            State with
            {
                RecentFiles = Array.Empty<SpineProjectReference>(),
            });
        return Task.CompletedTask;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task OpenAsync(IReadOnlyList<string> selectedPaths, CancellationToken cancellationToken)
    {
        LastOpenedSelectedPaths = selectedPaths.ToArray();
        return Task.CompletedTask;
    }

    public Task OpenAsync(string selectedPath, string? companionPath, CancellationToken cancellationToken)
    {
        LastOpenedSelectedPaths = companionPath is null
            ? [selectedPath]
            : [selectedPath, companionPath];
        return Task.CompletedTask;
    }

    public Task OpenAsync(SpineProjectReference projectReference, CancellationToken cancellationToken)
    {
        LastOpenedProjectReference = projectReference;
        return Task.CompletedTask;
    }

    public Task ReloadAsync(CancellationToken cancellationToken)
    {
        ReloadCount++;
        return Task.CompletedTask;
    }

    public Task RemoveRecentProjectAsync(
        SpineProjectReference projectReference,
        CancellationToken cancellationToken)
    {
        UpdateState(
            State with
            {
                RecentFiles = State.RecentFiles
                    .Where(
                        existingReference =>
                            !string.Equals(existingReference.SkeletonPath, projectReference.SkeletonPath, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(existingReference.AtlasPath, projectReference.AtlasPath, StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
            });
        return Task.CompletedTask;
    }

    public Task RetryLastOpenAsync(CancellationToken cancellationToken)
    {
        RetryCount++;
        return Task.CompletedTask;
    }

    public Task RestoreLastSessionAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void UpdatePlaybackState(PlaybackState playbackState)
    {
        if (State.CurrentSession is null)
        {
            return;
        }

        UpdateState(
            State with
            {
                CurrentSession = State.CurrentSession with
                {
                    Playback = playbackState,
                },
            });
    }

    public void UpdateSelectedSkin(string? skinName)
    {
        if (State.CurrentSession is null)
        {
            return;
        }

        UpdateState(
            State with
            {
                CurrentSession = State.CurrentSession with
                {
                    SelectedSkinName = string.IsNullOrWhiteSpace(skinName)
                        ? null
                        : skinName,
                },
            });
    }

    public void UpdatePreferredRuntimeId(string? runtimeId)
    {
        UpdateState(
            State with
            {
                PreferredRuntimeId = string.IsNullOrWhiteSpace(runtimeId)
                    ? null
                    : runtimeId,
            });
    }

    public void UpdateViewportState(ViewportState viewportState)
    {
        if (State.CurrentSession is null)
        {
            return;
        }

        UpdateState(
            State with
            {
                CurrentSession = State.CurrentSession with
                {
                    Viewport = viewportState,
                },
            });
    }

    internal void UpdateState(WorkspaceState state)
    {
        State = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
