using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Oleander.Extensions.Configuration.Tests
{
    [TestClass]
    public class ConfiguredTypesOptionsTest
    {
        [TestMethod]
        public void TestGenericClass()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };

            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dict, "types"));
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonStream(ms);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptions<>), typeof(ConfiguredTypesOptions<>));

            }).Build();

            var c1Options = host.Services.GetRequiredService<IConfiguredTypesOptions<Class1>>();
            var c2Options = host.Services.GetRequiredService<IConfiguredTypesOptions<Class2>>();
            var c3Options = host.Services.GetRequiredService<IConfiguredTypesOptions<Class3>>();

            Assert.IsNotNull(c1Options);
            Assert.IsNotNull(c2Options);
            Assert.IsNotNull(c3Options);

            // -- Class1 monitor
            Assert.AreEqual(1, c1Options.GetKeys().Count);
            Assert.AreEqual("C1", c1Options.GetKeys().ToList()[0]);
            Assert.AreEqual("Value1", c1Options.Get("C1")?.Property1);
            
            Assert.IsNull(c1Options.Get("C2")?.Property1);
            Assert.IsNull(c1Options.Get("C3")?.Property1);

            // -- Class2 monitor
            Assert.AreEqual(1, c2Options.GetKeys().Count);
            Assert.AreEqual("C2", c2Options.GetKeys().ToList()[0]);
            Assert.AreEqual("Value2", c2Options.Get("C2")?.Property2);

            Assert.IsNull(c2Options.Get("C1")?.Property2);
            Assert.IsNull(c2Options.Get("C3")?.Property2);

            // -- Class3 monitor
            Assert.AreEqual(1, c3Options.GetKeys().Count);
            Assert.AreEqual("C3", c3Options.GetKeys().ToList()[0]); 
            Assert.AreEqual("Value3", c3Options.Get("C3")?.Property3);

            Assert.IsNull(c3Options.Get("C1")?.Property3);
            Assert.IsNull(c3Options.Get("C2")?.Property3);
        }

        [TestMethod]
        public void TestGenericInterface()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };


            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dict, "types"));
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonStream(ms);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptions<>), typeof(ConfiguredTypesOptions<>));

            }).Build();

            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptions<IInterface1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptions<IInterface2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptions<IInterface3>>();

            Assert.IsNotNull(c1OptionsMonitor);
            Assert.IsNotNull(c2OptionsMonitor);
            Assert.IsNotNull(c3OptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value1", c1OptionsMonitor.Get("C1")?.Property1);

            Assert.IsNull(c1OptionsMonitor.Get("C2"));
            Assert.IsNull(c1OptionsMonitor.Get("C3"));

            // -- Class2 monitor
            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value2", c2OptionsMonitor.Get("C2")?.Property2);

            Assert.IsNull(c2OptionsMonitor.Get("C1"));
            Assert.IsNull(c2OptionsMonitor.Get("C3"));

            // -- Class3 monitor
            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3", c3OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value3", c3OptionsMonitor.Get("C3")?.Property3);

            Assert.IsNull(c3OptionsMonitor.Get("C1"));
            Assert.IsNull(c3OptionsMonitor.Get("C2"));
        }

        [TestMethod]
        public void TestNonGenericClass()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };
          

            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dict, "types"));
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonStream(ms);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptions), typeof(ConfiguredTypesOptions));

            }).Build();

            var cOptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptions>();

            Assert.IsNotNull(cOptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class1>().Count);
            Assert.AreEqual("C1", cOptionsMonitor.GetKeys<Class1>().ToList()[0]);
            Assert.AreEqual("Value1", cOptionsMonitor.Get<Class1>("C1")?.Property1);

            Assert.IsNull(cOptionsMonitor.Get<Class1>("C2"));
            Assert.IsNull(cOptionsMonitor.Get<Class1>("C3"));

            // -- Class2 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class2>().Count);
            Assert.AreEqual("C2", cOptionsMonitor.GetKeys<Class2>().ToList()[0]);
            Assert.AreEqual("Value2", cOptionsMonitor.Get<Class2>("C2")?.Property2);

            Assert.IsNull(cOptionsMonitor.Get<Class2>("C1"));
            Assert.IsNull(cOptionsMonitor.Get<Class2>("C3"));

            // -- Class3 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class3>().Count);
            Assert.AreEqual("C3", cOptionsMonitor.GetKeys<Class3>().ToList()[0]);
            Assert.AreEqual("Value3", cOptionsMonitor.Get<Class3>("C3")?.Property3);

            Assert.IsNull(cOptionsMonitor.Get<Class3>("C1"));
            Assert.IsNull(cOptionsMonitor.Get<Class3>("C2"));
        }

        [TestMethod]
        public void TestNonGenericInterface()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };

            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(JsonStringBuilder.Build(dict, "types"));
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

           

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonStream(ms);

            }).ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptions), typeof(ConfiguredTypesOptions));

            }).Build();

            var cOptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptions>();

            Assert.IsNotNull(cOptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface1>().Count);
            Assert.AreEqual("C1", cOptionsMonitor.GetKeys<IInterface1>().ToList()[0]);
            Assert.AreEqual("Value1", cOptionsMonitor.Get<IInterface1>("C1")?.Property1);

            Assert.IsNull(cOptionsMonitor.Get<IInterface1>("C2"));
            Assert.IsNull(cOptionsMonitor.Get<IInterface1>("C3"));

            // -- Class2 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface2>().Count);
            Assert.AreEqual("C2", cOptionsMonitor.GetKeys<IInterface2>().ToList()[0]);
            Assert.AreEqual("Value2", cOptionsMonitor.Get<IInterface2>("C2")?.Property2);

            Assert.IsNull(cOptionsMonitor.Get<IInterface2>("C1"));
            Assert.IsNull(cOptionsMonitor.Get<IInterface2>("C3"));

            // -- Class3 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3", cOptionsMonitor.GetKeys<IInterface3>().ToList()[0]);
            Assert.AreEqual("Value3", cOptionsMonitor.Get<IInterface3>("C3")?.Property3);

            Assert.IsNull(cOptionsMonitor.Get<IInterface3>("C1"));
            Assert.IsNull(cOptionsMonitor.Get<IInterface3>("C2"));
        }

        [TestMethod]
        public void TestGenericSingleton()
        {
            var host = new HostBuilder().ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptions<>), typeof(ConfiguredTypesOptions<>));

            }).Build();

            var c1OptionsMonitorA = host.Services.GetRequiredService<IConfiguredTypesOptions<Class1>>();
            var c1OptionsMonitorB = host.Services.GetRequiredService<IConfiguredTypesOptions<Class1>>();

            Assert.AreSame(c1OptionsMonitorA, c1OptionsMonitorB);

            var c1OptionsMonitorC = host.Services.GetRequiredService<IOptions<Class1>>();
            var c1OptionsMonitorD = host.Services.GetRequiredService<IOptions<Class1>>();

            Assert.AreSame(c1OptionsMonitorC, c1OptionsMonitorD);
        }

        [TestMethod]
        public void TestNonGenericSingleton()
        {
            var host = new HostBuilder().ConfigureServices(collection =>
            {
                var serviceProvider = collection.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptions), typeof(ConfiguredTypesOptions));

            }).Build();

            var c1OptionsMonitorA = host.Services.GetRequiredService<IConfiguredTypesOptions>();
            var c1OptionsMonitorB = host.Services.GetRequiredService<IConfiguredTypesOptions>();

            Assert.AreSame(c1OptionsMonitorA, c1OptionsMonitorB);
        }
    }
}