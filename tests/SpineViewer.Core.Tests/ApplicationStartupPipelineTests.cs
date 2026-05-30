using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Services;
using Xunit;

namespace SpineViewer.Core.Tests;

public sealed class ApplicationStartupPipelineTests
{
    [Fact]
    public async Task RunAsync_ExecutesTasksInRegistrationOrderAsync()
    {
        List<string> executionOrder = [];
        IApplicationStartupTask[] startupTasks =
        [
            new CallbackStartupTask("first", executionOrder),
            new CallbackStartupTask("second", executionOrder),
        ];

        ApplicationStartupPipeline pipeline = new(
            startupTasks,
            new TestAppLogger<ApplicationStartupPipeline>());

        await pipeline.RunAsync(CancellationToken.None);

        Assert.Equal(["first", "second"], executionOrder);
    }

    private sealed class CallbackStartupTask : IApplicationStartupTask
    {
        private readonly List<string> _executionOrder;
        private readonly string _name;

        public CallbackStartupTask(string name, List<string> executionOrder)
        {
            _name = name;
            _executionOrder = executionOrder;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _executionOrder.Add(_name);
            return Task.CompletedTask;
        }
    }

    private sealed class TestAppLogger<TCategory> : IAppLogger<TCategory>
    {
        public void LogError(string message, Exception exception)
        {
        }

        public void LogInformation(string message)
        {
        }

        public void LogWarning(string message)
        {
        }
    }
}
