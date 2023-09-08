using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;
using Oleander.Extensions.DependencyInjection;

namespace Oleander.Extensions.Configuration.Hosting.Abstractions.Tests
{
    public class ConfiguredTypesExtensionsTest
    {
        [Fact]
        public void TestThrowConfiguredTypesExceptions()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");
            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonFile(testSettingsPath, false, true);

            }).ConfigureServices(collection =>
            {
                collection
                    .AddConfiguredTypes();

            }).Build();

            try
            {
               host.LogConfiguredTypesExceptions<ConfiguredTypesExtensionsTest>();
            }
            catch (AggregateException)
            {
               return;
            }

            Assert.Fail("AggregateException not thrown!");
        }
    }
}