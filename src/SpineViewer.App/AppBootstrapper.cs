using Microsoft.Extensions.DependencyInjection;

namespace SpineViewer.App;

/// <summary>
/// Owns the application's root service provider for the lifetime of the process.
/// </summary>
public sealed class AppBootstrapper : IDisposable
{
    private static AppBootstrapper? _current;

    private AppBootstrapper(ServiceProvider services)
    {
        Services = services;
    }

    /// <summary>
    /// Gets the current application bootstrapper.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the bootstrapper has not been initialized.</exception>
    public static AppBootstrapper Current =>
        _current ?? throw new InvalidOperationException("The application bootstrapper has not been initialized.");

    /// <summary>
    /// Gets the root service provider for the application.
    /// </summary>
    public ServiceProvider Services { get; }

    /// <summary>
    /// Builds and stores the process-wide composition root.
    /// </summary>
    /// <returns>The initialized bootstrapper.</returns>
    public static AppBootstrapper Initialize()
    {
        if (_current is not null)
        {
            throw new InvalidOperationException("The application bootstrapper is already initialized.");
        }

        ServiceCollection services = new();
        services.AddSpineViewerApplication();

        ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });

        _current = new AppBootstrapper(provider);
        return _current;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Services.Dispose();

        if (ReferenceEquals(_current, this))
        {
            _current = null;
        }
    }
}
