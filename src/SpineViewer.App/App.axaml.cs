using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SpineViewer.Core.Abstractions;
using SpineViewer.Features.Shell.Views;

namespace SpineViewer.App;

/// <summary>
/// Initializes the Avalonia application and hands shell creation to the composition root.
/// </summary>
public partial class App : Application
{
    private IServiceScope? _serviceScope;

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _serviceScope = AppBootstrapper.Current.Services.CreateScope();
            desktop.Exit += OnDesktopExit;

            IApplicationStartupPipeline startupPipeline =
                _serviceScope.ServiceProvider.GetRequiredService<IApplicationStartupPipeline>();

            startupPipeline.RunAsync(CancellationToken.None).GetAwaiter().GetResult();
            desktop.MainWindow = _serviceScope.ServiceProvider.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _serviceScope?.Dispose();
        _serviceScope = null;
    }
}
