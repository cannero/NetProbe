using CommunityToolkit.Mvvm.Messaging.Messages;

namespace NetProbe.App.Messages;

public class SaveExportResult
{
    public SaveExportResult(string path)
    {
        Canceled = false;
        Path = path;
    }

    public SaveExportResult()
    {
        Canceled = true;
    }
    
    public bool Canceled { get; }
    public string? Path { get; }
}

public class SaveExportToPathRequestMessage : AsyncRequestMessage<SaveExportResult>
{
    public string PreAssignedFileName { get; } = "NetProbe.zip";
    public string Filter { get; } = "Zip file (*.zip)|*.zip";
}
