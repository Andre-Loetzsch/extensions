using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oleander.Extensions.Configuration;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Extensions.Logging.File.App
{
    internal class Programm
    {
        public static void Main()
        {
            Console.WriteLine("Start!");

            TestConfigurationChanged();

            Console.WriteLine("Stop!");
            Console.ReadLine();
        }

        public static void TestConfigurationChanged()
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

            for (var i = 1; i <= 10; i++)
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

            var logContent = Encoding.UTF8.GetString(buffer).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var format1Counter = logContent.Count(l => l.Contains("Format1"));
            var format2Counter = logContent.Count(l => l.Contains("Format2"));

        }
    }
}
