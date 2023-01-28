using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using H.NotifyIcon;
using NetProbe.Core.Interfaces;
using NetProbe.Core.Services;
using Serilog;

namespace NetProbe.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IRecipient<OpenWindowMessage>,
 IRecipient<HideWindowMessage>, IRecipient<ExitAppMessage>
{
    private TaskbarIcon? notifyIcon;

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/NetProbe.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information("Starting");
        Log.Information($"{System.Reflection.Assembly.GetExecutingAssembly().Location}");
        Log.Information($"{System.IO.Path.GetFullPath(".")}");

        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal("Unhandled: {exception}", e.Exception);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SetupIoc();

        ExitIfStartupNotPossible();

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
                .AddSingleton<NotifyIconViewModel>() //Services
                .AddTransient<IStartupChecker, StartupChecker>()
        .BuildServiceProvider());
    }

    private void ExitIfStartupNotPossible()
    {
        var startupChecker = Ioc.Default.GetRequiredService<IStartupChecker>();
        if (startupChecker.CanStart)
        {
            Log.Information("starting");
        }
        else
        {
            Log.Fatal("startup not possible");
            MessageBox.Show("Cannot start", "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            ExitApp();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        notifyIcon?.Dispose();
        base.OnExit(e);
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
