using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpineViewer.Core.Abstractions;
using SpineViewer.Core.Services;
using SpineViewer.Features.Shell.ViewModels;
using SpineViewer.Features.Viewport.Contracts;
using SpineViewer.Features.Viewport.Services;
using SpineViewer.Features.Viewport.ViewModels;
using SpineViewer.Features.Workspace.Services;
using SpineViewer.Features.Shell.Views;
using SpineViewer.Infrastructure.Diagnostics;
using SpineViewer.Infrastructure.Persistence;
using SpineViewer.Infrastructure.Spine.Adapters;
using SpineViewer.Infrastructure.Spine.Loading;
using SpineViewer.Infrastructure.Spine.Sessions;
using SpineViewer.Infrastructure.Startup;
using SpineViewer.App.Startup;

namespace SpineViewer.App;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpineViewerApplication(this IServiceCollection services)
    {
        string dataDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SpineViewer");
        string settingsPath = Path.Combine(dataDirectoryPath, "settings.json");
        string recentFilesPath = Path.Combine(dataDirectoryPath, "recent-files.json");

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
        services.AddSingleton<IApplicationStartupTask, RestoreLastSessionStartupTask>();
        services.AddSingleton<ISettingsRepository>(_ => new JsonSettingsRepository(settingsPath));
        services.AddSingleton<IRecentFilesService>(
            serviceProvider => new RecentFilesService(
                recentFilesPath,
                serviceProvider.GetRequiredService<ISettingsRepository>()));
        services.AddSingleton<ISpineRuntimeAdapter, Spine38RuntimeAdapter>();
        services.AddSingleton<ISpineRuntimeAdapter, Spine41RuntimeAdapter>();
        services.AddSingleton<ISpineRuntimeCatalog, SpineRuntimeCatalog>();
        services.AddSingleton<ISpineProjectReferenceResolver, SpineProjectReferenceResolver>();
        services.AddSingleton<IVersionDetectionService, VersionDetectionService>();
        services.AddSingleton<IRuntimeSelectionService, RuntimeSelectionService>();
        services.AddSingleton<ISpineProjectLoader, SpineProjectLoader>();
        services.AddSingleton<ISpineSessionFactory, SpineSessionFactory>();
        services.AddSingleton<IWorkspaceSessionService, WorkspaceSessionService>();
        services.AddSingleton<IRenderInvalidationService, RenderInvalidationService>();
        services.AddSingleton<IViewportFrameScheduler, ViewportFrameScheduler>();
        services.AddSingleton<IViewportCameraService, ViewportCameraService>();
        services.AddSingleton<IViewportOverlaySource, GridOverlaySource>();
        services.AddSingleton<IViewportOverlaySource, SessionPlaceholderOverlaySource>();
        services.AddSingleton<IViewportOverlaySource, BoneOverlaySource>();
        services.AddSingleton<IViewportOverlaySource, BoundsOverlaySource>();
        services.AddSingleton<IViewportOverlaySource, OriginOverlaySource>();
        services.AddSingleton<IViewportSceneComposer, ViewportSceneComposer>();

        services.AddTransient<ViewportViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        return services;
    }
}
