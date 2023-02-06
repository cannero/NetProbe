using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using NetProbe.App.Messages;
using NetProbe.Core.Interfaces;

namespace NetProbe.App;

public partial class MainWindowViewModel : ObservableRecipient
{
    private readonly ILogger<MainWindowViewModel> logger;
    private readonly IMainWindowOpenAndCloser mainWindowCloser;
    private readonly IZipper zipper;
    private readonly bool canStart = false;

    public MainWindowViewModel(ILogger<MainWindowViewModel> theLogger,
                               IMainWindowOpenAndCloser windowCloser, IZipper theZipper,
                               IStartupChecker startupChecker)
    {
        (logger, mainWindowCloser, zipper) = (theLogger, windowCloser, theZipper);
        if (startupChecker.CanStart())
        {
            canStart = true;
            Message = "Running";
        }
        else
        {
            Message = "Cannot start";
        }
    }

    [RelayCommand]
    public void HideWindow()
    {
        mainWindowCloser.HideWindow();
    }

    [RelayCommand]
    public void WindowClosed()
    {
        mainWindowCloser.WindowClosed();
    }

    [RelayCommand]
    public void ExitApplication()
    {
        Messenger.Send<ExitAppMessage>();
    }

    [RelayCommand]
    public async void ExportAllLogs()
    {
        Message = canStart switch
        {
            true => "Running",
            false => "Cannot Start",
        };

        var result = await Messenger.Send<SaveExportToPathRequestMessage>();
        if (result.Canceled)
        {
            return;
        }

        var successful = zipper.ZipIt(result.Path);
        if (!successful)
        {
            Message = "Data export failed";
        }
    }

    [ObservableProperty]
    private string? message;
}
