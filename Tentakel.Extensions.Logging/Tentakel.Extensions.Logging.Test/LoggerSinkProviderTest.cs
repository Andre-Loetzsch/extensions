using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging.Test
{
    [TestClass]
    public class LoggerSinkProviderTest
    {

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

            var sinks = new Dictionary<string, FakeLoggerSink>
            {
                ["sink 1"] = loggerSinkMonitor.Get("sink 1"),
                ["sink 2"] = loggerSinkMonitor.Get("sink 2"),
                ["sink 3"] = loggerSinkMonitor.Get("sink 3"),
                ["sink 4"] = loggerSinkMonitor.Get("prod", "sink 4"),
                ["sink 5"] = loggerSinkMonitor.Get("prod", "sink 5"),
                ["sink 6"] = loggerSinkMonitor.Get("prod", "sink 6"),
                ["sink 7"] = loggerSinkMonitor.Get("dev", "sink 7"),
                ["sink 8"] = loggerSinkMonitor.Get("dev", "sink 8"),
                ["sink 9"] = loggerSinkMonitor.Get("dev", "sink 9")
            };

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var loggerA = loggerFactory.CreateLogger("A");
            var loggerB = loggerFactory.CreateLogger("B");
            var loggerC = loggerFactory.CreateLogger("C");
            var loggerD = loggerFactory.CreateLogger("D");

            Log(loggerA, loggerB, loggerC, loggerD);

            Assert.IsTrue(loggerSinkProvider.WaitOne(300));

            AssertDefaultCfg(sinks);

            foreach (var sink in sinks.Values)
            {
                sink.Entries.Clear();
            }

            loggerSinkProvider.ConfigurationName = "prod";

            Log(loggerA, loggerB, loggerC, loggerD);

            Assert.IsTrue(loggerSinkProvider.WaitOne(3000));

            AssertProdCfg(sinks);

            foreach (var sink in sinks.Values)
            {
                sink.Entries.Clear();
            }

            loggerSinkProvider.ConfigurationName = "dev";

            Log(loggerA, loggerB, loggerC, loggerD);

            Assert.IsTrue(loggerSinkProvider.WaitOne(3000));

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


            var loggerSinkProvider = (LoggerSinkProvider)host.Services.GetRequiredService<IEnumerable<ILoggerProvider>>().First(x => x.GetType() == typeof(LoggerSinkProvider));
            var loggerSinkMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<FakeLoggerSink>>();

            var sinks = new Dictionary<string, FakeLoggerSink>
            {
                ["sink 1"] = loggerSinkMonitor.Get("sink 1"),
                ["sink 2"] = loggerSinkMonitor.Get("sink 2"),
                ["sink 3"] = loggerSinkMonitor.Get("sink 3"),
                ["sink 4"] = loggerSinkMonitor.Get("prod", "sink 4"),
                ["sink 5"] = loggerSinkMonitor.Get("prod", "sink 5"),
                ["sink 6"] = loggerSinkMonitor.Get("prod", "sink 6"),
                ["sink 7"] = loggerSinkMonitor.Get("dev", "sink 7"),
                ["sink 8"] = loggerSinkMonitor.Get("dev", "sink 8"),
                ["sink 9"] = loggerSinkMonitor.Get("dev", "sink 9")
            };

            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var loggerA = loggerFactory.CreateLogger("A");
            var loggerB = loggerFactory.CreateLogger("B");
            var loggerC = loggerFactory.CreateLogger("C");
            var loggerD = loggerFactory.CreateLogger("D");

            Log(loggerA, loggerB, loggerC, loggerD);

            Assert.IsTrue(loggerSinkProvider.WaitOne(3000));

            AssertDefaultCfg(sinks);

            foreach (var sink in sinks.Values)
            {
                sink.Entries.Clear();
            }

            loggerSinkProvider.ConfigurationName = "prod";

            Log(loggerA, loggerB, loggerC, loggerD);

            Assert.IsTrue(loggerSinkProvider.WaitOne(3000));

            AssertProdCfg(sinks);

            foreach (var sink in sinks.Values)
            {
                sink.Entries.Clear();
            }

            loggerSinkProvider.ConfigurationName = "dev";

            Log(loggerA, loggerB, loggerC, loggerD);

            Assert.IsTrue(loggerSinkProvider.WaitOne(3000));

            AssertDevCfg(sinks);

        }

        private static void Log(ILogger loggerA, ILogger loggerB, ILogger loggerC, ILogger loggerD)
        {
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
    }
}
