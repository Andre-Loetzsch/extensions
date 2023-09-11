using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Oleander.Extensions.DependencyInjection;

namespace Oleander.Extensions.Hosting.Abstractions.Tests
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