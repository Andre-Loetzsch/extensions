using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration.Test.Common;

namespace Tentakel.Extensions.Configuration.Test
{
    [TestClass]
    public class ConfiguredTypesOptionsMonitorTest
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
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

            }).Build();

            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class3>>();

            Assert.IsNotNull(c1OptionsMonitor);
            Assert.IsNotNull(c2OptionsMonitor);
            Assert.IsNotNull(c3OptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value1", c1OptionsMonitor.Get("C1").Property1);

            Assert.AreEqual(1, c1OptionsMonitor.GetKeys("A").Count);
            Assert.AreEqual("C1A", c1OptionsMonitor.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value1A", c1OptionsMonitor.Get("A", "C1A").Property1);

            Assert.AreEqual(1, c1OptionsMonitor.GetKeys("B").Count);
            Assert.AreEqual("C1B", c1OptionsMonitor.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value1B", c1OptionsMonitor.Get("B", "C1B").Property1);
            
            Assert.IsNull(c1OptionsMonitor.Get("C2").Property1);
            Assert.IsNull(c1OptionsMonitor.Get("C3").Property1);

            // -- Class2 monitor
            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value2", c2OptionsMonitor.Get("C2").Property2);

            Assert.AreEqual(1, c2OptionsMonitor.GetKeys("A").Count);
            Assert.AreEqual("C2A", c2OptionsMonitor.GetKeys("A").ToList()[0]); 
            Assert.AreEqual("Value2A", c2OptionsMonitor.Get("A", "C2A").Property2);

            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2B", c2OptionsMonitor.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value2B", c2OptionsMonitor.Get("B", "C2B").Property2);

            Assert.IsNull(c2OptionsMonitor.Get("C1").Property2);
            Assert.IsNull(c2OptionsMonitor.Get("C3").Property2);

            // -- Class3 monitor
            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3", c3OptionsMonitor.GetKeys().ToList()[0]); 
            Assert.AreEqual("Value3", c3OptionsMonitor.Get("C3").Property3);

            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3A", c3OptionsMonitor.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value3A", c3OptionsMonitor.Get("A", "C3A").Property3);

            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3B", c3OptionsMonitor.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value3B", c3OptionsMonitor.Get("B", "C3B").Property3);

            Assert.IsNull(c3OptionsMonitor.Get("C1").Property3);
            Assert.IsNull(c3OptionsMonitor.Get("C2").Property3);
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
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

            }).Build();

            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<IInterface1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<IInterface2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<IInterface3>>();

            Assert.IsNotNull(c1OptionsMonitor);
            Assert.IsNotNull(c2OptionsMonitor);
            Assert.IsNotNull(c3OptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value1", c1OptionsMonitor.Get("C1").Property1);

            Assert.AreEqual(1, c1OptionsMonitor.GetKeys("A").Count);
            Assert.AreEqual("C1A", c1OptionsMonitor.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value1A", c1OptionsMonitor.Get("A", "C1A").Property1);

            Assert.AreEqual(1, c1OptionsMonitor.GetKeys("B").Count);
            Assert.AreEqual("C1B", c1OptionsMonitor.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value1B", c1OptionsMonitor.Get("B", "C1B").Property1);

            Assert.IsNull(c1OptionsMonitor.Get("C2"));
            Assert.IsNull(c1OptionsMonitor.Get("C3"));

            // -- Class2 monitor
            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value2", c2OptionsMonitor.Get("C2").Property2);

            Assert.AreEqual(1, c2OptionsMonitor.GetKeys("A").Count);
            Assert.AreEqual("C2A", c2OptionsMonitor.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value2A", c2OptionsMonitor.Get("A", "C2A").Property2);

            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2B", c2OptionsMonitor.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value2B", c2OptionsMonitor.Get("B", "C2B").Property2);

            Assert.IsNull(c2OptionsMonitor.Get("C1"));
            Assert.IsNull(c2OptionsMonitor.Get("C3"));

            // -- Class3 monitor
            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3", c3OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("Value3", c3OptionsMonitor.Get("C3").Property3);

            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3A", c3OptionsMonitor.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value3A", c3OptionsMonitor.Get("A", "C3A").Property3);

            Assert.AreEqual(1, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3B", c3OptionsMonitor.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value3B", c3OptionsMonitor.Get("B", "C3B").Property3);

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
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor), typeof(ConfiguredTypesOptionsMonitor));

            }).Build();

            var cOptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();

            Assert.IsNotNull(cOptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class1>().Count);
            Assert.AreEqual("C1", cOptionsMonitor.GetKeys<Class1>().ToList()[0]);
            Assert.AreEqual("Value1", cOptionsMonitor.Get<Class1>("C1").Property1);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class1>("A").Count);
            Assert.AreEqual("C1A", cOptionsMonitor.GetKeys<Class1>("A").ToList()[0]);
            Assert.AreEqual("Value1A", cOptionsMonitor.Get<Class1>("A", "C1A").Property1);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class1>("B").Count);
            Assert.AreEqual("C1B", cOptionsMonitor.GetKeys<Class1>("B").ToList()[0]);
            Assert.AreEqual("Value1B", cOptionsMonitor.Get<Class1>("B", "C1B").Property1);

            Assert.IsNull(cOptionsMonitor.Get<Class1>("C2"));
            Assert.IsNull(cOptionsMonitor.Get<Class1>("C3"));

            // -- Class2 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class2>().Count);
            Assert.AreEqual("C2", cOptionsMonitor.GetKeys<Class2>().ToList()[0]);
            Assert.AreEqual("Value2", cOptionsMonitor.Get<Class2>("C2").Property2);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class2>("A").Count);
            Assert.AreEqual("C2A", cOptionsMonitor.GetKeys<Class2>("A").ToList()[0]);
            Assert.AreEqual("Value2A", cOptionsMonitor.Get<Class2>("A", "C2A").Property2);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class2>().Count);
            Assert.AreEqual("C2B", cOptionsMonitor.GetKeys<Class2>("B").ToList()[0]);
            Assert.AreEqual("Value2B", cOptionsMonitor.Get<Class2>("B", "C2B").Property2);

            Assert.IsNull(cOptionsMonitor.Get<Class2>("C1"));
            Assert.IsNull(cOptionsMonitor.Get<Class2>("C3"));

            // -- Class3 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class3>().Count);
            Assert.AreEqual("C3", cOptionsMonitor.GetKeys<Class3>().ToList()[0]);
            Assert.AreEqual("Value3", cOptionsMonitor.Get<Class3>("C3").Property3);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class3>().Count);
            Assert.AreEqual("C3A", cOptionsMonitor.GetKeys<Class3>("A").ToList()[0]);
            Assert.AreEqual("Value3A", cOptionsMonitor.Get<Class3>("A", "C3A").Property3);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<Class3>().Count);
            Assert.AreEqual("C3B", cOptionsMonitor.GetKeys<Class3>("B").ToList()[0]);
            Assert.AreEqual("Value3B", cOptionsMonitor.Get<Class3>("B", "C3B").Property3);

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
                
                collection
                    .AddSingleton((IConfigurationRoot)configuration)
                    .Configure<ConfiguredTypes>(configuration.GetSection("types"))
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor), typeof(ConfiguredTypesOptionsMonitor));

            }).Build();

            var cOptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();

            Assert.IsNotNull(cOptionsMonitor);

            // -- Class1 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface1>().Count);
            Assert.AreEqual("C1", cOptionsMonitor.GetKeys<IInterface1>().ToList()[0]);
            Assert.AreEqual("Value1", cOptionsMonitor.Get<IInterface1>("C1").Property1);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface1>("A").Count);
            Assert.AreEqual("C1A", cOptionsMonitor.GetKeys<IInterface1>("A").ToList()[0]);
            Assert.AreEqual("Value1A", cOptionsMonitor.Get<IInterface1>("A", "C1A").Property1);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface1>("B").Count);
            Assert.AreEqual("C1B", cOptionsMonitor.GetKeys<IInterface1>("B").ToList()[0]);
            Assert.AreEqual("Value1B", cOptionsMonitor.Get<IInterface1>("B", "C1B").Property1);

            Assert.IsNull(cOptionsMonitor.Get<IInterface1>("C2"));
            Assert.IsNull(cOptionsMonitor.Get<IInterface1>("C3"));

            // -- Class2 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface2>().Count);
            Assert.AreEqual("C2", cOptionsMonitor.GetKeys<IInterface2>().ToList()[0]);
            Assert.AreEqual("Value2", cOptionsMonitor.Get<IInterface2>("C2").Property2);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface2>("A").Count);
            Assert.AreEqual("C2A", cOptionsMonitor.GetKeys<IInterface2>("A").ToList()[0]);
            Assert.AreEqual("Value2A", cOptionsMonitor.Get<IInterface2>("A", "C2A").Property2);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface2>().Count);
            Assert.AreEqual("C2B", cOptionsMonitor.GetKeys<IInterface2>("B").ToList()[0]);
            Assert.AreEqual("Value2B", cOptionsMonitor.Get<IInterface2>("B", "C2B").Property2);

            Assert.IsNull(cOptionsMonitor.Get<IInterface2>("C1"));
            Assert.IsNull(cOptionsMonitor.Get<IInterface2>("C3"));

            // -- Class3 monitor
            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3", cOptionsMonitor.GetKeys<IInterface3>().ToList()[0]);
            Assert.AreEqual("Value3", cOptionsMonitor.Get<IInterface3>("C3").Property3);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3A", cOptionsMonitor.GetKeys<IInterface3>("A").ToList()[0]);
            Assert.AreEqual("Value3A", cOptionsMonitor.Get<IInterface3>("A", "C3A").Property3);

            Assert.AreEqual(1, cOptionsMonitor.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3B", cOptionsMonitor.GetKeys<IInterface3>("B").ToList()[0]);
            Assert.AreEqual("Value3B", cOptionsMonitor.Get<IInterface3>("B", "C3B").Property3);

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
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

            }).Build();

            var c1OptionsMonitorA = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class1>>();
            var c1OptionsMonitorB = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class1>>();

            Assert.AreSame(c1OptionsMonitorA, c1OptionsMonitorB);
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
                    .Configure<ConfiguredTypes>("A", configuration.GetSection("typesA"))
                    .Configure<ConfiguredTypes>("B", configuration.GetSection("typesB"))
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor), typeof(ConfiguredTypesOptionsMonitor));

            }).Build();

            var c1OptionsMonitorA = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();
            var c1OptionsMonitorB = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();

            Assert.AreSame(c1OptionsMonitorA, c1OptionsMonitorB);
        }
    }
}