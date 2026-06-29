using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Models;
using SpineViewer.Core.Services;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class ViewerSettingsServiceTests
{
    [Fact]
    public async Task InitializeAsync_RaisesSettingsChangedThroughUiDispatcherAsync()
    {
        RecordingUiDispatcher uiDispatcher = new();
        ViewerSettingsService service = new(
            new StubSettingsRepository(new ViewerSettings()),
            uiDispatcher);
        bool wasRaised = false;
        bool raisedInsideDispatcher = false;

        service.SettingsChanged += (_, _) =>
        {
            wasRaised = true;
            raisedInsideDispatcher = uiDispatcher.IsInvoking;
        };

        await service.InitializeAsync(CancellationToken.None);

        Assert.True(wasRaised);
        Assert.True(raisedInsideDispatcher);
        Assert.Equal(1, uiDispatcher.InvocationCount);
    }

    private sealed class RecordingUiDispatcher : IUiDispatcher
    {
        public int InvocationCount { get; private set; }

        public bool IsInvoking { get; private set; }

        public void Invoke(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            InvocationCount++;
            IsInvoking = true;

            try
            {
                callback();
            }
            finally
            {
                IsInvoking = false;
            }
        }
    }

    private sealed class StubSettingsRepository : ISettingsRepository
    {
        private ViewerSettings _settings;

        public StubSettingsRepository(ViewerSettings settings)
        {
            _settings = settings;
        }

        public Task<ViewerSettings> LoadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_settings);
        }

        public Task SaveAsync(ViewerSettings settings, CancellationToken cancellationToken)
        {
            _settings = settings;
            return Task.CompletedTask;
        }
    }
}
