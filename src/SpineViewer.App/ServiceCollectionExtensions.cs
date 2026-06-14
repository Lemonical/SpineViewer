using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Services;
using SpineViewer.Features.Shell.ViewModels;
using SpineViewer.Features.Shell.Views;
using SpineViewer.Infrastructure.Diagnostics;
using SpineViewer.Infrastructure.Spine.Adapters;
using SpineViewer.Infrastructure.Spine.Loading;
using SpineViewer.Infrastructure.Startup;

namespace SpineViewer.App;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpineViewerApplication(this IServiceCollection services)
    {
        services.AddLogging(
            builder =>
            {
                builder.ClearProviders();
                builder.AddSimpleConsole(
                    options =>
                    {
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                    });
                builder.SetMinimumLevel(LogLevel.Information);
            });

        services.AddSingleton(typeof(IAppLogger<>), typeof(MicrosoftExtensionsAppLogger<>));
        services.AddSingleton<IApplicationStartupPipeline, ApplicationStartupPipeline>();
        services.AddSingleton<IApplicationStartupTask, LogApplicationStartupTask>();
        services.AddSingleton<ISpineRuntimeAdapter, Spine38RuntimeAdapter>();
        services.AddSingleton<ISpineRuntimeAdapter, Spine41RuntimeAdapter>();
        services.AddSingleton<ISpineRuntimeCatalog, SpineRuntimeCatalog>();
        services.AddSingleton<ISpineProjectReferenceResolver, SpineProjectReferenceResolver>();
        services.AddSingleton<IVersionDetectionService, VersionDetectionService>();
        services.AddSingleton<IRuntimeSelectionService, RuntimeSelectionService>();
        services.AddSingleton<ISpineProjectLoader, SpineProjectLoader>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        return services;
    }
}
