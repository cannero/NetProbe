using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NetProbe.App.Messages;

namespace NetProbe.App;

public partial class MainWindow : Window, IRecipient<SaveExportToPathRequestMessage>
{
    private readonly ILogger<MainWindow> logger;

    public MainWindow()
    {
        InitializeComponent();

        logger = Ioc.Default.GetRequiredService<ILogger<MainWindow>>();
        DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>();
        WeakReferenceMessenger.Default.Register<SaveExportToPathRequestMessage>(this);
    }

    public void Receive(SaveExportToPathRequestMessage message)
    {
        logger.LogInformation("message received");
        SaveFileDialog saveFileDialog = new(){
            Filter = message.Filter,
            FileName = message.PreAssignedFileName,
            InitialDirectory = Environment.GetFolderPath(
                 Environment.SpecialFolder.MyDocuments),
            RestoreDirectory = true, // not implemented
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            message.Reply(Task.FromResult(
                new SaveExportResult(saveFileDialog.FileName)));
        }
        else
        {
            logger.LogInformation("Export canceled by user");
            message.Reply(Task.FromResult(
                new SaveExportResult()));
        }
    }
}
