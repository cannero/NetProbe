using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using H.NotifyIcon;
using NetProbe.Core.Interfaces;
using NetProbe.Core.Services;
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
    private bool appMutexAquired = false;
    private TaskbarIcon? notifyIcon;

    public App()
    {
        if (appMutex.WaitOne(TimeSpan.Zero, true))
        {
            appMutexAquired = true;
        }

        SetupSerilog();

        this.DispatcherUnhandledException += App_DispatcherUnhandledException;

        if (!appMutexAquired)
        {
            Log.Information("app already running, exiting");
            Environment.Exit(0);
        }
    }

    private void SetupSerilog()
    {
        var logfileName = appMutexAquired switch
        {
            true => "NetProbe.log",
            false => "NetProbeAlreadyRunning.log",
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File($"logs/{logfileName}", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information("==============Starting==============");
        Log.Information($"{System.Reflection.Assembly.GetExecutingAssembly().Location}");
        Log.Information($"{System.IO.Path.GetFullPath(".")}");
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal("Unhandled: {exception}", e.Exception);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (!appMutexAquired)
        {
            return;
        }

        base.OnStartup(e);

        SetupIoc();

        if (ExitAsStartupNotPossible())
        {
            return;
        }

        WeakReferenceMessenger.Default.Register<OpenWindowMessage>(this);
        WeakReferenceMessenger.Default.Register<HideWindowMessage>(this);
        WeakReferenceMessenger.Default.Register<ExitAppMessage>(this);

        //create the notifyicon (it's a resource declared in NotifyIconResources.xaml
        notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
        notifyIcon.DataContext = Ioc.Default.GetRequiredService<NotifyIconViewModel>();
        notifyIcon.ForceCreate();

        // todo: check how this is done in the correct way
        Receive(new OpenWindowMessage());
    }

    private void SetupIoc()
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true))
                .AddSingleton<NotifyIconViewModel>()
                .AddTransient<IRegistryReader, RegistryReader>()
                .AddTransient<IStartupChecker, StartupChecker>()
                .BuildServiceProvider());
    }

    private bool ExitAsStartupNotPossible()
    {
        var startupChecker = Ioc.Default.GetRequiredService<IStartupChecker>();
        if (startupChecker.CanStart("not existing", "not existing", "c:/tmp/config.xml"))
        {
            Log.Information("starting");
            return false;
        }
        else
        {
            Log.Fatal("startup not possible");
            MessageBox.Show("Cannot start", "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            ExitApp();
            return true;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        CloseResources();
        base.OnExit(e);
    }

    private void CloseResources()
    {
        if (!appMutexAquired)
        {
            return;
        }

        notifyIcon?.Dispose();
        appMutex.ReleaseMutex();
    }

    public void Receive(OpenWindowMessage message)
    {
        Log.Debug("open window");
        MainWindow ??= new MainWindow();
        MainWindow.Show(disableEfficiencyMode: true);
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
