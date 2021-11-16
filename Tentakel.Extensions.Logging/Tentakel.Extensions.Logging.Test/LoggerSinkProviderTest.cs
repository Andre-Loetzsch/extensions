using System;
using System.Collections;
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
        public void TestMethod1()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new ConsoleColorSink { Categories = new[] { "A", "B", "A1" }, ForegroundColor = ConsoleColor.Blue, LogLevel = LogLevel.Debug, Name = "Blue" },
                ["C2"] = new ConsoleColorSink { Categories = new[] { "B", "C", "A2" }, ForegroundColor = ConsoleColor.DarkGreen, LogLevel = LogLevel.Debug, Name = "DarkGreen" },
                ["C3"] = new ConsoleColorSink { Categories = new[] { "C", "A", "A3" }, ForegroundColor = ConsoleColor.DarkRed, LogLevel = LogLevel.Debug, Name = "DarkRed" }
            };

            var dictA = new Dictionary<string, object>
            {
                ["C1A"] = new ConsoleColorSink { Categories = new[] { "A", "B", "A1" }, ForegroundColor = ConsoleColor.Blue, LogLevel = LogLevel.Debug, Name = "Blue" },
                ["C2A"] = new ConsoleColorSink { Categories = new[] { "B", "C", "A2" }, ForegroundColor = ConsoleColor.DarkGreen, LogLevel = LogLevel.Debug, Name = "DarkGreen" },
                ["C3A"] = new ConsoleColorSink { Categories = new[] { "C", "A", "A3" }, ForegroundColor = ConsoleColor.DarkRed, LogLevel = LogLevel.Debug, Name = "DarkRed" }
            };

            var dictB = new Dictionary<string, object>
            {
                ["C1B"] = new ConsoleColorSink { Categories = new[] { "A", "B", "A1" }, ForegroundColor = ConsoleColor.Blue, LogLevel = LogLevel.Debug, Name = "Blue" },
                ["C2B"] = new ConsoleColorSink { Categories = new[] { "B", "C", "A2" }, ForegroundColor = ConsoleColor.DarkGreen, LogLevel = LogLevel.Debug, Name = "DarkGreen" },
                ["C3B"] = new ConsoleColorSink { Categories = new[] { "C", "A", "A3" }, ForegroundColor = ConsoleColor.DarkRed, LogLevel = LogLevel.Debug, Name = "DarkRed" }
            };

            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dict, "types"));
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var msA = new MemoryStream();
            buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dictA, "typesA"));
            msA.Write(buffer, 0, buffer.Length);
            msA.Position = 0;

            var msB = new MemoryStream();
            buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dictB, "typesB"));
            msB.Write(buffer, 0, buffer.Length);
            msB.Position = 0;

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonStream(ms)
                    .AddJsonStream(msA)
                    .AddJsonStream(msB)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));
                collection.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());

            }).Build();


            //ILoggerFactory loggerFactory = new LoggerFactory().AddConsole((_, __) => true);
            //ILogger logger = loggerFactory.CreateLogger<Program>();


            var provider = host.Services.GetRequiredService<IEnumerable<ILoggerProvider>>();//.First();

            //var loggerSinks = provider.LoggerSinks.ToList();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            var loggerA = loggerFactory.CreateLogger("A");
            var loggerB = loggerFactory.CreateLogger("B");
            var loggerC = loggerFactory.CreateLogger("C");

            loggerA.LogDebug("TestX");


            Thread.Sleep(10000);

        }
    }
}
