using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Tentakel.Extensions.Logging.File;
using IOFile = System.IO.File;

namespace Tentakel.Extensions.Logging.ZipFile;

public class ZipFileSink : FileSink
{
    private int _compressionLevel = Deflater.DEFAULT_COMPRESSION;
    public int CompressionLevel
    {
        get => this._compressionLevel;
        set
        {
            if (value is < Deflater.NO_COMPRESSION or > Deflater.BEST_COMPRESSION)
            {
                value = Deflater.DEFAULT_COMPRESSION;
            }

            this._compressionLevel = value;
        }
    }

    #region protected virtual

    protected override void FileNameChanged(string? oldFileName, string newFileName)
    {
        if (oldFileName == null || !IOFile.Exists(oldFileName)) return;

        var fileExtension = Path.GetExtension(oldFileName);

        if (string.Equals(fileExtension, ".zip", StringComparison.InvariantCultureIgnoreCase)) return;

        var zipFileName = string.Concat(oldFileName[..^fileExtension.Length], ".zip");
        var logFiles = this.FindExistsPartialFileNames(oldFileName).ToList();

        logFiles.Add(oldFileName);

        CompressFiles(this.CompressionLevel, zipFileName, logFiles);

        foreach (var logFile in logFiles.Where(IOFile.Exists))
        {
            IOFile.Delete(logFile);
        }
    }

    #endregion

    #region private methods

    private static void CompressFiles(int compressionLevel, string zipFileName, IEnumerable<string> logFiles)
    {
        var fileStream = new FileStream(zipFileName, FileMode.Create);
        var zipOutputStream = new ZipOutputStream(fileStream);
        var buffer = new byte[4097];

        zipOutputStream.SetLevel(compressionLevel);

        foreach (var logFile in logFiles)
        {
            var zipEntry = new ZipEntry(Path.GetFileName(logFile))
            {
                DateTime = DateTime.Now
            };

            zipOutputStream.PutNextEntry(zipEntry);

            var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            int byteLen;

            do
            {
                byteLen = fs.Read(buffer, 0, buffer.Length);
                zipOutputStream.Write(buffer, 0, byteLen);
            }
            while (!(byteLen <= 0));

            fs.Close();
        }

        zipOutputStream.Finish();
        zipOutputStream.Close();
        fileStream.Close();
    }

    #endregion
}