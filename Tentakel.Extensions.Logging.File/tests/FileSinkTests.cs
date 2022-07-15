using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Logging.Providers;
using Xunit;

namespace Tentakel.Extensions.Logging.File.Tests;

public class FileSinkTests
{
    [Fact]
    public void TestConfigurationChanged()
    {
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

        for (var i = 0; i < 10000; i++)
        {
            if (i == 5000)
            {
                var loggingFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logging.json");
                var loggingFile1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logging1.json");

                System.IO.File.WriteAllText(loggingFile, System.IO.File.ReadAllText(loggingFile1));
            }

            Thread.Sleep(5);

            logger.LogInformation("This is information: {i}", i);
        }

        Thread.Sleep(2000);
    }
}