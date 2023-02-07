using System.Reflection;
using System.Windows.Media;
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
    private readonly IStartupChecker startupChecker;
    private ModelState state = ModelState.CannotStart;

    public MainWindowViewModel(ILogger<MainWindowViewModel> theLogger,
                               IMainWindowOpenAndCloser windowCloser, IZipper theZipper,
                               IStartupChecker startupCheck)
    {
        (logger, mainWindowCloser, zipper, startupChecker) =
        (theLogger, windowCloser, theZipper, startupCheck);
        if (startupChecker.CanStart())
        {
            state = ModelState.CanStart;
        }
        UpdateState();
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
        state = startupChecker.CanStart() switch
        {
            true => ModelState.CanStart,
            false => ModelState.CannotStart,
        };

        var result = await Messenger.Send<SaveExportToPathRequestMessage>();
        if (result.Canceled)
        {
            return;
        }

        var successful = zipper.ZipIt(result.Path);
        if (!successful)
        {
            state = ModelState.ZipFailed;
            UpdateState();
        }
    }

    public string Version
    {
        get
        {
            return "Version: " + Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
        }
    }

    [ObservableProperty]
    private string? message;

    [ObservableProperty]
    private Color backgroundColor;

    private void UpdateState()
    {
        SetMessage();
        SetBackgroundColor();
    }

    private void SetMessage()
    {
        Message = state switch
        {
            ModelState.CannotStart => "Cannot Start",
            ModelState.ZipFailed => "Zip Failed",
            ModelState.CanStart => "Running",
            _ => "Unknown",
        };
    }

    private void SetBackgroundColor()
    {
        BackgroundColor = state switch
        {
            ModelState.CannotStart => Colors.Red,
            ModelState.ZipFailed => Colors.Yellow,
            ModelState.CanStart => Colors.LightGreen,
            _ => Colors.White,
        };
    }

    private enum ModelState
    {
        CannotStart,
        ZipFailed,
        CanStart,
    }
}
