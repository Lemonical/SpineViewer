using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Optris.Icons.Avalonia;
using Optris.Icons.Avalonia.FontAwesome;
using SpineViewer.Core.Abstractions;
using SpineViewer.Features.Shell.Views;

namespace SpineViewer.App;

/// <summary>
/// Initializes the Avalonia application and hands shell creation to the composition root.
/// </summary>
public partial class App : Application
{
    private static bool _fontAwesomeRegistered;
    private IServiceScope? _serviceScope;
    private CancellationTokenSource? _startupCancellationTokenSource;

    /// <inheritdoc />
    public override void Initialize()
    {
        EnsureIconProviders();
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _serviceScope = CreateServiceScope();
            _startupCancellationTokenSource = new CancellationTokenSource();
            desktop.Exit += OnDesktopExit;
            desktop.MainWindow = _serviceScope.ServiceProvider.GetRequiredService<MainWindow>();
            StartStartupPipeline(_serviceScope.ServiceProvider, _startupCancellationTokenSource.Token);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            _serviceScope = CreateServiceScope();
            _startupCancellationTokenSource = new CancellationTokenSource();
            singleView.MainView = _serviceScope.ServiceProvider.GetRequiredService<MainView>();
            StartStartupPipeline(_serviceScope.ServiceProvider, _startupCancellationTokenSource.Token);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceScope CreateServiceScope()
    {
        IServiceScope serviceScope = AppBootstrapper.Current.Services.CreateScope();
        return serviceScope;
    }

    private void StartStartupPipeline(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        _ = RunStartupPipelineAsync(serviceProvider, cancellationToken);
    }

    private static async Task RunStartupPipelineAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        IApplicationStartupPipeline startupPipeline =
            serviceProvider.GetRequiredService<IApplicationStartupPipeline>();
        IAppLogger<App> logger = serviceProvider.GetRequiredService<IAppLogger<App>>();

        try
        {
            await startupPipeline.RunAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError("Application startup pipeline failed.", exception);
        }
    }

    private static void EnsureIconProviders()
    {
        if (_fontAwesomeRegistered)
        {
            return;
        }

        IconProvider.Current.Register<FontAwesomeIconProvider>();
        _fontAwesomeRegistered = true;
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _startupCancellationTokenSource?.Cancel();
        _startupCancellationTokenSource?.Dispose();
        _startupCancellationTokenSource = null;
        _serviceScope?.Dispose();
        _serviceScope = null;
    }
}
