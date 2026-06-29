using Avalonia;
using SpineViewer.App;

namespace SpineViewer.Desktop;

/// <summary>
/// Provides the desktop process entrypoint for the application host.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Starts the desktop shell or runs the non-UI smoke test path.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the process.</param>
    /// <returns>A process exit code.</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Any(static argument => string.Equals(argument, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            return SmokeTestRunner.RunAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        using AppBootstrapper bootstrapper = AppBootstrapper.Initialize();
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Builds the Avalonia application host.
    /// </summary>
    /// <returns>The configured Avalonia application builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<SpineViewer.App.App>()
            .UsePlatformDetect()
            .WithInterFont();
    }
}
