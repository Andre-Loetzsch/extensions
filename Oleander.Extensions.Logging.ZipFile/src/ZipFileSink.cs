using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Oleander.Extensions.Logging.File;
using IOFile = System.IO.File;

namespace Oleander.Extensions.Logging.ZipFile;

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

    public bool CompressEachPartialFiles { get; set; }

    #region protected virtual

    protected override void ArchiveFileCreated(string fileName, string archiveFileName)
    {
        if (!IOFile.Exists(archiveFileName)) return;

        var fileExtension = Path.GetExtension(archiveFileName);

        if (string.Equals(fileExtension, ".zip", StringComparison.InvariantCultureIgnoreCase)) return;

        //var zipFileName = string.Concat(archiveFileName[..^fileExtension.Length], ".zip");
        var zipFileName = string.Concat(archiveFileName.Substring(0, archiveFileName.Length - fileExtension.Length), ".zip");
        var logFiles = this.FindExistsPartialFileNames(archiveFileName).ToList();

        logFiles.Add(archiveFileName);

        CompressFiles(this.CompressionLevel, zipFileName, logFiles);

        foreach (var logFile in logFiles.Where(IOFile.Exists))
        {
            IOFile.Delete(logFile);
        }
    }

    protected override void PartialFileCreated(string originalFileName, string partialFileName)
    {
        if (!this.CompressEachPartialFiles) return;
        if (!IOFile.Exists(partialFileName)) return;

        var fileExtension = Path.GetExtension(partialFileName);

        if (string.Equals(fileExtension, ".zip", StringComparison.InvariantCultureIgnoreCase)) return;
       
        var zipFileName = string.Concat(partialFileName.Substring(0, partialFileName.Length - fileExtension.Length), ".zip");

        if (zipFileName.ToLower().Contains(".partial"))
        {
            zipFileName = string.Concat(zipFileName.Substring(0, zipFileName.ToLower().IndexOf(".partial", StringComparison.Ordinal)), ".zip");
        }

        zipFileName = this.FindNextPartialFileName(zipFileName);

        //var tempFileName = string.Concat(zipFileName[..^4], fileExtension);
        var tempFileName = string.Concat(zipFileName.Substring(0, zipFileName.Length - 4), fileExtension);

        if (!IOFile.Exists(tempFileName))
        {
            IOFile.Move(partialFileName, tempFileName);
            partialFileName = tempFileName;
        }

        CompressFiles(this.CompressionLevel, zipFileName, [partialFileName]);

        IOFile.Delete(partialFileName);
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