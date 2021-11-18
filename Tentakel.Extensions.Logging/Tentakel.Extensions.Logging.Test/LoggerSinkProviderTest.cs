using System;
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
using Tentakel.Extensions.Logging.LoggerSinks;
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
        public void TestMethod1()
        {
            var defaultCfg = new Dictionary<string, object>
            {
                ["sink 1"] = new FakeLoggerSink { Categories = new[] { "A", "B", "A1" }, LogLevel = LogLevel.Debug, Name = "Sink 1" },
                ["sink 2"] = new FakeLoggerSink { Categories = new[] { "B", "C", "A2" }, LogLevel = LogLevel.Debug, Name = "Sink 2" },
                ["sink 3"] = new FakeLoggerSink { Categories = new[] { "C", "A", "A3" }, LogLevel = LogLevel.Debug, Name = "Sink 3" }
            };

            var prodCfg = new Dictionary<string, object>
            {
                ["sink 4"] = new FakeLoggerSink { Categories = new[] { "A", "B", "A1" }, LogLevel = LogLevel.Debug, Name = "Sink 4" },
                ["sink 5"] = new FakeLoggerSink { Categories = new[] { "B", "C", "A2" }, LogLevel = LogLevel.Debug, Name = "Sink 5" },
                ["sink 6"] = new FakeLoggerSink { Categories = new[] { "C", "A", "A3" }, LogLevel = LogLevel.Debug, Name = "Sink 6" }
            };

            var devCfg = new Dictionary<string, object>
            {
                ["sink 7"] = new FakeLoggerSink { Categories = new[] { "A", "B", "A1" }, LogLevel = LogLevel.Debug, Name = "Sink 7" },
                ["sink 8"] = new FakeLoggerSink { Categories = new[] { "B", "C", "A2" }, LogLevel = LogLevel.Debug, Name = "Sink 8" },
                ["sink 9"] = new FakeLoggerSink { Categories = new[] { "C", "A", "A3" }, LogLevel = LogLevel.Debug, Name = "Sink 9" }
            };

            var defaultMs = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(defaultCfg, "types"));
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
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    //.Configure<ConfiguredTypes>(configuration.GetSection("defaultCfg"))
                    .Configure<ConfiguredTypes>("prod", configuration.GetSection("prodCfg"))
                    .Configure<ConfiguredTypes>("dev", configuration.GetSection("devCfg"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

                collection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());

            }).ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
             
            }).Build();


            var loggerSinkProvider = host.Services.GetRequiredService<IEnumerable<ILoggerProvider>>().First(x => x.GetType() == typeof(LoggerSinkProvider));
            var loggerSinkMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<FakeLoggerSink>>();




            var sink1 = loggerSinkMonitor.Get("sink 1");


            //var loggerSinks = provider.LoggerSinks.ToList();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

         
            var loggerA = loggerFactory.CreateLogger("A");
            var loggerB = loggerFactory.CreateLogger("B");
            var loggerC = loggerFactory.CreateLogger("C");

       
            loggerA.LogDebug("Test D");


            //loggerA.LogInformation("Test I");

            //var t = typeof(LoggerSinkProvider).AssemblyQualifiedName;

            Thread.Sleep(1000);

            Assert.AreEqual(1, sink1.Entries.Count);

        }

        [TestMethod]
        public void TestMethod2()
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


            var loggerSinkProvider = host.Services.GetRequiredService<IEnumerable<ILoggerProvider>>().First(x => x.GetType() == typeof(LoggerSinkProvider));
            var loggerSinkMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<FakeLoggerSink>>();




            var sink1 = loggerSinkMonitor.Get("sink 1");


            //var loggerSinks = provider.LoggerSinks.ToList();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();


            var loggerA = loggerFactory.CreateLogger("A");
            var loggerB = loggerFactory.CreateLogger("B");
            var loggerC = loggerFactory.CreateLogger("C");


            loggerA.LogDebug("Test D");


            //loggerA.LogInformation("Test I");

            //var t = typeof(LoggerSinkProvider).AssemblyQualifiedName;

            Thread.Sleep(1000);

            Assert.AreEqual(1, sink1.Entries.Count);

        }



    }
}
