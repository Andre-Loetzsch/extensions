using Microsoft.Extensions.Logging;
using IOFile = System.IO.File;

namespace Tentakel.Extensions.Logging.ZipFile.Tests;

public class ZipFileSinkTests
{

    [Fact]
    public void CreateZipFileIfLogFileNameChanged()
    {
        var traceAFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceA.log");
        var traceBFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceB.log");

        using (var zipFileSink = new ZipFileSink())
        {
            zipFileSink.FileNameTemplate = traceAFileName;
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            zipFileSink.FileNameTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceB.log");
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });
        }

        traceAFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceA.zip");

        Assert.True(IOFile.Exists(traceAFileName));
        Assert.True(IOFile.Exists(traceBFileName));

        IOFile.Delete(traceAFileName);
        IOFile.Delete(traceBFileName);
    }

    [Fact]
    public void CreateZipFileWithPartialLogFiles()
    {
        var traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.log");
        var traceDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceD.log");

        using (var zipFileSink = new ZipFileSink
        {
            FileNameTemplate = traceCFileName,
            MaxFileSize = 1000
        })
        {
            for (var i = 0; i < 110; i++)
            {
                zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message {i}" });
            }

            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial1.log")), "Assert.True->traceC.partial1.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial2.log")), "Assert.True->traceC.partial2.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial3.log")), "Assert.True->traceC.partial3.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial4.log")), "Assert.True->traceC.partial4.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial5.log")), "Assert.True->traceC.partial5.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial6.log")), "Assert.True->traceC.partial6.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial7.log")), "Assert.True->traceC.partial7.log");

            zipFileSink.FileNameTemplate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceD.log");
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.zip")));

            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial1.log")));
            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial2.log")));
            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial3.log")));
            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial4.log")));
            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial5.log")));
            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial6.log")));
            Assert.False(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.partial7.log")));
        }

        traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.zip");

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
            zipFileSink.FileNameTemplate = traceEFileName;

            for (var i = 0; i < 10000000; i++)
            {
                zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message {i}" });
            }

            var now = DateTime.Now;
            zipFileSink.FileNameTemplate = traceFFileName;
            zipFileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message" });

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