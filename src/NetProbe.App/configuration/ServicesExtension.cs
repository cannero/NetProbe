using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetProbe.Core.Interfaces;
using NetProbe.Core.Services;
using NetProbe.Core.ValueObjects;
using NetProbe.Infra.Dummys;
using NetProbe.Infra.IO;
using Serilog;

namespace NetProbe.App.Configuration;

public static class ServicesExtension
{
    public static IServiceCollection AddNetProbe(this IServiceCollection services, string logpath)
    {
        return services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton<NotifyIconViewModel>()
                .AddSingleton<IMainWindowOpenAndCloser>(s => s.GetService<NotifyIconViewModel>()!)
                .AddSingleton(new RegistryConfiguration{
                                  Key = "not existing key",
                                  ValueName = "not existing value",
                                  FallbackPath = "C:/tmp/netprobeconfig.xml",})
                .AddSingleton<IRegistryReader, RegistryReader>()
                .AddSingleton<IProbeConfigurationProvider, RegistryConfigurationProvider>()
                .AddSingleton<IAvailabilityService, AvailabilityService>()
                .AddSingleton<IProbe, PingProbe>()
                .AddSingleton<IProbe, MySqlConnectionProbe>()
                .AddTransient<IStartupChecker, StartupChecker>()
                .AddTransient<IZipper>(p => new Zipper(p.GetService<ILogger<Zipper>>(), logpath))
                //.AddTransient<IZipper, FailingZipper>()
                .AddTransient<MainWindowViewModel>();
    }
}
