using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration.Test.Common;

// ReSharper disable All

namespace Tentakel.Extensions.Configuration.Test
{
    [TestClass]
    public class ConfiguredTypesTest
    {
        [TestMethod]
        public void TestGetAndGetAll()
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

            var configurationRoot = new ConfigurationRoot(
                new List<IConfigurationProvider>
                {
                    new JsonStreamConfigurationProvider(new JsonStreamConfigurationSource { Stream = ms })
                });

            var configuredTypes = configurationRoot.GetSection("types").Get<ConfiguredTypes>();
            configuredTypes.ConfigurationRoot = configurationRoot;
            configuredTypes["EXC"] = new ConfiguredType {Type = typeof(Class3).FullName};

            Assert.IsTrue(configuredTypes.TryGet<Class1>("C1", out var c1));
            Assert.AreEqual(typeof(Class1), c1.GetType());
            Assert.AreEqual("Value1", c1.Property1);

            Assert.IsTrue(configuredTypes.TryGet<IInterface1>("C1", out var i1));
            Assert.AreEqual(typeof(Class1), i1.GetType());
            Assert.AreEqual("Value1", i1.Property1);


            Assert.IsTrue(configuredTypes.TryGet<IInterface2>("C2", out var c2));
            Assert.AreEqual(typeof(Class2), c2.GetType());
            Assert.AreEqual("Value2", c2.Property2);

            Assert.IsTrue(configuredTypes.TryGet<IInterface2>("C2", out var i2));
            Assert.AreEqual(typeof(Class2), i2.GetType());
            Assert.AreEqual("Value2", i2.Property2);


            Assert.IsTrue(configuredTypes.TryGet<IInterface3>("C3", out var c3));
            Assert.AreEqual(typeof(Class3), c3.GetType());
            Assert.AreEqual("Value3", c3.Property3);

            Assert.IsTrue(configuredTypes.TryGet<IInterface3>("C3", out var i3));
            Assert.AreEqual(typeof(Class3), i3.GetType());
            Assert.AreEqual("Value3", i3.Property3);


            Assert.IsTrue(configuredTypes.TryGet<IInterface1_2>("C1_2", out var c1_2));
            Assert.AreEqual(typeof(Class1_2), c1_2.GetType());
            Assert.AreEqual("Value1", c1_2.Property1);
            Assert.AreEqual("Value2", c1_2.Property2);

            Assert.IsTrue(configuredTypes.TryGet<IInterface1_2>("C1_2", out var i1_2));
            Assert.AreEqual(typeof(Class1_2), i1_2.GetType());
            Assert.AreEqual("Value1", i1_2.Property1);
            Assert.AreEqual("Value2", i1_2.Property2);


            Assert.IsTrue(configuredTypes.TryGet<IInterface2_3>("C2_3", out var c2_3));
            Assert.AreEqual(typeof(Class2_3), c2_3.GetType());
            Assert.AreEqual("Value2", c2_3.Property2);
            Assert.AreEqual("Value3", c2_3.Property3);

            Assert.IsTrue(configuredTypes.TryGet<IInterface2_3>("C2_3", out var i2_3));
            Assert.AreEqual(typeof(Class2_3), i2_3.GetType());
            Assert.AreEqual("Value2", i2_3.Property2);
            Assert.AreEqual("Value3", i2_3.Property3);


            Assert.IsTrue(configuredTypes.TryGet<Class1_2_3>("C1_2_3", out var c1_2_3));
            Assert.AreEqual(typeof(Class1_2_3), c1_2_3.GetType());
            Assert.AreEqual("Value1", c1_2_3.Property1);
            Assert.AreEqual("Value2", c1_2_3.Property2);
            Assert.AreEqual("Value3", c1_2_3.Property3);

            Assert.IsTrue(configuredTypes.TryGet("C1_2_3", out i1));
            Assert.AreEqual(typeof(Class1_2_3), i1.GetType());
            Assert.AreEqual("Value1", i1.Property1);

            Assert.IsTrue(configuredTypes.TryGet("C1_2_3", out i2));
            Assert.AreEqual(typeof(Class1_2_3), i2.GetType());
            Assert.AreEqual("Value2", i2.Property2);

            Assert.IsTrue(configuredTypes.TryGet("C1_2_3", out i3));
            Assert.AreEqual(typeof(Class1_2_3), i3.GetType());
            Assert.AreEqual("Value3", i3.Property3);

            var services1 = configuredTypes.GetAll<IInterface1>().ToList();
            Assert.AreEqual(3, services1.Count);
            Assert.AreEqual(typeof(Class1), services1[0].GetType());
            Assert.AreEqual(typeof(Class1_2), services1[1].GetType());
            Assert.AreEqual(typeof(Class1_2_3), services1[2].GetType());

            var services2 = configuredTypes.GetAll<IInterface2>().ToList();
            Assert.AreEqual(4, services2.Count);
            Assert.AreEqual(typeof(Class1_2), services2[0].GetType());
            Assert.AreEqual(typeof(Class1_2_3), services2[1].GetType());
            Assert.AreEqual(typeof(Class2), services2[2].GetType());
            Assert.AreEqual(typeof(Class2_3), services2[3].GetType());

            var services3 = configuredTypes.GetAll<IInterface3>().ToList();
            Assert.AreEqual(3, services3.Count);
            Assert.AreEqual(typeof(Class1_2_3), services3[0].GetType());
            Assert.AreEqual(typeof(Class2_3), services3[1].GetType());
            Assert.AreEqual(typeof(Class3), services3[2].GetType());

            var exc = configuredTypes.GetAll<Exception>().ToList();
            Assert.AreEqual(1, exc.Count);
            Assert.AreEqual(typeof(TypeLoadException), exc[0].GetType());
        }

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

            var services = new ServiceCollection().AddOptions();
            services.AddSingleton<IConfigurationRoot>(configurationRoot);
            services.AddSingleton(configuredTypes);
            services.TryAddSingleton<ConfiguredTypesProvider>();
            services.Configure<ConfiguredTypes>(configurationRoot.GetSection("types"));

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
                } );

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
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetInvalidOperationException()
        {
            var _ = new ConfiguredTypes().Get<object>("C1");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetAllInvalidOperationException()
        {
            var _ = new ConfiguredTypes().GetAll<object>().ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestTryGetInvalidOperationException()
        {
            new ConfiguredTypes().TryGet<object>("C1", out _);
        }




        [TestMethod]
        public void TestCreateHostConfigurationFromJsonStream2()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonFile(testSettingsPath, false, true);

               

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

                collection.Configure<ConfiguredTypes>(configuration.GetSection("types"));
                collection.Configure<ConfiguredTypes>("C1", configurationRoot.GetSection("types:A"));
                collection.Configure<ConfiguredTypes>("C2", configurationRoot.GetSection("types:B"));


            }).Build();


            //var monitor = host.Services.GetRequiredService<IOptionsSnapshot<ConfiguredTypesProvider>>();


            //var currentValue = monitor.Value.Get<Class1>("C1");

            //var a = monitor.Get("types:A").Get<Class2>("C3");
            //var b = monitor.Get("types:A").Get<Class2>("C4");


            //return;

            Task.Run(() =>
            {
                var configuredTypes = host.Services.GetRequiredService<IConfiguredTypes>();
                var objects = configuredTypes.GetAll<object>().ToList();

                var c2A = configuredTypes.Get<Class2>("types:A:C3");
                var c3B = configuredTypes.Get<Class3>("types:B:C6");


            });


            Thread.Sleep(100000);
          
        }
    }
}