namespace Tentakel.Extensions.Logging.File;

public class FileChangedEventArgs
{
    public FileChangedEventArgs(string? oldFileName, string newFileName)
    {
        this.OldFileName = oldFileName;
        this.NewFileName = newFileName;
    }
    public string? OldFileName { get; }
    public string NewFileName { get; }
}