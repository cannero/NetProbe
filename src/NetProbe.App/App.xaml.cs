using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetProbe.App.Configuration;
using NetProbe.App.Messages;
using NetProbe.Core.Interfaces;
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

        AppSettingsExtension.SetupSerilog(appAlreadyRunning, logpath);
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        WriteStart();

        SetupIoc();

        var piper = Ioc.Default.GetRequiredService<IPiper>();
        if (appAlreadyRunning)
        {
            piper.SendAlreadyRunning();
            Log.Information("app already running, exiting");
            Environment.Exit(0);
        }
        else
        {
            piper.Start(() => ShowNotification("Already running, find it in the taskbar"));
        }
    }

    private void WriteStart()
    {
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

        WeakReferenceMessenger.Default.Register<OpenWindowMessage>(this);
        WeakReferenceMessenger.Default.Register<HideWindowMessage>(this);
        WeakReferenceMessenger.Default.Register<ExitAppMessage>(this);

        //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
        notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
        notifyIcon.DataContext = Ioc.Default.GetRequiredService<NotifyIconViewModel>();
        notifyIcon.ForceCreate();

        if (EverythingOkStartingProbes())
        {
            ShowNotification("The appliation is running in the background");
        }
        // only open in case of error
        OpenMainWindow();
    }

    private void SetupIoc()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddConfiguration()
                .AddNetProbe(logpath)
                .BuildServiceProvider(validateScopes: true));
    }

    private bool EverythingOkStartingProbes()
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
            return true;
        }
        else
        {
            Log.Fatal("startup not possible");
            return false;
        }
    }

    private void ShowNotification(string message)
    {
        if (notifyIcon == null)
        {
            // when already running message is received during app startup
            return;
        }

        notifyIcon.ShowNotification("NetProbe", message,
                NotificationIcon.None, customIcon: null, largeIcon: false, sound: false,
                respectQuietTime: false, realtime: true, TimeSpan.FromSeconds(2));
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

        var piper = Ioc.Default.GetRequiredService<IPiper>();
        piper.Stop();
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
