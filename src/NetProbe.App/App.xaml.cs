using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetProbe.App.Messages;
using NetProbe.Core.Interfaces;
using NetProbe.Core.Services;
using NetProbe.Core.ValueObjects;
using NetProbe.Infra.Dummys;
using NetProbe.Infra.IO;
using Serilog;

namespace NetProbe.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IRecipient<OpenWindowMessage>,
 IRecipient<HideWindowMessage>, IRecipient<ExitAppMessage>
{
    private Mutex appMutex = new Mutex(true, "NetProbe99887766");
    private bool appAlreadyRunning = true;
    private TaskbarIcon? notifyIcon;
    private const string logpath = "logs/";

    public App()
    {
        if (appMutex.WaitOne(TimeSpan.Zero, true))
        {
            appAlreadyRunning = false;
        }

        SetupSerilog();

        this.DispatcherUnhandledException += App_DispatcherUnhandledException;

        if (appAlreadyRunning)
        {
            Log.Information("app already running, exiting");
            Environment.Exit(0);
        }
    }

    private void SetupSerilog()
    {
        var logfileName = appAlreadyRunning switch
        {
            false => "NetProbe.log",
            true => "NetProbeAlreadyRunning.log",
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File($"{logpath}{logfileName}", rollingInterval: RollingInterval.Day)
            .CreateLogger();
#if DEBUG
        Log.Information("==============Starting Debug==============");
#else
        Log.Information("==============Starting Release============");
#endif
        Log.Information($"{System.Reflection.Assembly.GetExecutingAssembly().Location}");
        Log.Information($"{System.IO.Path.GetFullPath(".")}");
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal("Unhandled: {exception}", e.Exception);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (appAlreadyRunning)
        {
            return;
        }

        base.OnStartup(e);

        SetupIoc();

        WeakReferenceMessenger.Default.Register<OpenWindowMessage>(this);
        WeakReferenceMessenger.Default.Register<HideWindowMessage>(this);
        WeakReferenceMessenger.Default.Register<ExitAppMessage>(this);

        //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
        notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
        notifyIcon.DataContext = Ioc.Default.GetRequiredService<NotifyIconViewModel>();
        notifyIcon.ForceCreate();

        StartProbesIfPossible();

        // only open in case of error
        OpenMainWindow();
    }

    private void SetupIoc()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
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
                .AddTransient<MainWindowViewModel>()
                .BuildServiceProvider(validateScopes: true));
    }

    private void StartProbesIfPossible()
    {
        var startupChecker = Ioc.Default.GetRequiredService<IStartupChecker>();
        if (startupChecker.CanStart())
        {
            Log.Information("starting");
            var availService = Ioc.Default.GetRequiredService<IAvailabilityService>();
            foreach (var probe in Ioc.Default.GetServices<IProbe>())
            {
                availService.AddProbe(probe);
            }
            availService.Start();
        }
        else
        {
            Log.Fatal("startup not possible");
            //MessageBox.Show("Cannot start", "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        CloseResources();
        base.OnExit(e);
    }

    private void CloseResources()
    {
        if (appAlreadyRunning)
        {
            return;
        }

        notifyIcon?.Dispose();
        appMutex.ReleaseMutex();

        var availService = Ioc.Default.GetRequiredService<IAvailabilityService>();
        availService.Stop();
    }

    private void OpenMainWindow()
    {
        MainWindow ??= new MainWindow();
        MainWindow.Show(disableEfficiencyMode: true);
    }

    public void Receive(OpenWindowMessage message)
    {
        Log.Debug("open window");
        OpenMainWindow();
    }

    public void Receive(HideWindowMessage message)
    {
        Log.Debug("hide window");
        MainWindow?.Hide(enableEfficiencyMode: true);
    }

    public void Receive(ExitAppMessage message)
    {
        ExitApp();
    }

    private void ExitApp()
    {
        Log.Debug("exit");
        Shutdown();
    }
}
