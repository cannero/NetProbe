using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetProbe.Core.ValueObjects;
using Serilog;

namespace NetProbe.App.Configuration;

public static class AppSettingsExtension
{
    private static IConfiguration CreateConfig()
    {
        var startpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? "./";

        var builder = new ConfigurationBuilder()
         .SetBasePath(startpath)
         .AddJsonFile("configuration/appsettings.json", optional: false);

        return builder.Build();
    }

    public static ILogger SetupSerilog(bool appAlreadyRunning, string logpath)
    {
        if (appAlreadyRunning)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File($"{logpath}NetProbeAlreadyRunning.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
            return Log.Logger;
        }

        try
        {
            var config = CreateConfig();

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

            return Log.Logger;
        }
        catch (Exception ex)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File($"{logpath}NetProbe.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            Log.Fatal(ex, "logger could not be created");
            throw;
        }
    }

    public static IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        var config = CreateConfig();

        services.AddSingleton<RegistryConfiguration>(config.GetSection("RegistrySettings")
                                                     .Get<RegistryConfiguration>() ?? new());
        return services;
    }
}
