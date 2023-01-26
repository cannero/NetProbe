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
        .BuildServiceProvider());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
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
        Log.Debug("exit");
        Shutdown();
    }
}

