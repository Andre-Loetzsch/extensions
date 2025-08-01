﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Configuration;
using Oleander.Extensions.Logging.Providers;
using Xunit;
using IOFile = System.IO.File;

namespace Oleander.Extensions.Logging.File.Tests;

public class FileSinkTests
{
    [Fact]
    public void TestConfigurationChanged()
    {
        var logfileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "TestConfigurationChanged.log");

        if (System.IO.File.Exists(logfileName)) System.IO.File.Delete(logfileName);

        var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddJsonFile("logging.json", false, true);
        }).ConfigureServices(collection =>
        {
            var serviceProvider = collection.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            collection
                .AddSingleton((IConfigurationRoot)configuration)
                .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));
            collection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());

        }).ConfigureLogging((hostingContext, logging) => { logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging")); }).Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("test");

        for (var i = 1; i <= 1000; i++)
        {
            if (i == 501)
            {
                var loggingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logging.json");
                var loggingFile1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logging1.json");

                System.IO.File.WriteAllText(loggingFile, System.IO.File.ReadAllText(loggingFile1));

                Thread.Sleep(500);
            }

            Thread.Sleep(5);

            logger.LogInformation("This is information: {i}", i);
        }

        Thread.Sleep(2000);

        byte[] buffer;

        using (var fs = System.IO.File.Open(logfileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            buffer = new byte[fs.Length];
            _ = fs.Read(buffer, 0, buffer.Length);
        }

        var logContent = Encoding.UTF8.GetString(buffer).Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        var format1Counter = logContent.Count(l => l.Contains("Format1"));
        var format2Counter = logContent.Count(l => l.Contains("Format2"));

        Assert.True(format1Counter > 480);
        Assert.True(format2Counter > 480);
        Assert.Equal(1000, format1Counter + format2Counter);
    }

    [Fact]
    public void CreateFileIfLogFileNameChanged()
    {
        var traceAFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceA.log");
        var traceBFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceB.log");
        string fileName;

        using (var fileSink = new FileSink())
        {
            fileSink.ArchiveFileNameTemplate = traceAFileName;
            fileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            fileSink.ArchiveFileNameTemplate = traceBFileName;
            fileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });

            fileName = fileSink.FileName;
        }

        Assert.True(IOFile.Exists(traceAFileName));
        Assert.True(IOFile.Exists(fileName));

        IOFile.Delete(traceAFileName);
        IOFile.Delete(fileName);
    }

    [Fact]
    public void CreateFileWithPartialLogFiles()
    {
        var traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.log");
        var traceDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceD.log");
        var archiveCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.log");
        var archiveDFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveD.log");

        if (IOFile.Exists(traceCFileName)) IOFile.Delete(traceCFileName);
        if (IOFile.Exists(traceCFileName)) IOFile.Delete(traceDFileName);

        using (var fileSink = new FileSink())
        {
            fileSink.FileNameTemplate = traceCFileName;
            fileSink.ArchiveFileNameTemplate = archiveCFileName;
            fileSink.MaxFileSize = 1000;
            for (var i = 0; i < 110; i++)
            {
                fileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = $"Test message {i}" });
            }

            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial1.log")), "Assert.True->traceArchiveC.partial1.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial2.log")), "Assert.True->traceArchiveC.partial2.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial3.log")), "Assert.True->traceArchiveC.partial3.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial4.log")), "Assert.True->traceArchiveC.partial4.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial5.log")), "Assert.True->traceArchiveC.partial5.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial6.log")), "Assert.True->traceArchiveC.partial6.log");
            Assert.True(IOFile.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.partial7.log")), "Assert.True->traceArchiveC.partial7.log");

            fileSink.FileNameTemplate = traceDFileName;
            fileSink.ArchiveFileNameTemplate = archiveDFileName;

            fileSink.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message" });
        }

        Assert.False(IOFile.Exists(traceCFileName));

        Assert.True(IOFile.Exists(archiveCFileName));
        Assert.True(IOFile.Exists(traceDFileName));

        IOFile.Delete(archiveCFileName);
        IOFile.Delete(traceDFileName);

        foreach (var file in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging"), "traceArchiveC.partial*.log"))
        {
            IOFile.Delete(file);
        }
    }

    [Fact]
    public void TestWriteInSameLogFile()
    {
        var traceCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "traceC.log");
        var archiveCFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging", "traceArchiveC.log");

        if (IOFile.Exists(traceCFileName)) IOFile.Delete(traceCFileName);

        using var fileSink1 = new FileSink();
        fileSink1.FileNameTemplate = traceCFileName;
        fileSink1.ArchiveFileNameTemplate = archiveCFileName;

        using var fileSink2 = new FileSink();
        fileSink2.FileNameTemplate = traceCFileName;
        fileSink2.ArchiveFileNameTemplate = archiveCFileName;

        fileSink1.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message 1" });
        fileSink1.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message 2" });
        fileSink1.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message 3" });
        
        fileSink2.Log(new() { LogLevel = LogLevel.Debug, LogCategory = "Test", Message = "Test message 4" });
        fileSink2.Log(new() { LogLevel = LogLevel.Debug, LogCategory = "Test", Message = "Test message 5" });
        fileSink2.Log(new() { LogLevel = LogLevel.Debug, LogCategory = "Test", Message = "Test message 6" });

        fileSink1.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message 7" });
        fileSink1.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message 8" });
        fileSink1.Log(new() { LogLevel = LogLevel.Information, LogCategory = "Test", Message = "Test message 9" });

        fileSink2.Log(new() { LogLevel = LogLevel.Debug, LogCategory = "Test", Message = "Test message 10" });
        fileSink2.Log(new() { LogLevel = LogLevel.Debug, LogCategory = "Test", Message = "Test message 11" });
        fileSink2.Log(new() { LogLevel = LogLevel.Debug, LogCategory = "Test", Message = "Test message 12" });

        Assert.True(IOFile.Exists(traceCFileName));

        byte[] buffer;

        using (var fs = IOFile.Open(traceCFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            buffer = new byte[fs.Length];
            fs.Position = 0;
            _ = fs.Read(buffer, 0, buffer.Length);
        }

        var logContent = Encoding.UTF8.GetString(buffer).Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(12, logContent.Length);
        Assert.Contains("Test message 1", logContent[0]);
        Assert.Contains("Test message 2", logContent[1]);
        Assert.Contains("Test message 3", logContent[2]);
        Assert.Contains("Test message 4", logContent[3]);
        Assert.Contains("Test message 5", logContent[4]);
        Assert.Contains("Test message 6", logContent[5]);
        Assert.Contains("Test message 7", logContent[6]);
        Assert.Contains("Test message 8", logContent[7]);
        Assert.Contains("Test message 9", logContent[8]);
        Assert.Contains("Test message 10", logContent[9]);
        Assert.Contains("Test message 11", logContent[10]);
        Assert.Contains("Test message 12", logContent[11]);
    }
}