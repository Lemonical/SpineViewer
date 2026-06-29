using Microsoft.Extensions.DependencyInjection;
using SpineViewer.Core.Abstractions;
using SpineViewer.Features.Shell.ViewModels;

namespace SpineViewer.App;

/// <summary>
/// Executes a non-UI smoke test for the composition root.
/// </summary>
public static class SmokeTestRunner
{
    /// <summary>
    /// Runs the composition root without starting a desktop lifetime.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the smoke test.</param>
    /// <returns>An exit code describing success or failure.</returns>
    public static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        using AppBootstrapper bootstrapper = AppBootstrapper.Initialize();
        using IServiceScope scope = bootstrapper.Services.CreateScope();

        IApplicationStartupPipeline startupPipeline =
            scope.ServiceProvider.GetRequiredService<IApplicationStartupPipeline>();

        await startupPipeline.RunAsync(cancellationToken).ConfigureAwait(false);

        MainWindowViewModel viewModel = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();

        return string.IsNullOrWhiteSpace(viewModel.Title) ? 1 : 0;
    }
}
