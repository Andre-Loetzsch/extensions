using Tentakel.Extensions.Logging.File;

namespace Tentakel.Extensions.Logging.ZipFile;

public class ZipFileSink : FileSink
{
    public ZipFileSink()
    {
        this.FileChanged += this.OnFileChanged;
    }

    private void OnFileChanged(object? sender, FileChangedEventArgs e)
    {

    }
}