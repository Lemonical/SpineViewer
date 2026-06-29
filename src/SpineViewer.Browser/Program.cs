using Avalonia;
using Avalonia.Browser;
using SpineViewer.App;

namespace SpineViewer.Browser;

/// <summary>
/// Provides the browser process entrypoint for the application host.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Starts the browser single-view shell.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the browser host.</param>
    /// <returns>A task that completes when the browser host shuts down.</returns>
    public static Task Main(string[] args)
    {
        AppBootstrapper.Initialize();
        return BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    /// <summary>
    /// Builds the Avalonia application host.
    /// </summary>
    /// <returns>The configured Avalonia application builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<SpineViewer.App.App>();
    }
}
