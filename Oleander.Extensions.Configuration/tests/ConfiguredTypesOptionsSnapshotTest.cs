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
    public class ConfiguredTypesOptionsSnapshotTest
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
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsSnapshot<>), typeof(ConfiguredTypesOptionsSnapshot<>));

            }).Build();

            var c1OptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<Class1>>();
            var c2OptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<Class2>>();
            var c3OptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<Class3>>();

            Assert.IsNotNull(c1OptionsSnapshot);
            Assert.IsNotNull(c2OptionsSnapshot);
            Assert.IsNotNull(c3OptionsSnapshot);

            // -- Class1 Snapshot
            Assert.AreEqual(1, c1OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C1", c1OptionsSnapshot.GetKeys().ToList()[0]);
            Assert.AreEqual("Value1", c1OptionsSnapshot.Get("C1")?.Property1);

            Assert.AreEqual(1, c1OptionsSnapshot.GetKeys("A").Count);
            Assert.AreEqual("C1A", c1OptionsSnapshot.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value1A", c1OptionsSnapshot.Get("A", "C1A")?.Property1);

            Assert.AreEqual(1, c1OptionsSnapshot.GetKeys("B").Count);
            Assert.AreEqual("C1B", c1OptionsSnapshot.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value1B", c1OptionsSnapshot.Get("B", "C1B")?.Property1);
            
            Assert.IsNull(c1OptionsSnapshot.Get("C2")?.Property1);
            Assert.IsNull(c1OptionsSnapshot.Get("C3")?.Property1);

            // -- Class2 Snapshot
            Assert.AreEqual(1, c2OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C2", c2OptionsSnapshot.GetKeys().ToList()[0]);
            Assert.AreEqual("Value2", c2OptionsSnapshot.Get("C2")?.Property2);

            Assert.AreEqual(1, c2OptionsSnapshot.GetKeys("A").Count);
            Assert.AreEqual("C2A", c2OptionsSnapshot.GetKeys("A").ToList()[0]); 
            Assert.AreEqual("Value2A", c2OptionsSnapshot.Get("A", "C2A")?.Property2);

            Assert.AreEqual(1, c2OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C2B", c2OptionsSnapshot.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value2B", c2OptionsSnapshot.Get("B", "C2B")?.Property2);

            Assert.IsNull(c2OptionsSnapshot.Get("C1")?.Property2);
            Assert.IsNull(c2OptionsSnapshot.Get("C3")?.Property2);

            // -- Class3 Snapshot
            Assert.AreEqual(1, c3OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C3", c3OptionsSnapshot.GetKeys().ToList()[0]); 
            Assert.AreEqual("Value3", c3OptionsSnapshot.Get("C3")?.Property3);

            Assert.AreEqual(1, c3OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C3A", c3OptionsSnapshot.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value3A", c3OptionsSnapshot.Get("A", "C3A")?.Property3);

            Assert.AreEqual(1, c3OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C3B", c3OptionsSnapshot.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value3B", c3OptionsSnapshot.Get("B", "C3B")?.Property3);

            Assert.IsNull(c3OptionsSnapshot.Get("C1")?.Property3);
            Assert.IsNull(c3OptionsSnapshot.Get("C2")?.Property3);
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
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsSnapshot<>), typeof(ConfiguredTypesOptionsSnapshot<>));

            }).Build();

            var c1OptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<IInterface1>>();
            var c2OptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<IInterface2>>();
            var c3OptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<IInterface3>>();

            Assert.IsNotNull(c1OptionsSnapshot);
            Assert.IsNotNull(c2OptionsSnapshot);
            Assert.IsNotNull(c3OptionsSnapshot);

            // -- Class1 Snapshot
            Assert.AreEqual(1, c1OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C1", c1OptionsSnapshot.GetKeys().ToList()[0]);
            Assert.AreEqual("Value1", c1OptionsSnapshot.Get("C1")?.Property1);

            Assert.AreEqual(1, c1OptionsSnapshot.GetKeys("A").Count);
            Assert.AreEqual("C1A", c1OptionsSnapshot.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value1A", c1OptionsSnapshot.Get("A", "C1A")?.Property1);

            Assert.AreEqual(1, c1OptionsSnapshot.GetKeys("B").Count);
            Assert.AreEqual("C1B", c1OptionsSnapshot.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value1B", c1OptionsSnapshot.Get("B", "C1B")?.Property1);

            Assert.IsNull(c1OptionsSnapshot.Get("C2"));
            Assert.IsNull(c1OptionsSnapshot.Get("C3"));

            // -- Class2 Snapshot
            Assert.AreEqual(1, c2OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C2", c2OptionsSnapshot.GetKeys().ToList()[0]);
            Assert.AreEqual("Value2", c2OptionsSnapshot.Get("C2")?.Property2);

            Assert.AreEqual(1, c2OptionsSnapshot.GetKeys("A").Count);
            Assert.AreEqual("C2A", c2OptionsSnapshot.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value2A", c2OptionsSnapshot.Get("A", "C2A")?.Property2);

            Assert.AreEqual(1, c2OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C2B", c2OptionsSnapshot.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value2B", c2OptionsSnapshot.Get("B", "C2B")?.Property2);

            Assert.IsNull(c2OptionsSnapshot.Get("C1"));
            Assert.IsNull(c2OptionsSnapshot.Get("C3"));

            // -- Class3 Snapshot
            Assert.AreEqual(1, c3OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C3", c3OptionsSnapshot.GetKeys().ToList()[0]);
            Assert.AreEqual("Value3", c3OptionsSnapshot.Get("C3")?.Property3);

            Assert.AreEqual(1, c3OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C3A", c3OptionsSnapshot.GetKeys("A").ToList()[0]);
            Assert.AreEqual("Value3A", c3OptionsSnapshot.Get("A", "C3A")?.Property3);

            Assert.AreEqual(1, c3OptionsSnapshot.GetKeys().Count);
            Assert.AreEqual("C3B", c3OptionsSnapshot.GetKeys("B").ToList()[0]);
            Assert.AreEqual("Value3B", c3OptionsSnapshot.Get("B", "C3B")?.Property3);

            Assert.IsNull(c3OptionsSnapshot.Get("C1"));
            Assert.IsNull(c3OptionsSnapshot.Get("C2"));
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
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsSnapshot), typeof(ConfiguredTypesOptionsSnapshot));

            }).Build();

            var cOptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot>();

            Assert.IsNotNull(cOptionsSnapshot);

            // -- Class1 Snapshot
            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class1>().Count);
            Assert.AreEqual("C1", cOptionsSnapshot.GetKeys<Class1>().ToList()[0]);
            Assert.AreEqual("Value1", cOptionsSnapshot.Get<Class1>("C1")?.Property1);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class1>("A").Count);
            Assert.AreEqual("C1A", cOptionsSnapshot.GetKeys<Class1>("A").ToList()[0]);
            Assert.AreEqual("Value1A", cOptionsSnapshot.Get<Class1>("A", "C1A")?.Property1);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class1>("B").Count);
            Assert.AreEqual("C1B", cOptionsSnapshot.GetKeys<Class1>("B").ToList()[0]);
            Assert.AreEqual("Value1B", cOptionsSnapshot.Get<Class1>("B", "C1B")?.Property1);

            Assert.IsNull(cOptionsSnapshot.Get<Class1>("C2"));
            Assert.IsNull(cOptionsSnapshot.Get<Class1>("C3"));

            // -- Class2 Snapshot
            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class2>().Count);
            Assert.AreEqual("C2", cOptionsSnapshot.GetKeys<Class2>().ToList()[0]);
            Assert.AreEqual("Value2", cOptionsSnapshot.Get<Class2>("C2")?.Property2);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class2>("A").Count);
            Assert.AreEqual("C2A", cOptionsSnapshot.GetKeys<Class2>("A").ToList()[0]);
            Assert.AreEqual("Value2A", cOptionsSnapshot.Get<Class2>("A", "C2A")?.Property2);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class2>().Count);
            Assert.AreEqual("C2B", cOptionsSnapshot.GetKeys<Class2>("B").ToList()[0]);
            Assert.AreEqual("Value2B", cOptionsSnapshot.Get<Class2>("B", "C2B")?.Property2);

            Assert.IsNull(cOptionsSnapshot.Get<Class2>("C1"));
            Assert.IsNull(cOptionsSnapshot.Get<Class2>("C3"));

            // -- Class3 Snapshot
            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class3>().Count);
            Assert.AreEqual("C3", cOptionsSnapshot.GetKeys<Class3>().ToList()[0]);
            Assert.AreEqual("Value3", cOptionsSnapshot.Get<Class3>("C3")?.Property3);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class3>().Count);
            Assert.AreEqual("C3A", cOptionsSnapshot.GetKeys<Class3>("A").ToList()[0]);
            Assert.AreEqual("Value3A", cOptionsSnapshot.Get<Class3>("A", "C3A")?.Property3);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<Class3>().Count);
            Assert.AreEqual("C3B", cOptionsSnapshot.GetKeys<Class3>("B").ToList()[0]);
            Assert.AreEqual("Value3B", cOptionsSnapshot.Get<Class3>("B", "C3B")?.Property3);

            Assert.IsNull(cOptionsSnapshot.Get<Class3>("C1"));
            Assert.IsNull(cOptionsSnapshot.Get<Class3>("C2"));
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
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsSnapshot), typeof(ConfiguredTypesOptionsSnapshot));

            }).Build();

            var cOptionsSnapshot = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot>();

            Assert.IsNotNull(cOptionsSnapshot);

            // -- Class1 Snapshot
            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface1>().Count);
            Assert.AreEqual("C1", cOptionsSnapshot.GetKeys<IInterface1>().ToList()[0]);
            Assert.AreEqual("Value1", cOptionsSnapshot.Get<IInterface1>("C1")?.Property1);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface1>("A").Count);
            Assert.AreEqual("C1A", cOptionsSnapshot.GetKeys<IInterface1>("A").ToList()[0]);
            Assert.AreEqual("Value1A", cOptionsSnapshot.Get<IInterface1>("A", "C1A")?.Property1);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface1>("B").Count);
            Assert.AreEqual("C1B", cOptionsSnapshot.GetKeys<IInterface1>("B").ToList()[0]);
            Assert.AreEqual("Value1B", cOptionsSnapshot.Get<IInterface1>("B", "C1B")?.Property1);

            Assert.IsNull(cOptionsSnapshot.Get<IInterface1>("C2"));
            Assert.IsNull(cOptionsSnapshot.Get<IInterface1>("C3"));

            // -- Class2 Snapshot
            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface2>().Count);
            Assert.AreEqual("C2", cOptionsSnapshot.GetKeys<IInterface2>().ToList()[0]);
            Assert.AreEqual("Value2", cOptionsSnapshot.Get<IInterface2>("C2")?.Property2);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface2>("A").Count);
            Assert.AreEqual("C2A", cOptionsSnapshot.GetKeys<IInterface2>("A").ToList()[0]);
            Assert.AreEqual("Value2A", cOptionsSnapshot.Get<IInterface2>("A", "C2A")?.Property2);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface2>().Count);
            Assert.AreEqual("C2B", cOptionsSnapshot.GetKeys<IInterface2>("B").ToList()[0]);
            Assert.AreEqual("Value2B", cOptionsSnapshot.Get<IInterface2>("B", "C2B")?.Property2);

            Assert.IsNull(cOptionsSnapshot.Get<IInterface2>("C1"));
            Assert.IsNull(cOptionsSnapshot.Get<IInterface2>("C3"));

            // -- Class3 Snapshot
            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3", cOptionsSnapshot.GetKeys<IInterface3>().ToList()[0]);
            Assert.AreEqual("Value3", cOptionsSnapshot.Get<IInterface3>("C3")?.Property3);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3A", cOptionsSnapshot.GetKeys<IInterface3>("A").ToList()[0]);
            Assert.AreEqual("Value3A", cOptionsSnapshot.Get<IInterface3>("A", "C3A")?.Property3);

            Assert.AreEqual(1, cOptionsSnapshot.GetKeys<IInterface3>().Count);
            Assert.AreEqual("C3B", cOptionsSnapshot.GetKeys<IInterface3>("B").ToList()[0]);
            Assert.AreEqual("Value3B", cOptionsSnapshot.Get<IInterface3>("B", "C3B")?.Property3);

            Assert.IsNull(cOptionsSnapshot.Get<IInterface3>("C1"));
            Assert.IsNull(cOptionsSnapshot.Get<IInterface3>("C2"));
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
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsSnapshot<>), typeof(ConfiguredTypesOptionsSnapshot<>));

            }).Build();

            var c1OptionsSnapshotA = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<Class1>>();
            var c1OptionsSnapshotB = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot<Class1>>();

            Assert.AreSame(c1OptionsSnapshotA, c1OptionsSnapshotB);

            var c1OptionsSnapshotC = host.Services.GetRequiredService<IOptionsSnapshot<Class1>>();
            var c1OptionsSnapshotD = host.Services.GetRequiredService<IOptionsSnapshot<Class1>>();

            Assert.AreSame(c1OptionsSnapshotC, c1OptionsSnapshotD);
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
                    .TryAddSingleton(typeof(IConfiguredTypesOptionsSnapshot), typeof(ConfiguredTypesOptionsSnapshot));

            }).Build();

            var c1OptionsSnapshotA = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot>();
            var c1OptionsSnapshotB = host.Services.GetRequiredService<IConfiguredTypesOptionsSnapshot>();

            Assert.AreSame(c1OptionsSnapshotA, c1OptionsSnapshotB);
        }
    }
}