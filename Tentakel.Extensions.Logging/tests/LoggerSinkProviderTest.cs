using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging.Tests;

[TestClass]
public class LoggerSinkProviderTest
{
    [TestInitialize]
    public void InitTest()
    {
    

    }

    [TestMethod]
    public void TestLoggerSinkBaseDefaultName()
    {
        Assert.AreEqual(nameof(FakeLoggerSink), new FakeLoggerSink().Name);
    }

    [TestMethod]
    public void TestAddJsonStream()
    {
        var defaultCfg = new Dictionary<string, object>
        {
            ["sink 1"] = new FakeLoggerSink { Categories = new[] { "A", "B", "D" }, LogLevel = LogLevel.Debug, Name = "Sink 1" },
            ["sink 2"] = new FakeLoggerSink { Categories = new[] { "A", "B", "C" }, LogLevel = LogLevel.Information, Name = "Sink 2" },
            ["sink 3"] = new FakeLoggerSink { Categories = new[] { "A", "C", "D" }, LogLevel = LogLevel.Error, Name = "Sink 3" }
        };

        var prodCfg = new Dictionary<string, object>
        {
            ["sink 4"] = new FakeLoggerSink { Categories = new[] { "A", "B", "D" }, LogLevel = LogLevel.Debug, Name = "Sink 4" },
            ["sink 5"] = new FakeLoggerSink { Categories = new[] { "A", "B", "C" }, LogLevel = LogLevel.Information, Name = "Sink 5" },
            ["sink 6"] = new FakeLoggerSink { Categories = new[] { "A", "C", "D" }, LogLevel = LogLevel.Error, Name = "Sink 6" }
        };

        var devCfg = new Dictionary<string, object>
        {
            ["sink 7"] = new FakeLoggerSink { Categories = new[] { "A", "B", "D" }, LogLevel = LogLevel.Debug, Name = "Sink 7" },
            ["sink 8"] = new FakeLoggerSink { Categories = new[] { "A", "B", "C" }, LogLevel = LogLevel.Information, Name = "Sink 8" },
            ["sink 9"] = new FakeLoggerSink { Categories = new[] { "A", "C", "D" }, LogLevel = LogLevel.Error, Name = "Sink 9" }
        };

        var defaultMs = new MemoryStream();
        var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(defaultCfg, "defaultCfg"));
        defaultMs.Write(buffer, 0, buffer.Length);
        defaultMs.Position = 0;

        var prodMs = new MemoryStream();
        buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(prodCfg, "prodCfg"));
        prodMs.Write(buffer, 0, buffer.Length);
        prodMs.Position = 0;

        var devMs = new MemoryStream();
        buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(devCfg, "devCfg"));
        devMs.Write(buffer, 0, buffer.Length);
        devMs.Position = 0;

        var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder
                .AddJsonStream(defaultMs)
                .AddJsonStream(prodMs)
                .AddJsonStream(devMs)
                .AddJsonFile("appsettings.json");

        }).ConfigureServices(collection =>
        {
            var serviceProvider = collection.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            collection
                .AddSingleton((IConfigurationRoot)configuration)
                .Configure<ConfiguredTypes>(configuration.GetSection("defaultCfg"))
                .Configure<ConfiguredTypes>("prod", configuration.GetSection("prodCfg"))
                .Configure<ConfiguredTypes>("dev", configuration.GetSection("devCfg"))
                .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

        }).ConfigureLogging((hostingContext, logging) =>
        {
            logging
                .ClearProviders()
                .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());
        }).Build();


        var loggerSinkProvider = (LoggerSinkProvider)host.Services.GetRequiredService<IEnumerable<ILoggerProvider>>().First(x => x.GetType() == typeof(LoggerSinkProvider));
        var loggerSinkMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<FakeLoggerSink>>();

        var sink1 = loggerSinkMonitor.Get("sink 1");
        var sink2 = loggerSinkMonitor.Get("sink 2");
        var sink3 = loggerSinkMonitor.Get("sink 3");
        var sink4 = loggerSinkMonitor.Get("prod", "sink 4");
        var sink5 = loggerSinkMonitor.Get("prod", "sink 5");
        var sink6 = loggerSinkMonitor.Get("prod", "sink 6");
        var sink7 = loggerSinkMonitor.Get("dev", "sink 7");
        var sink8 = loggerSinkMonitor.Get("dev", "sink 8");
        var sink9 = loggerSinkMonitor.Get("dev", "sink 9");

        Assert.IsNotNull(sink1);
        Assert.IsNotNull(sink2);
        Assert.IsNotNull(sink3);
        Assert.IsNotNull(sink4);
        Assert.IsNotNull(sink5);
        Assert.IsNotNull(sink6);
        Assert.IsNotNull(sink7);
        Assert.IsNotNull(sink8);
        Assert.IsNotNull(sink9);

        var sinks = new Dictionary<string, FakeLoggerSink>
        {
            ["sink 1"] = sink1,
            ["sink 2"] = sink2,
            ["sink 3"] = sink3,
            ["sink 4"] = sink4,
            ["sink 5"] = sink5,
            ["sink 6"] = sink6,
            ["sink 7"] = sink7,
            ["sink 8"] = sink8,
            ["sink 9"] = sink9
        };

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var loggerA = loggerFactory.CreateLogger("A");
        var loggerB = loggerFactory.CreateLogger("B");
        var loggerC = loggerFactory.CreateLogger("C");
        var loggerD = loggerFactory.CreateLogger("D");

        Log(loggerA, loggerB, loggerC, loggerD);

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(1500));

        AssertDefaultCfg(sinks);

        foreach (var sink in sinks.Values)
        {
            sink.Entries.Clear();
        }

        loggerSinkProvider.ConfigurationName = "prod";

        Log(loggerA, loggerB, loggerC, loggerD);

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(1500));

        AssertProdCfg(sinks);

        foreach (var sink in sinks.Values)
        {
            sink.Entries.Clear();
        }

        loggerSinkProvider.ConfigurationName = "dev";

        Log(loggerA, loggerB, loggerC, loggerD);

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(1500));

        AssertDevCfg(sinks);
    }

    [TestMethod]
    public void TesAddJsonFile()
    {
        var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder
                .AddJsonFile("appsettings.json")
                .AddJsonFile("logging.json");

        }).ConfigureServices(collection =>
        {
            var serviceProvider = collection.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            collection
                .AddSingleton((IConfigurationRoot)configuration)
                .Configure<ConfiguredTypes>(configuration.GetSection("defaultCfg"))
                .Configure<ConfiguredTypes>("prod", configuration.GetSection("prodCfg"))
                .Configure<ConfiguredTypes>("dev", configuration.GetSection("devCfg"))
                .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));
            collection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());

        }).ConfigureLogging((hostingContext, logging) =>
        {
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

        }).Build();


        var loggerSinkProvider = (LoggerSinkProvider)host.Services.GetRequiredService<IEnumerable<ILoggerProvider>>()
            .First(x => x.GetType() == typeof(LoggerSinkProvider));

        var loggerSinkMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<FakeLoggerSink>>();


        var sink1 = loggerSinkMonitor.Get("sink 1");
        var sink2 = loggerSinkMonitor.Get("sink 2");
        var sink3 = loggerSinkMonitor.Get("sink 3");
        var sink4 = loggerSinkMonitor.Get("prod", "sink 4");
        var sink5 = loggerSinkMonitor.Get("prod", "sink 5");
        var sink6 = loggerSinkMonitor.Get("prod", "sink 6");
        var sink7 = loggerSinkMonitor.Get("dev", "sink 7");
        var sink8 = loggerSinkMonitor.Get("dev", "sink 8");
        var sink9 = loggerSinkMonitor.Get("dev", "sink 9");

        Assert.IsNotNull(sink1);
        Assert.IsNotNull(sink2);
        Assert.IsNotNull(sink3);
        Assert.IsNotNull(sink4);
        Assert.IsNotNull(sink5);
        Assert.IsNotNull(sink6);
        Assert.IsNotNull(sink7);
        Assert.IsNotNull(sink8);
        Assert.IsNotNull(sink9);

        var sinks = new Dictionary<string, FakeLoggerSink>
        {
            ["sink 1"] = sink1,
            ["sink 2"] = sink2,
            ["sink 3"] = sink3,
            ["sink 4"] = sink4,
            ["sink 5"] = sink5,
            ["sink 6"] = sink6,
            ["sink 7"] = sink7,
            ["sink 8"] = sink8,
            ["sink 9"] = sink9
        };

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var loggerA = loggerFactory.CreateLogger("A");
        var loggerB = loggerFactory.CreateLogger("B");
        var loggerC = loggerFactory.CreateLogger("C");
        var loggerD = loggerFactory.CreateLogger("D");

        Log(loggerA, loggerB, loggerC, loggerD);

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));

        AssertDefaultCfg(sinks);

        foreach (var sink in sinks.Values)
        {
            sink.Entries.Clear();
        }

        loggerSinkProvider.ConfigurationName = "prod";

        Log(loggerA, loggerB, loggerC, loggerD);

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(1500));

        AssertProdCfg(sinks);

        foreach (var sink in sinks.Values)
        {
            sink.Entries.Clear();
        }

        loggerSinkProvider.ConfigurationName = "dev";

        Log(loggerA, loggerB, loggerC, loggerD);

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(1500));

        AssertDevCfg(sinks);
    }

    private static void Log(ILogger loggerA, ILogger loggerB, ILogger loggerC, ILogger loggerD)
    {
        //loggerA.BeginScope()

        loggerA.LogDebug("Test A Debug");
        loggerB.LogDebug("Test B Debug");
        loggerC.LogDebug("Test C Debug");
        loggerD.LogDebug("Test D Debug");

        loggerA.LogInformation("Test A Information");
        loggerB.LogInformation("Test B Information");
        loggerC.LogInformation("Test C Information");
        loggerD.LogInformation("Test D Information");

        loggerA.LogError("Test A Error");
        loggerB.LogError("Test B Error");
        loggerC.LogError("Test C Error");
        loggerD.LogError("Test D Error");
    }

    private static void AssertDefaultCfg(Dictionary<string, FakeLoggerSink> sinks)
    {
        Assert.AreEqual(9, sinks["sink 1"].Entries.Count);
        Assert.AreEqual(6, sinks["sink 2"].Entries.Count);
        Assert.AreEqual(3, sinks["sink 3"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 4"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 5"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 6"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 7"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 8"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 9"].Entries.Count);
    }

    private static void AssertProdCfg(Dictionary<string, FakeLoggerSink> sinks)
    {
        Assert.AreEqual(0, sinks["sink 1"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 2"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 3"].Entries.Count);
        Assert.AreEqual(9, sinks["sink 4"].Entries.Count);
        Assert.AreEqual(6, sinks["sink 5"].Entries.Count);
        Assert.AreEqual(3, sinks["sink 6"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 7"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 8"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 9"].Entries.Count);
    }

    private static void AssertDevCfg(Dictionary<string, FakeLoggerSink> sinks)
    {
        Assert.AreEqual(0, sinks["sink 1"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 2"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 3"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 4"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 5"].Entries.Count);
        Assert.AreEqual(0, sinks["sink 6"].Entries.Count);
        Assert.AreEqual(9, sinks["sink 7"].Entries.Count);
        Assert.AreEqual(6, sinks["sink 8"].Entries.Count);
        Assert.AreEqual(3, sinks["sink 9"].Entries.Count);
    }

    [TestMethod]
    public void TestLoggerSinkLogCategories()
    {
        var loggerSinkProvider = new LoggerSinkProvider();

        var loggerSink1 = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = new[] { "Test" } };
        var loggerSink2 = new FakeLoggerSink { Name = "S2", LogLevel = LogLevel.Information, Categories = new[] { "Test*" } };
        var loggerSink3 = new FakeLoggerSink { Name = "S3", LogLevel = LogLevel.Information, Categories = new[] { "*Test" } };
        var loggerSink4 = new FakeLoggerSink { Name = "S4", LogLevel = LogLevel.Information, Categories = new[] { "*Test*" } };

        loggerSinkProvider.AddOrUpdateLoggerSinks(new[]
        {
            loggerSink1,
            loggerSink2,
            loggerSink3,
            loggerSink4
        });

        var logger1 = loggerSinkProvider.CreateLogger("Test");
        var logger2 = loggerSinkProvider.CreateLogger("Test1");
        var logger3 = loggerSinkProvider.CreateLogger("UnitTest");
        var logger4 = loggerSinkProvider.CreateLogger("UnitTest1");

        logger1.LogInformation("This is test message 1.");
        logger2.LogInformation("This is test message 2.");
        logger3.LogInformation("This is test message 3.");
        logger4.LogInformation("This is test message 4.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));

        Assert.AreEqual(1, loggerSink1.Entries.Count);
        Assert.AreEqual("This is test message 1.", loggerSink1.Entries[0].Message);

        Assert.AreEqual(2, loggerSink2.Entries.Count);
        Assert.AreEqual("This is test message 1.", loggerSink2.Entries[0].Message);
        Assert.AreEqual("This is test message 2.", loggerSink2.Entries[1].Message);

        Assert.AreEqual(2, loggerSink3.Entries.Count);
        Assert.AreEqual("This is test message 1.", loggerSink3.Entries[0].Message);
        Assert.AreEqual("This is test message 3.", loggerSink3.Entries[1].Message);

        Assert.AreEqual(4, loggerSink4.Entries.Count);
        Assert.AreEqual("This is test message 1.", loggerSink4.Entries[0].Message);
        Assert.AreEqual("This is test message 2.", loggerSink4.Entries[1].Message);
        Assert.AreEqual("This is test message 3.", loggerSink4.Entries[2].Message);
        Assert.AreEqual("This is test message 4.", loggerSink4.Entries[3].Message);
    }
}