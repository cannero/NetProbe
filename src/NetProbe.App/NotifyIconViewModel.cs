using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using H.NotifyIcon;
using Microsoft.Extensions.Logging;
using NetProbe.App.Messages;

namespace NetProbe.App;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon and
/// the shown/hidden state for the MainWindow.
/// </summary>
public partial class NotifyIconViewModel : ObservableRecipient, IMainWindowOpenAndCloser
{
    private readonly ILogger<NotifyIconViewModel> logger;

    public NotifyIconViewModel(ILogger<NotifyIconViewModel> thelogger)
    {
        logger = thelogger;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowWindowCommand))]
    public bool canExecuteShowWindow = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(HideWindowCommand))]
    public bool canExecuteHideWindow = true;

    /// <summary>
    /// Shows a window, if none is already open.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteShowWindow))]
    public void ShowWindow()
    {
        Messenger.Send<OpenWindowMessage>();
        CanExecuteShowWindow = false;
        CanExecuteHideWindow = true;
    }

    /// <summary>
    /// Hides the main window. This command is only enabled if a window is open.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteHideWindow))]
    public void HideWindow()
    {
        logger.LogDebug("hiding");
        Messenger.Send<HideWindowMessage>();
        WindowClosed();
    }

    /// <summary>
    /// Use this command for the Close Button.
    /// The function updates the state but does not send a message to close the window.
    /// </summary>
    [RelayCommand]
    public void WindowClosed()
    {
        logger.LogDebug("hidden");
        CanExecuteShowWindow = true;
        CanExecuteHideWindow = false;
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    [RelayCommand]
    public void ExitApplication()
    {
        Messenger.Send<ExitAppMessage>();
    }
}
