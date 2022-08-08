using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tentakel.Extensions.Configuration.Tests
{
    [TestClass]
    public class OptionsMonitorTest
    {
    
        [TestMethod]
        public void TestMergedConfiguration()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };

            var dictA = new Dictionary<string, object>
            {
                ["C1A"] = new Class1 { Property1 = "Value1A" },
                ["C2A"] = new Class2 { Property2 = "Value2A" },
                ["C3A"] = new Class3 { Property3 = "Value3A" }
            };

            var dictB = new Dictionary<string, object>
            {
                ["C1B"] = new Class1 { Property1 = "Value1B" },
                ["C2B"] = new Class2 { Property2 = "Value2B" },
                ["C3B"] = new Class3 { Property3 = "Value3B" }
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
                    .AddJsonStream(msB);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                collection.Configure<ConfiguredTypes>(configuration.GetSection("types"));
                collection.Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"));
                collection.Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"));

                collection.TryAddSingleton<ConfigureOptions>();
                collection.TryAddSingleton(collection);

                serviceProvider = collection.BuildServiceProvider();

                var configureOptions = serviceProvider.GetRequiredService<ConfigureOptions>();

                configureOptions.Configure();
                configureOptions.Configure("A");
                configureOptions.Configure("B");

            }).Build();

            var c1OptionsMonitor = host.Services.GetRequiredService<IOptionsMonitor<Class1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IOptionsMonitor<Class2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IOptionsMonitor<Class3>>();

            Assert.IsNotNull(c1OptionsMonitor);
            Assert.IsNotNull(c2OptionsMonitor);
            Assert.IsNotNull(c3OptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual("Value1", c1OptionsMonitor.Get("C1").Property1);
            Assert.AreEqual("Value1A", c1OptionsMonitor.Get("C1A").Property1);
            Assert.AreEqual("Value1B", c1OptionsMonitor.Get("C1B").Property1);

            Assert.IsNull(c1OptionsMonitor.Get("C2").Property1);
            Assert.IsNull(c1OptionsMonitor.Get("C3").Property1);

            // -- Class2 monitor
            Assert.AreEqual("Value2", c2OptionsMonitor.Get("C2").Property2);
            Assert.AreEqual("Value2A", c2OptionsMonitor.Get("C2A").Property2);
            Assert.AreEqual("Value2B", c2OptionsMonitor.Get("C2B").Property2);

            Assert.IsNull(c2OptionsMonitor.Get("C1").Property2);
            Assert.IsNull(c2OptionsMonitor.Get("C3").Property2);

            // -- Class3 monitor
            Assert.AreEqual("Value3", c3OptionsMonitor.Get("C3").Property3);
            Assert.AreEqual("Value3A", c3OptionsMonitor.Get("C3A").Property3);
            Assert.AreEqual("Value3B", c3OptionsMonitor.Get("C3B").Property3);

            Assert.IsNull(c3OptionsMonitor.Get("C1").Property3);
            Assert.IsNull(c3OptionsMonitor.Get("C2").Property3);
        }

        [TestMethod]
        public void TestOptionsMonitorGet()
        {
            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json"), false, true);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                collection.AddSingleton((IConfigurationRoot)configuration)
                    .AddSingleton(collection);

                collection.TryAddSingleton<ConfigureOptions>();
                collection.TryAddSingleton<ConfiguredTypesOptionsMonitor>();
                collection.Configure<ConfiguredTypes>(configuration.GetSection("types"));

                serviceProvider = collection.BuildServiceProvider();
                serviceProvider.GetRequiredService<ConfigureOptions>().Configure();
                collection.TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor), typeof(ConfiguredTypesOptionsMonitor));

            }).Build();

            var c1Monitor = host.Services.GetRequiredService<IOptionsMonitor<Class1>>();
            Assert.AreEqual("Value1A", c1Monitor.Get("C1A").Property1);

            var c2Monitor = host.Services.GetRequiredService<IOptionsMonitor<Class2>>();
            Assert.AreEqual("Value2A", c2Monitor.Get("C2A").Property2);

            var configuredTypes = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();

            foreach (var key in configuredTypes.GetKeys<Class1>())
            {
                Assert.AreNotSame(configuredTypes.Get<Class1>(key), c1Monitor.Get(key));

                var c1 = configuredTypes.Get<Class1>(key);
                Assert.IsNotNull(c1);
                Assert.AreEqual(c1.Property1, c1Monitor.Get(key).Property1);
            }

            foreach (var key in configuredTypes.GetKeys<Class2>())
            {
                Assert.AreNotSame(configuredTypes.Get<Class2>(key), c2Monitor.Get(key));

                var c2 = configuredTypes.Get<Class2>(key);
                Assert.IsNotNull(c2);
                Assert.AreEqual(c2.Property2, c2Monitor.Get(key).Property2);
            }
        }
    }
}