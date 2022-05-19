using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration.Tests;

// ReSharper disable All

namespace Tentakel.Extensions.Configuration.Test
{
    [TestClass]
    public class ConfiguredTypesTest
    {
        [TestMethod]
        public void TestTypeLoadException()
        {
            var configuredTypes = new ConfiguredTypes
            {
                ConfigurationRoot = new ConfigurationRoot(new List<IConfigurationProvider>()),
                ["EXC"] = new ConfiguredType { Type = typeof(Class3).FullName }
            };

            var c3 = configuredTypes.GetAll<Class3>().ToList();
            Assert.AreEqual(0, c3.Count);

            var exc = configuredTypes.GetAll<Exception>().ToList();
            Assert.AreEqual(1, exc.Count);
            Assert.AreEqual(typeof(TypeLoadException), exc[0].GetType());
        }

        [TestMethod]
        public void TestGetAll()
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
        }

        [TestMethod]
        public void TestGet()
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
            configuredTypes["EXC"] = new ConfiguredType { Type = typeof(Class3).FullName };

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

            Assert.IsTrue(configuredTypes.TryGet<IInterface1>("C1_2_3", out i1));
            Assert.AreEqual(typeof(Class1_2_3), i1.GetType());
            Assert.AreEqual("Value1", i1.Property1);

            Assert.IsTrue(configuredTypes.TryGet<IInterface2>("C1_2_3", out i2));
            Assert.AreEqual(typeof(Class1_2_3), i2.GetType());
            Assert.AreEqual("Value2", i2.Property2);

            Assert.IsTrue(configuredTypes.TryGet<IInterface3>("C1_2_3", out i3));
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

            var exc = configuredTypes.Get<Exception>("EXC");
            Assert.IsNotNull(exc);
            Assert.AreEqual(typeof(TypeLoadException), exc.GetType());
        }
     
        [TestMethod]
        public void TestGetDefaultOnject()
        {
            var c1 = new ConfiguredTypes().Get<object>("C1");
            Assert.IsNull(c1);
        }

        [TestMethod]
        public void TestGetAllOnjects()
        {
            var objList = new ConfiguredTypes().GetAll<object>().ToList();
            Assert.IsNotNull(objList);
            Assert.AreEqual(0, objList.Count);
        }

        [TestMethod]
        public void TestTryGetFalse()
        {
            Assert.IsFalse(new ConfiguredTypes().TryGet<object>("C1", out _));
        }
    }
}