using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using H.NotifyIcon;
using Serilog;

namespace NetProbe.App;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon.
/// </summary>
public partial class NotifyIconViewModel : ObservableRecipient
{
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
        Log.Debug("hiding");
        Messenger.Send<HideWindowMessage>();
        WindowHidden();
    }

    [RelayCommand]
    public void WindowHidden()
    {
        Log.Debug("hidden");
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
