using System;
using System.IO;
using Microsoft.Extensions.Logging;
using IOFile = System.IO.File;

namespace Oleander.Extensions.Logging.ZipFile.Tests;

public class ZipFileSinkTests
{

    [Fact]
    public void CreateZipFileIfLogFileNameChanged()
    {
        var traceAFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceA.log");
        var traceBFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceB.log");
        string fileName;

        using (var zipFileSink = new ZipFileSink())
        {
            zipFileSink.ArchiveFileNameTemplate = traceAFileName;
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            zipFileSink.ArchiveFileNameTemplate = traceBFileName;
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            fileName = zipFileSink.FileName;
        }

        Assert.False(IOFile.Exists(traceAFileName));

        traceAFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceA.zip");

        Assert.True(IOFile.Exists(traceAFileName));
        Assert.True(IOFile.Exists(fileName));

        IOFile.Delete(traceAFileName);
        IOFile.Delete(fileName);
    }

    [Fact]
    public void CreateZipFileWithPartialLogFiles()
    {
        var traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.log");
        var traceDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceD.log");
        var archiveCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.log");
        var archiveDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveD.log");

        using (var zipFileSink = new ZipFileSink())
        {
            zipFileSink.FileNameTemplate = traceCFileName;
            zipFileSink.ArchiveFileNameTemplate = archiveCFileName;
            zipFileSink.MaxFileSize = 1000;
            for (var i = 0; i < 110; i++)
            {
                zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message {i}" });
            }

            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial1.log")), "Assert.True->traceArchiveC.partial1.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial2.log")), "Assert.True->traceArchiveC.partial2.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial3.log")), "Assert.True->traceArchiveC.partial3.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial4.log")), "Assert.True->traceArchiveC.partial4.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial5.log")), "Assert.True->traceArchiveC.partial5.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial6.log")), "Assert.True->traceArchiveC.partial6.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial7.log")), "Assert.True->traceArchiveC.partial7.log");

            zipFileSink.FileNameTemplate = traceDFileName;
            zipFileSink.ArchiveFileNameTemplate = archiveDFileName;

            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });
        }

        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.zip")));

        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial1.log")));
        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial2.log")));
        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial3.log")));
        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial4.log")));
        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial5.log")));
        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial6.log")));
        Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial7.log")));

        traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.zip");

        Assert.True(IOFile.Exists(traceCFileName));
        Assert.True(IOFile.Exists(traceDFileName));

        IOFile.Delete(traceCFileName);
        IOFile.Delete(traceDFileName);
    }


    [Fact]
    public void CreateZipFileWithPartialLogFilesCompressEachPartialFilesIsTrue()
    {
        var traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.log");
        var traceDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceD.log");
        var archiveCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.log");
        var archiveDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveD.log");


        using (var zipFileSink = new ZipFileSink())
        {
            zipFileSink.FileNameTemplate = traceCFileName;
            zipFileSink.ArchiveFileNameTemplate = archiveCFileName;
            zipFileSink.MaxFileSize = 1000;
            zipFileSink.CompressEachPartialFiles = true;
            for (var i = 0; i < 110; i++)
            {
                zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message {i}" });
            }

            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial1.zip")), "Assert.True->traceArchiveC.partial1.zip");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial2.zip")), "Assert.True->traceArchiveC.partial2.zip");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial3.zip")), "Assert.True->traceArchiveC.partial3.zip");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial4.zip")), "Assert.True->traceArchiveC.partial4.zip");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial5.zip")), "Assert.True->traceArchiveC.partial5.zip");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial6.zip")), "Assert.True->traceArchiveC.partial6.zip");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial7.zip")), "Assert.True->traceArchiveC.partial7.zip");

            zipFileSink.FileNameTemplate = traceDFileName;
            zipFileSink.ArchiveFileNameTemplate = archiveDFileName;

            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.zip")));
        }

        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial1.zip")), "Assert.True->traceArchiveC.partial1.zip");
        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial2.zip")), "Assert.True->traceArchiveC.partial2.zip");
        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial3.zip")), "Assert.True->traceArchiveC.partial3.zip");
        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial4.zip")), "Assert.True->traceArchiveC.partial4.zip");
        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial5.zip")), "Assert.True->traceArchiveC.partial5.zip");
        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial6.zip")), "Assert.True->traceArchiveC.partial6.zip");
        Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial7.zip")), "Assert.True->traceArchiveC.partial7.zip");

        foreach (var file in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging"), "traceArchiveC.partial*.zip"))
        {
            IOFile.Delete(file);
        }

        traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.zip");

        Assert.True(IOFile.Exists(traceCFileName));
        Assert.True(IOFile.Exists(traceDFileName));

        IOFile.Delete(traceCFileName);
        IOFile.Delete(traceDFileName);
    }


    [Fact]
    public void CreateLargeZipFile()
    {
        var traceEFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceE.log");
        var traceFFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceF.log");

        using (var zipFileSink = new ZipFileSink())
        {
            zipFileSink.ArchiveFileNameTemplate = traceEFileName;

            for (var i = 0; i < 10000000; i++)
            {
                zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message {i}" });
            }

            var now = DateTime.Now;
            zipFileSink.ArchiveFileNameTemplate = traceFFileName;
            traceFFileName = zipFileSink.FileName;
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            var diff = DateTime.Now - now;
            Assert.True(diff.TotalMilliseconds < 20000, $"diff.TotalMilliseconds={diff.TotalMilliseconds}");
        }

        traceEFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceE.zip");

        Assert.True(IOFile.Exists(traceEFileName));
        Assert.True(IOFile.Exists(traceFFileName));

        IOFile.Delete(traceEFileName);
        IOFile.Delete(traceFFileName);
    }
}