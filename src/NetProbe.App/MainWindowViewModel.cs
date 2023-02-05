using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using NetProbe.App.Messages;

namespace NetProbe.App;

public partial class MainWindowViewModel : ObservableRecipient
{
    private readonly ILogger<MainWindowViewModel> logger;
    private readonly IMainWindowOpenAndCloser mainWindowCloser;

    public MainWindowViewModel()
    {
        logger = Ioc.Default.GetRequiredService<ILogger<MainWindowViewModel>>();
        mainWindowCloser = Ioc.Default.GetRequiredService<IMainWindowOpenAndCloser>();
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
}
