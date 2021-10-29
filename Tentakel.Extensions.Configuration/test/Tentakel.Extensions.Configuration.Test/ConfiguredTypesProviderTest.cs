using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration.Test.Common;

namespace Tentakel.Extensions.Configuration.Test
{
    [TestClass]
    public class ConfiguredTypesProviderTest
    {
        [TestMethod]
        public void TestCreateAndUpdateJsonConfigFile()
        {
            // ------------------------------
            // Create Test.json file
            // ------------------------------

            var dict = new Dictionary<string, object>
            {
                ["Test"] = new Class1 { Property1 = "Value1" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.json");
            File.WriteAllText(testSettingsPath, jsonStr);

            // ------------------------------
            // Load Test.json file
            // ------------------------------

            var jsonConfigurationSource = new JsonConfigurationSource
            {
                Path = testSettingsPath,
                Optional = false,
                ReloadOnChange = true
            };

            jsonConfigurationSource.ResolveFileProvider();

            var configurationRoot = new ConfigurationRoot(
                new List<IConfigurationProvider>
                {
                    new JsonConfigurationProvider(jsonConfigurationSource)
                });

            var configuredTypes = configurationRoot.GetSection("types").Get<ConfiguredTypes>();
            configuredTypes.ConfigurationRoot = configurationRoot;

            var services = new ServiceCollection()
                .AddOptions()
                .AddSingleton<IConfigurationRoot>(configurationRoot)
                .AddSingleton(configuredTypes);
           
            services.Configure<ConfiguredTypes>(configurationRoot.GetSection("types"))
                .TryAddSingleton<ConfiguredTypesProvider>();

            var serviceProviderFactory = new DefaultServiceProviderFactory();
            var containerBuilder = serviceProviderFactory.CreateBuilder(services);
            var serviceProvider = serviceProviderFactory.CreateServiceProvider(containerBuilder);
            var configuredTypesProvider = serviceProvider.GetRequiredService<ConfiguredTypesProvider>();
            var waitHandle = new AutoResetEvent(false);

            configuredTypesProvider.ConfigurationChanged += () => waitHandle.Set();

            Assert.IsTrue(configuredTypesProvider.TryGet<object>("Test", out var obj));
            var c1 = obj as Class1;
            Assert.IsNotNull(c1);
            Assert.AreEqual("Value1", c1.Property1);

            // ------------------------------
            // Update Test.json file
            // ------------------------------

            dict = new Dictionary<string, object>
            {
                ["Test"] = new Class2 { Property2 = "Value2" }
            };

            jsonStr = JsonStringBuilder.Build(dict, "types");
            testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.json");
            File.WriteAllText(testSettingsPath, jsonStr);

            Assert.IsTrue(waitHandle.WaitOne(1500));
            Assert.IsTrue(configuredTypesProvider.TryGet("Test", out obj));
            var c2 = obj as Class2;
            Assert.IsNotNull(c2);
            Assert.AreEqual("Value2", c2.Property2);
        }

        [TestMethod]
        public void TestCreateHostConfigurationFromJsonStream()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" },
                ["C1_2"] = new Class1_2 { Property1 = "Value1", Property2 = "Value2" },
                ["C2_3"] = new Class2_3 { Property2 = "Value2", Property3 = "Value3" },
                ["C1_2_3"] = new Class1_2_3 { Property1 = "Value1", Property2 = "Value2", Property3 = "Value3" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(jsonStr);
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonStream(ms);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var configurationRoot = (IConfigurationRoot)configuration;

                collection.AddSingleton(configurationRoot);
                collection.TryAddSingleton<ConfiguredTypesProvider>();
                collection.TryAddSingleton<IConfiguredTypes>(provider =>
                {
                    return provider.GetService<ConfiguredTypesProvider>();
                });

                collection.Configure<ConfiguredTypes>(configurationRoot.GetSection("types"));

            }).Build();

            var configuredTypes = host.Services.GetRequiredService<IConfiguredTypes>();
            var objects = configuredTypes.GetAll<object>().ToList();

            Assert.AreEqual(6, objects.Count);
            Assert.AreEqual(typeof(Class1), objects[0].GetType());
            Assert.AreEqual(typeof(Class1_2), objects[1].GetType());
            Assert.AreEqual(typeof(Class1_2_3), objects[2].GetType());
            Assert.AreEqual(typeof(Class2), objects[3].GetType());
            Assert.AreEqual(typeof(Class2_3), objects[4].GetType());
            Assert.AreEqual(typeof(Class3), objects[5].GetType());
        }

        [TestMethod]
        public void TestConfigureOptions()
        {
            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json"), false, true);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                collection.AddSingleton((IConfigurationRoot)configuration);
                collection.AddSingleton(collection);

                collection.TryAddSingleton<ConfigureOptions>();
                collection.TryAddSingleton<ConfiguredTypesProvider>();
                collection.TryAddSingleton<IConfiguredTypes>(provider => provider.GetService<ConfiguredTypesProvider>());

                collection.Configure<ConfiguredTypes>(configuration.GetSection("types"));

                serviceProvider = collection.BuildServiceProvider();
                serviceProvider.GetRequiredService<ConfigureOptions>().Configure();

            }).Build();

            var c1Monitor = host.Services.GetRequiredService<IOptionsMonitor<Class1>>();
            Assert.AreEqual("Value1A", c1Monitor.Get("C1A").Property1);

            var c2Monitor = host.Services.GetRequiredService<IOptionsMonitor<Class2>>();
            Assert.AreEqual("Value2A", c2Monitor.Get("C2A").Property2);

            var configuredTypes = host.Services.GetRequiredService<IConfiguredTypes>();

            foreach (var key in configuredTypes.GetKeys<Class1>())
            {
                Assert.AreNotSame(configuredTypes.Get<Class1>(key), c1Monitor.Get(key));

                Assert.AreEqual(
                    configuredTypes.Get<Class1>(key).Property1,
                    c1Monitor.Get(key).Property1);
            }

            foreach (var key in configuredTypes.GetKeys<Class2>())
            {
                Assert.AreNotSame(configuredTypes.Get<Class2>(key), c2Monitor.Get(key));

                Assert.AreEqual(
                    configuredTypes.Get<Class2>(key).Property2,
                    c2Monitor.Get(key).Property2);
            }

        }
    }
}