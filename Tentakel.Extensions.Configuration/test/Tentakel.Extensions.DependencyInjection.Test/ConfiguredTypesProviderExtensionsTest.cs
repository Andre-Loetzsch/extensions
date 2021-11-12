using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Configuration.Test.Common;

namespace Tentakel.Extensions.DependencyInjection.Test
{
    [TestClass]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    // ReSharper disable once InconsistentNaming
    public class ConfiguredTypesProviderExtensionsTest
    {
        [TestMethod]
        public void TestAddConfiguredTypes()
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

            var host = new HostBuilder().ConfigureServices(collection =>
            {
                collection.AddConfiguredTypes();

            }).ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonStream(ms);
            }).Build();

            var typesOptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();
            var objects = typesOptionsMonitor.GetAll<object>().ToList();

            Assert.AreEqual(6, objects.Count);
            Assert.AreEqual(typeof(Class1), objects[0].GetType());
            Assert.AreEqual(typeof(Class1_2), objects[1].GetType());
            Assert.AreEqual(typeof(Class1_2_3), objects[2].GetType());
            Assert.AreEqual(typeof(Class2), objects[3].GetType());
            Assert.AreEqual(typeof(Class2_3), objects[4].GetType());
            Assert.AreEqual(typeof(Class3), objects[5].GetType());
        }

        [TestMethod]
        public void TestAddConfiguredServices()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1A"] = new Class1 { Property1 = "Value1A" },
                ["C1B"] = new Class1 { Property1 = "Value1B" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(jsonStr);
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var host = new HostBuilder().ConfigureServices(collection =>
            {
                collection
                    .AddConfiguredTypes();

            }).ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonStream(ms);
            }).Build();


            var list1 = host.Services.GetRequiredServices<IInterface1>().ToList();
            Assert.IsNotNull(list1);
            Assert.AreEqual(2, list1.Count);
            Assert.AreEqual("Value1A", list1[0].Property1);
            Assert.AreEqual("Value1B", list1[1].Property1);

            var list2 = host.Services.GetRequiredServices<IInterface1>().ToList();
            Assert.IsNotNull(list2);
            Assert.AreEqual(2, list2.Count);
            Assert.AreEqual("Value1A", list2[0].Property1);
            Assert.AreEqual("Value1B", list2[1].Property1);

            Assert.AreSame(list1[0], list2[0]);
            Assert.AreSame(list1[1], list2[1]);
        }

        [TestMethod]
        public void TestAddSingletonConfiguredType()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1A"] = new Class1 { Property1 = "Value1A" },
                ["C1B"] = new Class1 { Property1 = "Value1B" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            var ms = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(jsonStr);
            ms.Write(buffer, 0, buffer.Length);
            ms.Position = 0;

            var host = new HostBuilder().ConfigureServices(collection =>
            {
                collection.AddConfiguredTypes();

            }).ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonStream(ms);
            }).Build();


            var serviceA1 = host.Services.GetRequiredService<IInterface1>("C1A");
            Assert.IsNotNull(serviceA1);
            Assert.AreEqual("Value1A", serviceA1.Property1);

            var serviceA2 = host.Services.GetRequiredService<IInterface1>("C1A");
            Assert.IsNotNull(serviceA2);
            Assert.AreEqual("Value1A", serviceA2.Property1);
            Assert.AreSame(serviceA1, serviceA2);

            var serviceB = host.Services.GetRequiredService<IInterface1>("C1B");
            Assert.IsNotNull(serviceB);
            Assert.AreEqual("Value1B", serviceB.Property1);
            Assert.AreNotSame(serviceA1, serviceB);

            var services = host.Services.GetRequiredServices<IInterface1>().ToList();

            Assert.AreEqual(2, services.Count);
            Assert.AreEqual("Value1A", services[0].Property1);
            Assert.AreEqual("Value1B", services[1].Property1);

            Assert.AreSame(serviceA1, services[0]);
            Assert.AreSame(serviceA2, services[0]);
            Assert.AreNotSame(serviceB, services[0]);

            Assert.AreNotSame(serviceA1, services[1]);
            Assert.AreNotSame(serviceA2, services[1]);
            Assert.AreSame(serviceB,  services[1]);
        }

        [TestMethod]
        public void TestOptionsMonitor()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");
            var testSettingsPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes1.json");
            var testSettingsPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes2.json");

            File.Copy(testSettingsPath, testSettingsPath2, true);


            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddJsonFile(testSettingsPath2, false, true);

            }).ConfigureServices(collection =>
            {
                collection.AddConfiguredTypes().AddConfigureOptions();

            }).Build();


            var c1OptionsMonitor = host.Services.GetRequiredService<IOptionsMonitor<Class1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IOptionsMonitor<Class2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IOptionsMonitor<Class3>>();

            Assert.IsNotNull(c1OptionsMonitor.Get("C1A"));
            Assert.IsNotNull(c1OptionsMonitor.Get("C1B"));
            
            Assert.IsNotNull(c2OptionsMonitor.Get("C2A"));
            Assert.IsNotNull(c2OptionsMonitor.Get("C2B"));

            Assert.IsNotNull(c3OptionsMonitor.Get("C3A"));
            Assert.IsNotNull(c3OptionsMonitor.Get("C3B"));

            Assert.AreEqual("Value1A", c1OptionsMonitor.Get("C1A").Property1);
            Assert.AreEqual("Value2A", c2OptionsMonitor.Get("C2A").Property2);

            var onChangeResult1 = new Dictionary<string, Class1>();
            var onChangeResult2 = new Dictionary<string, Class2>();
            var onChangeResult3 = new Dictionary<string, Class3>();

            var waitHandle1 = new AutoResetEvent(false);
            var waitHandle2 = new AutoResetEvent(false);
            var waitHandle3 = new AutoResetEvent(false);

            var disp1 = c1OptionsMonitor.OnChange((class1, s) =>
            {
                onChangeResult1.Add(s, class1);
                if (onChangeResult1.Count < 1) return;
                waitHandle1.Set();
            });

            var disp2 = c2OptionsMonitor.OnChange((class2, s) =>
            {
                onChangeResult2.Add(s, class2);
                if (onChangeResult2.Count < 1) return;
                waitHandle2.Set();
            });

            var disp3 = c3OptionsMonitor.OnChange((class3, s) =>
            {
                onChangeResult3.Add(s, class3);
                waitHandle3.Set();
            });

            File.Copy(testSettingsPath1, testSettingsPath2, true);
            Assert.IsTrue(WaitHandle.WaitAll(new WaitHandle[]{ waitHandle1, waitHandle2 }, 300));
            Assert.IsFalse(waitHandle3.WaitOne(300));

            Assert.AreEqual(1, onChangeResult1.Count);
            Assert.AreEqual(1, onChangeResult2.Count);
            Assert.AreEqual(0, onChangeResult3.Count);

            Assert.IsTrue(onChangeResult1.TryGetValue("C1A", out var c1A) && c1A.Property1 == "Value1AX");
            Assert.IsFalse(onChangeResult1.TryGetValue("C1B", out var c1B) && c1B.Property1 == "Value1BX");

            Assert.IsTrue(onChangeResult2.TryGetValue("C2A", out var c2A) && c2A.Property2 == "Value2AX");
            Assert.IsFalse(onChangeResult2.TryGetValue("C2B", out var c2B) && c2B.Property2 == "Value2BX");

            Assert.IsFalse(onChangeResult3.TryGetValue("C3A", out var c3A) && c3A.Property3 == "Value3AX");
            Assert.IsFalse(onChangeResult3.TryGetValue("C3B", out var c3B) && c3B.Property3 == "Value3BX");

            Assert.IsNotNull(c1OptionsMonitor.Get("C1A"));
            Assert.IsNotNull(c1OptionsMonitor.Get("C1B"));

            Assert.IsNotNull(c2OptionsMonitor.Get("C2A"));
            Assert.IsNotNull(c2OptionsMonitor.Get("C2B"));

            Assert.IsNotNull(c3OptionsMonitor.Get("C3A"));
            Assert.IsNotNull(c3OptionsMonitor.Get("C3B"));

            Assert.AreEqual("Value1AX", c1OptionsMonitor.Get("C1A").Property1);
            Assert.IsNull(c1OptionsMonitor.Get("C1B").Property1);

            Assert.AreEqual("Value2AX", c2OptionsMonitor.Get("C2A").Property2);
            Assert.IsNull(c2OptionsMonitor.Get("C2B").Property2);

            Assert.IsNull(c3OptionsMonitor.Get("C3A").Property3);
            Assert.IsNull(c3OptionsMonitor.Get("C3B").Property3);

            disp1.Dispose();
            disp2.Dispose();
            disp3.Dispose();
        }

        [TestMethod]
        public void TestGenericConfiguredTypesOptionsMonitorClass()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");
            var testSettingsPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes1.json");
            var testSettingsPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes2.json");
            var testSubSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypesSub.json");

            File.Copy(testSettingsPath, testSettingsPath2, true);

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                    .AddJsonFile(testSettingsPath2, false, true)
                    .AddJsonFile(testSubSettingsPath, false, true);

            }).ConfigureServices(collection =>
            {
                collection
                    .AddConfiguredTypes()
                    .AddConfiguredTypes("subTypes", "sub");

            }).Build();


            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class3>>();

            Assert.AreEqual(1, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1A", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.IsNotNull(c1OptionsMonitor.Get("C1A"));
            Assert.IsNotNull(c1OptionsMonitor.Get("C1B"));

            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2A", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.IsNotNull(c2OptionsMonitor.Get("C2A"));
            Assert.IsNotNull(c2OptionsMonitor.Get("C2B"));

            Assert.AreEqual(0, c3OptionsMonitor.GetKeys().Count);
            Assert.IsNotNull(c3OptionsMonitor.Get("C3A"));
            Assert.IsNotNull(c3OptionsMonitor.Get("C3B"));
            // not configured
            Assert.IsNotNull(c3OptionsMonitor.Get("C3C"));
            c3OptionsMonitor.Get("C3C").Property3 = "ValueC3C";

            Assert.AreEqual("Value1A", c1OptionsMonitor.Get("C1A").Property1);
            Assert.IsNull(c1OptionsMonitor.Get("C1B").Property1);

            Assert.AreEqual("Value2A", c2OptionsMonitor.Get("C2A").Property2);
            Assert.IsNull(c2OptionsMonitor.Get("C2B").Property2);

            Assert.IsNull(c3OptionsMonitor.Get("C3A").Property3);
            Assert.IsNull(c3OptionsMonitor.Get("C3B").Property3);

            Assert.IsNotNull(c1OptionsMonitor.Get("sub", "SC1A"));
            Assert.AreEqual("ValueS1A", c1OptionsMonitor.Get("sub", "SC1A").Property1);

            Assert.IsNotNull(c2OptionsMonitor.Get("sub", "SC2A"));
            Assert.AreEqual("ValueS2A", c2OptionsMonitor.Get("sub", "SC2A").Property2);

            var onChangeResult = new List<string>();
            var onChangeResult1 = new Dictionary<string, Class1>();
            var onChangeResult2 = new Dictionary<string, Class2>();
            var onChangeResult3 = new Dictionary<string, Class3>();

            var waitHandle = new AutoResetEvent(false);
            var waitHandle1 = new AutoResetEvent(false);
            var waitHandle2 = new AutoResetEvent(false);
            var waitHandle3 = new AutoResetEvent(false);

            var disp = c1OptionsMonitor.OnChange(name =>
            {
                onChangeResult.Add(name);
                if (onChangeResult.Count < 2) return;
                waitHandle.Set();
            });

            var disp1 = c1OptionsMonitor.OnChange((class1, name, key) =>
            {
                onChangeResult1.Add($"{name}:{key}", class1);
                if (onChangeResult1.Count < 3) return;
                waitHandle1.Set();
            });

            var disp2 = c2OptionsMonitor.OnChange((class2, name, key) =>
            {
                onChangeResult2.Add($"{name}:{key}", class2);
                if (onChangeResult2.Count < 3) return;
                waitHandle2.Set();
            });

            var disp3 = c3OptionsMonitor.OnChange((class3, name, key) =>
            {
                onChangeResult3.Add($"{name}:{key}", class3);
                if (onChangeResult3.Count < 2) return;
                waitHandle3.Set();
            });

            File.Copy(testSettingsPath1, testSettingsPath2, true);
            Assert.IsTrue(WaitHandle.WaitAll(new WaitHandle[] { waitHandle, waitHandle1, waitHandle2, waitHandle3 }, 300));

            Assert.AreEqual(2, onChangeResult.Count);
            Assert.AreEqual(3, onChangeResult1.Count);
            Assert.AreEqual(3, onChangeResult2.Count);
            Assert.AreEqual(3, onChangeResult3.Count);

            Assert.IsTrue(onChangeResult.Contains(Options.DefaultName));
            Assert.IsTrue(onChangeResult.Contains("sub"));

            Assert.IsTrue(onChangeResult1.TryGetValue(":C1A", out var c1A) && c1A.Property1 == "Value1AX");
            Assert.IsTrue(onChangeResult1.TryGetValue(":C1B", out var c1B) && c1B.Property1 == "Value1BX");
            Assert.IsTrue(onChangeResult1.TryGetValue("sub:SC1A", out var sC1A) && sC1A.Property1 == "ValueS1A");

            Assert.IsTrue(onChangeResult2.TryGetValue(":C2A", out var c2A) && c2A.Property2 == "Value2AX");
            Assert.IsTrue(onChangeResult2.TryGetValue(":C2B", out var c2B) && c2B.Property2 == "Value2BX");
            Assert.IsTrue(onChangeResult2.TryGetValue("sub:SC2A", out var sSc2A) && sSc2A.Property2 == "ValueS2A");

            Assert.IsTrue(onChangeResult3.TryGetValue(":C3A", out var c3A) && c3A.Property3 == "Value3AX");
            Assert.IsTrue(onChangeResult3.TryGetValue(":C3B", out var c3B) && c3B.Property3 == "Value3BX");
            Assert.IsTrue(onChangeResult3.TryGetValue(":C3C", out var c3C) && c3C.Property3 == null);

            Assert.AreEqual(2, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1A", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("C1B", c1OptionsMonitor.GetKeys().ToList()[1]);
            Assert.AreEqual("Value1AX", c1OptionsMonitor.Get("C1A").Property1);
            Assert.AreEqual("Value1BX", c1OptionsMonitor.Get("C1B").Property1);

            Assert.AreEqual(2, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2A", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("C2B", c2OptionsMonitor.GetKeys().ToList()[1]);
            Assert.AreEqual("Value2AX", c2OptionsMonitor.Get("C2A").Property2);
            Assert.AreEqual("Value2BX", c2OptionsMonitor.Get("C2B").Property2);

            Assert.AreEqual(2, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3A", c3OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("C3B", c3OptionsMonitor.GetKeys().ToList()[1]);
            Assert.AreEqual("Value3AX", c3OptionsMonitor.Get("C3A").Property3);
            Assert.AreEqual("Value3BX", c3OptionsMonitor.Get("C3B").Property3);
            Assert.IsNull(c3OptionsMonitor.Get("C3C").Property3);

            var c3OptionsMonitor2 = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class3>>();
            Assert.IsNull(c3OptionsMonitor2.Get("C3C").Property3);

            disp.Dispose();
            disp1.Dispose();
            disp2.Dispose();
            disp3.Dispose();
        }

        [TestMethod]
        public void TestGenericConfiguredTypesOptionsMonitorInterface()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");
            var testSettingsPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes1.json");
            var testSettingsPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes2.json");
            var testSubSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypesSub.json");

            File.Copy(testSettingsPath, testSettingsPath2, true);

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                    .AddJsonFile(testSettingsPath2, false, true)
                    .AddJsonFile(testSubSettingsPath, false, true);

            }).ConfigureServices(collection =>
            {
                collection
                    .AddConfiguredTypes()
                    .AddConfiguredTypes("subTypes", "sub");

            }).Build();


            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<IInterface1>>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<IInterface2>>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<IInterface3>>();

            Assert.AreEqual(1, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1A", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.IsNotNull(c1OptionsMonitor.Get("C1A"));
            Assert.IsNull(c1OptionsMonitor.Get("C1B"));

            Assert.AreEqual(1, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2A", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.IsNotNull(c2OptionsMonitor.Get("C2A"));
            Assert.IsNull(c2OptionsMonitor.Get("C2B"));

            Assert.AreEqual(0, c3OptionsMonitor.GetKeys().Count);
            Assert.IsNull(c3OptionsMonitor.Get("C3A"));
            Assert.IsNull(c3OptionsMonitor.Get("C3B"));
            // not configured
            Assert.IsNull(c3OptionsMonitor.Get("C3C"));

            Assert.AreEqual("Value1A", c1OptionsMonitor.Get("C1A").Property1);
            Assert.IsNull(c1OptionsMonitor.Get("C1B"));

            Assert.AreEqual("Value2A", c2OptionsMonitor.Get("C2A").Property2);
            Assert.IsNull(c2OptionsMonitor.Get("C2B"));

            Assert.IsNull(c3OptionsMonitor.Get("C3A"));
            Assert.IsNull(c3OptionsMonitor.Get("C3B"));

            Assert.IsNotNull(c1OptionsMonitor.Get("sub", "SC1A"));
            Assert.AreEqual("ValueS1A", c1OptionsMonitor.Get("sub", "SC1A").Property1);

            Assert.IsNotNull(c2OptionsMonitor.Get("sub", "SC2A"));
            Assert.AreEqual("ValueS2A", c2OptionsMonitor.Get("sub", "SC2A").Property2);

            var onChangeResult = new List<string>();
            var onChangeResult1 = new Dictionary<string, IInterface1>();
            var onChangeResult2 = new Dictionary<string, IInterface2>();
            var onChangeResult3 = new Dictionary<string, IInterface3>();

            var waitHandle = new AutoResetEvent(false);
            var waitHandle1 = new AutoResetEvent(false);
            var waitHandle2 = new AutoResetEvent(false);
            var waitHandle3 = new AutoResetEvent(false);

            var disp = c1OptionsMonitor.OnChange(name =>
            {
                onChangeResult.Add(name);
                if (onChangeResult.Count < 2) return;
                waitHandle.Set();
            });

            var disp1 = c1OptionsMonitor.OnChange((class1, name, key) =>
            {
                onChangeResult1.Add($"{name}:{key}", class1);
                if (onChangeResult1.Count < 3) return;
                waitHandle1.Set();
            });

            var disp2 = c2OptionsMonitor.OnChange((class2, name, key) =>
            {
                onChangeResult2.Add($"{name}:{key}", class2);
                if (onChangeResult2.Count < 3) return;
                waitHandle2.Set();
            });

            var disp3 = c3OptionsMonitor.OnChange((class3, name, key) =>
            {
                onChangeResult3.Add($"{name}:{key}", class3);
                if (onChangeResult3.Count < 3) return;
                waitHandle3.Set();
            });

            File.Copy(testSettingsPath1, testSettingsPath2, true);
            Assert.IsTrue(WaitHandle.WaitAll(new WaitHandle[] { waitHandle, waitHandle1, waitHandle2, waitHandle3 }, 300));

            Assert.AreEqual(2, onChangeResult.Count);
            Assert.AreEqual(3, onChangeResult1.Count);
            Assert.AreEqual(3, onChangeResult2.Count);
            Assert.AreEqual(3, onChangeResult3.Count);

            Assert.IsTrue(onChangeResult.Contains(Options.DefaultName));
            Assert.IsTrue(onChangeResult.Contains("sub"));

            Assert.IsTrue(onChangeResult1.TryGetValue(":C1A", out var c1A) && c1A.Property1 == "Value1AX");
            Assert.IsTrue(onChangeResult1.TryGetValue(":C1B", out var c1B) && c1B.Property1 == "Value1BX");
            Assert.IsTrue(onChangeResult1.TryGetValue("sub:SC1A", out var sC1A) && sC1A.Property1 == "ValueS1A");

            Assert.IsTrue(onChangeResult2.TryGetValue(":C2A", out var c2A) && c2A.Property2 == "Value2AX");
            Assert.IsTrue(onChangeResult2.TryGetValue(":C2B", out var c2B) && c2B.Property2 == "Value2BX");
            Assert.IsTrue(onChangeResult2.TryGetValue("sub:SC2A", out var sSc2A) && sSc2A.Property2 == "ValueS2A");

            Assert.IsTrue(onChangeResult3.TryGetValue(":C3A", out var c3A) && c3A.Property3 == "Value3AX");
            Assert.IsTrue(onChangeResult3.TryGetValue(":C3B", out var c3B) && c3B.Property3 == "Value3BX");
            Assert.IsTrue(onChangeResult3.TryGetValue(":C3C", out var c3C) && c3C == null);

            Assert.AreEqual(2, c1OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C1A", c1OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("C1B", c1OptionsMonitor.GetKeys().ToList()[1]);
            Assert.AreEqual("Value1AX", c1OptionsMonitor.Get("C1A").Property1);
            Assert.AreEqual("Value1BX", c1OptionsMonitor.Get("C1B").Property1);

            Assert.AreEqual(2, c2OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C2A", c2OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("C2B", c2OptionsMonitor.GetKeys().ToList()[1]);
            Assert.AreEqual("Value2AX", c2OptionsMonitor.Get("C2A").Property2);
            Assert.AreEqual("Value2BX", c2OptionsMonitor.Get("C2B").Property2);

            Assert.AreEqual(2, c3OptionsMonitor.GetKeys().Count);
            Assert.AreEqual("C3A", c3OptionsMonitor.GetKeys().ToList()[0]);
            Assert.AreEqual("C3B", c3OptionsMonitor.GetKeys().ToList()[1]);
            Assert.AreEqual("Value3AX", c3OptionsMonitor.Get("C3A").Property3);
            Assert.AreEqual("Value3BX", c3OptionsMonitor.Get("C3B").Property3);
            Assert.IsNull(c3OptionsMonitor.Get("C3C"));

            var c3OptionsMonitor2 = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class3>>();
            Assert.IsNull(c3OptionsMonitor2.Get("C3C").Property3);

            disp.Dispose();
            disp1.Dispose();
            disp2.Dispose();
            disp3.Dispose();
        }

        [TestMethod]
        public void TestNonGenericConfiguredTypesOptionsMonitorClass()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");
            var testSettingsPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes1.json");
            var testSettingsPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes2.json");
            var testSubSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypesSub.json");

            File.Copy(testSettingsPath, testSettingsPath2, true);

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                    .AddJsonFile(testSettingsPath2, false, true)
                    .AddJsonFile(testSubSettingsPath, false, true);

            }).ConfigureServices(collection =>
            {
                collection
                    .AddConfiguredTypes()
                    .AddConfiguredTypes("subTypes", "sub");

            }).Build();

            var optionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();

            Assert.AreEqual(1, optionsMonitor.GetKeys<Class1>().Count);
            Assert.AreEqual("C1A", optionsMonitor.GetKeys<Class1>().ToList()[0]);
            Assert.IsNotNull(optionsMonitor.Get<Class1>("C1A"));
            Assert.IsNull(optionsMonitor.Get<Class1>("C1B"));

            Assert.AreEqual(1, optionsMonitor.GetKeys<Class2>().Count);
            Assert.AreEqual("C2A", optionsMonitor.GetKeys<Class2>().ToList()[0]);
            Assert.IsNotNull(optionsMonitor.Get<Class2>("C2A"));
            Assert.IsNull(optionsMonitor.Get<Class2>("C2B"));

            Assert.AreEqual(0, optionsMonitor.GetKeys<Class3>().Count);
            Assert.IsNull(optionsMonitor.Get<Class3>("C3A"));
            Assert.IsNull(optionsMonitor.Get<Class3>("C3B"));
            // not configured
            Assert.IsNull(optionsMonitor.Get<Class3>("C3C"));

            Assert.AreEqual("Value1A", optionsMonitor.Get<Class1>("C1A").Property1);
            Assert.AreEqual("Value2A", optionsMonitor.Get<Class2>("C2A").Property2);

            Assert.IsNotNull(optionsMonitor.Get<Class1>("sub", "SC1A"));
            Assert.AreEqual("ValueS1A", optionsMonitor.Get<Class1>("sub", "SC1A").Property1);

            Assert.IsNotNull(optionsMonitor.Get<Class2>("sub", "SC2A"));
            Assert.AreEqual("ValueS2A", optionsMonitor.Get<Class2>("sub", "SC2A").Property2);

            var onChangeResult = new List<string>();
            var onChangeResult1 = new Dictionary<string, Class1>();
            var onChangeResult2 = new Dictionary<string, Class2>();
            var onChangeResult3 = new Dictionary<string, Class3>();

            var waitHandle = new AutoResetEvent(false);
            var waitHandle1 = new AutoResetEvent(false);
            var waitHandle2 = new AutoResetEvent(false);
            var waitHandle3 = new AutoResetEvent(false);

            var disp = optionsMonitor.OnChange(name =>
            {
                onChangeResult.Add(name);
                if (onChangeResult.Count < 2) return;
                waitHandle.Set();
            });

            var disp1 = optionsMonitor.OnChange<Class1>((class1, name, key) =>
            {
                onChangeResult1.Add($"{name}:{key}", class1);
                if (onChangeResult1.Count < 3) return;
                waitHandle1.Set();
            });

            var disp2 = optionsMonitor.OnChange<Class2>((class2, name, key) =>
            {
                onChangeResult2.Add($"{name}:{key}", class2);
                if (onChangeResult2.Count < 3) return;
                waitHandle2.Set();
            });

            var disp3 = optionsMonitor.OnChange<Class3>((class3, name, key) =>
            {
                onChangeResult3.Add($"{name}:{key}", class3);
                if (onChangeResult3.Count < 2) return;
                waitHandle3.Set();
            });

            File.Copy(testSettingsPath1, testSettingsPath2, true);
            Assert.IsTrue(WaitHandle.WaitAll(new WaitHandle[] { waitHandle, waitHandle1, waitHandle2, waitHandle3 }, 300));


            Assert.AreEqual(2, onChangeResult.Count);
            Assert.AreEqual(3, onChangeResult1.Count);
            Assert.AreEqual(3, onChangeResult2.Count);
            Assert.AreEqual(2, onChangeResult3.Count);

            Assert.IsTrue(onChangeResult.Contains(Options.DefaultName));
            Assert.IsTrue(onChangeResult.Contains("sub"));

            Assert.IsTrue(onChangeResult1.TryGetValue(":C1A", out var c1A) && c1A.Property1 == "Value1AX");
            Assert.IsTrue(onChangeResult1.TryGetValue(":C1B", out var c1B) && c1B.Property1 == "Value1BX");
            Assert.IsTrue(onChangeResult1.TryGetValue("sub:SC1A", out var sC1A) && sC1A.Property1 == "ValueS1A");

            Assert.IsTrue(onChangeResult2.TryGetValue(":C2A", out var c2A) && c2A.Property2 == "Value2AX");
            Assert.IsTrue(onChangeResult2.TryGetValue(":C2B", out var c2B) && c2B.Property2 == "Value2BX");
            Assert.IsTrue(onChangeResult2.TryGetValue("sub:SC2A", out var sSc2A) && sSc2A.Property2 == "ValueS2A");

            Assert.IsTrue(onChangeResult3.TryGetValue(":C3A", out var c3A) && c3A.Property3 == "Value3AX");
            Assert.IsTrue(onChangeResult3.TryGetValue(":C3B", out var c3B) && c3B.Property3 == "Value3BX");
            Assert.IsFalse(onChangeResult3.TryGetValue(":C3C", out _));

            Assert.AreEqual(2, optionsMonitor.GetKeys<Class1>().Count);
            Assert.AreEqual("C1A", optionsMonitor.GetKeys<Class1>().ToList()[0]);
            Assert.AreEqual("C1B", optionsMonitor.GetKeys<Class1>().ToList()[1]);
            Assert.AreEqual("Value1AX", optionsMonitor.Get<Class1>("C1A").Property1);
            Assert.AreEqual("Value1BX", optionsMonitor.Get<Class1>("C1B").Property1);

            Assert.AreEqual(2, optionsMonitor.GetKeys<Class2>().Count);
            Assert.AreEqual("C2A", optionsMonitor.GetKeys<Class2>().ToList()[0]);
            Assert.AreEqual("C2B", optionsMonitor.GetKeys<Class2>().ToList()[1]);
            Assert.AreEqual("Value2AX", optionsMonitor.Get<Class2>("C2A").Property2);
            Assert.AreEqual("Value2BX", optionsMonitor.Get<Class2>("C2B").Property2);

            Assert.AreEqual(2, optionsMonitor.GetKeys<Class3>().Count);
            Assert.AreEqual("C3A", optionsMonitor.GetKeys<Class3>().ToList()[0]);
            Assert.AreEqual("C3B", optionsMonitor.GetKeys<Class3>().ToList()[1]);

            Assert.AreEqual("Value3AX", optionsMonitor.Get<Class3>("C3A").Property3);
            Assert.AreEqual("Value3BX", optionsMonitor.Get<Class3>("C3B").Property3);
            Assert.IsNull(optionsMonitor.Get<Class3>("C3C"));

            var c3OptionsMonitor2 = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();
            Assert.IsNull(c3OptionsMonitor2.Get<Class3>("C3C"));

            disp.Dispose();
            disp1.Dispose();
            disp2.Dispose();
            disp3.Dispose();
        }

        [TestMethod]
        public void TestNonGenericConfiguredTypesOptionsMonitorInterface()
        {
            var testSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes.json");
            var testSettingsPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes1.json");
            var testSettingsPath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypes2.json");
            var testSubSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appTypesSub.json");

            File.Copy(testSettingsPath, testSettingsPath2, true);

            var host = new HostBuilder().ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder
                    .AddJsonFile(testSettingsPath2, false, true)
                    .AddJsonFile(testSubSettingsPath, false, true);

            }).ConfigureServices(collection =>
            {
                collection
                    .AddConfiguredTypes()
                    .AddConfiguredTypes("subTypes", "sub");

            }).Build();


            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();
            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();
            var c3OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();

            Assert.IsNotNull(c1OptionsMonitor.Get<IInterface1>("C1A"));
            Assert.IsNull(c1OptionsMonitor.Get<IInterface1>("C1B"));

            Assert.IsNotNull(c2OptionsMonitor.Get<IInterface2>("C2A"));
            Assert.IsNull(c2OptionsMonitor.Get<IInterface2>("C2B"));

            Assert.IsNull(c3OptionsMonitor.Get<IInterface3>("C3A"));
            Assert.IsNull(c3OptionsMonitor.Get<IInterface3>("C3B"));
            // not configured
            Assert.IsNull(c3OptionsMonitor.Get<IInterface3>("C3C"));

            Assert.AreEqual("Value1A", c1OptionsMonitor.Get<IInterface1>("C1A").Property1);
            Assert.IsNull(c1OptionsMonitor.Get<IInterface1>("C1B"));

            Assert.AreEqual("Value2A", c2OptionsMonitor.Get<IInterface2>("C2A").Property2);
            Assert.IsNull(c2OptionsMonitor.Get<IInterface2>("C2B"));

            Assert.IsNull(c3OptionsMonitor.Get<IInterface3>("C3A"));
            Assert.IsNull(c3OptionsMonitor.Get<IInterface3>("C3B"));

            Assert.IsNotNull(c1OptionsMonitor.Get<IInterface1>("sub", "SC1A"));
            Assert.AreEqual("ValueS1A", c1OptionsMonitor.Get<IInterface1>("sub", "SC1A").Property1);

            Assert.IsNotNull(c2OptionsMonitor.Get<IInterface2>("sub", "SC2A"));
            Assert.AreEqual("ValueS2A", c2OptionsMonitor.Get<IInterface2>("sub", "SC2A").Property2);

            var onChangeResult = new List<string>();
            var onChangeResult1 = new Dictionary<string, IInterface1>();
            var onChangeResult2 = new Dictionary<string, IInterface2>();
            var onChangeResult3 = new Dictionary<string, IInterface3>();

            var waitHandle = new AutoResetEvent(false);
            var waitHandle1 = new AutoResetEvent(false);
            var waitHandle2 = new AutoResetEvent(false);
            var waitHandle3 = new AutoResetEvent(false);

            var disp = c1OptionsMonitor.OnChange(name =>
            {
                onChangeResult.Add(name);
                if (onChangeResult.Count < 2) return;
                waitHandle.Set();
            });

            var disp1 = c1OptionsMonitor.OnChange<Class1>((class1, name, key) =>
            {
                onChangeResult1.Add($"{name}:{key}", class1);
                if (onChangeResult1.Count < 3) return;
                waitHandle1.Set();
            });

            var disp2 = c2OptionsMonitor.OnChange<Class2>((class2, name, key) =>
            {
                onChangeResult2.Add($"{name}:{key}", class2);
                if (onChangeResult2.Count < 3) return;
                waitHandle2.Set();
            });

            var disp3 = c3OptionsMonitor.OnChange<Class3>((class3, name, key) =>
            {
                onChangeResult3.Add($"{name}:{key}", class3);
                if (onChangeResult3.Count < 2) return;
                waitHandle3.Set();
            });

            File.Copy(testSettingsPath1, testSettingsPath2, true);
            Assert.IsTrue(WaitHandle.WaitAll(new WaitHandle[] { waitHandle, waitHandle1, waitHandle2, waitHandle3 }, 300)); 

            Assert.AreEqual(2, onChangeResult.Count);
            Assert.AreEqual(3, onChangeResult1.Count);
            Assert.AreEqual(3, onChangeResult2.Count);
            Assert.AreEqual(2, onChangeResult3.Count);

            Assert.IsTrue(onChangeResult.Contains(Options.DefaultName));
            Assert.IsTrue(onChangeResult.Contains("sub"));

            Assert.IsTrue(onChangeResult1.TryGetValue(":C1A", out var c1A) && c1A.Property1 == "Value1AX");
            Assert.IsTrue(onChangeResult1.TryGetValue(":C1B", out var c1B) && c1B.Property1 == "Value1BX");
            Assert.IsTrue(onChangeResult1.TryGetValue("sub:SC1A", out var sC1A) && sC1A.Property1 == "ValueS1A");

            Assert.IsTrue(onChangeResult2.TryGetValue(":C2A", out var c2A) && c2A.Property2 == "Value2AX");
            Assert.IsTrue(onChangeResult2.TryGetValue(":C2B", out var c2B) && c2B.Property2 == "Value2BX");
            Assert.IsTrue(onChangeResult2.TryGetValue("sub:SC2A", out var sSc2A) && sSc2A.Property2 == "ValueS2A");

            Assert.IsTrue(onChangeResult3.TryGetValue(":C3A", out var c3A) && c3A.Property3 == "Value3AX");
            Assert.IsTrue(onChangeResult3.TryGetValue(":C3B", out var c3B) && c3B.Property3 == "Value3BX");
            Assert.IsFalse(onChangeResult3.TryGetValue(":C3C", out _));

            Assert.AreEqual("Value1AX", c1OptionsMonitor.Get<IInterface1>("C1A").Property1);
            Assert.AreEqual("Value1BX", c1OptionsMonitor.Get<IInterface1>("C1B").Property1);

            Assert.AreEqual("Value2AX", c2OptionsMonitor.Get<IInterface2>("C2A").Property2);
            Assert.AreEqual("Value2BX", c2OptionsMonitor.Get<IInterface2>("C2B").Property2);

            Assert.AreEqual("Value3AX", c3OptionsMonitor.Get<IInterface3>("C3A").Property3);
            Assert.AreEqual("Value3BX", c3OptionsMonitor.Get<IInterface3>("C3B").Property3);
            Assert.IsNull(c3OptionsMonitor.Get<IInterface3>("C3C"));

            var c3OptionsMonitor2 = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor>();
            Assert.IsNull(c3OptionsMonitor2.Get<IInterface3>("C3C"));

            disp.Dispose();
            disp1.Dispose();
            disp2.Dispose();
            disp3.Dispose();
        }
    }
}
