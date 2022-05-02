using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration.Json.Tests.Common;

namespace Tentakel.Extensions.Configuration.Json.Tests
{
    [TestClass]
    public class ConfiguredTypesConfigurationTest
    {
        [TestMethod]
        public void TestMergedConfigurations()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSettings1.json"), jsonStr);

            dict = new()
            {
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };

            jsonStr = JsonStringBuilder.Build(dict, "types");
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSettings2.json"), jsonStr);

            IConfiguration configuration1 = new ConfigurationBuilder()
                .AddWritableJsonFile("TestSettings1.json").Build();

            IConfiguration configuration2 = new ConfigurationBuilder()
                .AddWritableJsonFile("TestSettings2.json").Build();


            var value = configuration2.GetSection("C3:Property3").Value;
            Assert.AreEqual("Value3", value);

            configuration2.GetSection("C3:Property3").Value = "changed from test";
            
            configuration2.Set("C4", new Class1_2_3
            {
                Property1 = "P1",
                Property2 = "P2",
                Property3 = "P3"
            });

            configuration2.Set("NC1", new NestedClass
            {

                C1 = new() { Property1 = "P1" },
                C2 = new() { Property2 = "P2" },

                Sub = new()
                {
                    C1 = new() { Property1 = "SUB1:P1" },
                    C2 = new() { Property2 = "SUB1:P2" },
                }

            });

            //-- test configuration2 --

            var c3 = configuration2.GetSection("C3").Get<Class3>();
            Assert.IsNotNull(c3);
            Assert.AreEqual("changed from test", c3.Property3);

            var c4 = configuration2.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);

            var nc1 = configuration2.GetSection("NC1").Get<NestedClass>();
            Assert.IsNotNull(nc1);
            Assert.AreEqual("P1", nc1.C1.Property1);
            Assert.AreEqual("P2", nc1.C2.Property2);

            //-- test configuration1 + configuration2

            var configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration1)
                .AddConfiguration(configuration2).Build();

            c3 = configuration.GetSection("C3").Get<Class3>();
            Assert.IsNotNull(c3);
            Assert.AreEqual("changed from test", c3.Property3);

            c4 = configuration.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);


            nc1 = configuration.GetSection("NC1").Get<NestedClass>();
            Assert.IsNotNull(nc1);
            Assert.AreEqual("P1", nc1.C1.Property1);
            Assert.AreEqual("P2", nc1.C2.Property2);

            Assert.IsNotNull(nc1.Sub);
            Assert.AreEqual("SUB1:P1", nc1.Sub.C1.Property1);
            Assert.AreEqual("SUB1:P2", nc1.Sub.C2.Property2);

            //-- test TestSettings1.json + TestSettings2.json

            configuration = new ConfigurationBuilder()
                .AddJsonFile("TestSettings1.json")
                .AddJsonFile("TestSettings2.json").Build();

            c3 = configuration.GetSection("C3").Get<Class3>();
            Assert.IsNotNull(c3);
            Assert.AreEqual("changed from test", c3.Property3);

            c4 = configuration.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);


            nc1 = configuration.GetSection("NC1").Get<NestedClass>();
            Assert.IsNotNull(nc1);
            Assert.AreEqual("P1", nc1.C1.Property1);
            Assert.AreEqual("P2", nc1.C2.Property2);

            Assert.IsNotNull(nc1.Sub);
            Assert.AreEqual("SUB1:P1", nc1.Sub.C1.Property1);
            Assert.AreEqual("SUB1:P2", nc1.Sub.C2.Property2);
        }

        [TestMethod]
        public void TestCreateJsonStringFromConfiguredTypes()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" },
                ["EX"] = new ArgumentException("Invalid Argument!") 
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            var buffer = Encoding.UTF8.GetBytes(jsonStr);
            var stream = new MemoryStream();
            
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            var configurationRoot = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            var configuredTypes = configurationRoot.GetSection("types").Get<ConfiguredTypes>();
            configuredTypes.ConfigurationRoot = configurationRoot;

            var c1 = configuredTypes.Get<Class1>("C1");
            Assert.IsNotNull(c1);
            Assert.AreEqual("Value1", c1.Property1);

            var c2 = configuredTypes.Get<Class2>("C2");
            Assert.IsNotNull(c2);
            Assert.AreEqual("Value2", c2.Property2);

            var c3 = configuredTypes.Get<Class3>("C3");
            Assert.IsNotNull(c3);
            Assert.AreEqual("Value3", c3.Property3);

            c1.Property1 = "P1:1";

            // create new IConfigurationRoot

            dict = new(configuredTypes
                .Where(x => x.Value.Instance is { } and not Exception)
                .Select(x => new KeyValuePair<string, object>(x.Key, x.Value.Instance)));

            jsonStr = JsonStringBuilder.Build(dict, "types");
            buffer = Encoding.UTF8.GetBytes(jsonStr);
            stream = new();
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            configurationRoot = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            configuredTypes = configurationRoot.GetSection("types").Get<ConfiguredTypes>();
            configuredTypes.ConfigurationRoot = configurationRoot;

            c1 = configuredTypes.Get<Class1>("C1");
            Assert.IsNotNull(c1);
            Assert.AreEqual("P1:1", c1.Property1);

            c2 = configuredTypes.Get<Class2>("C2");
            Assert.IsNotNull(c2);
            Assert.AreEqual("Value2", c2.Property2);

            c3 = configuredTypes.Get<Class3>("C3");
            Assert.IsNotNull(c3);
            Assert.AreEqual("Value3", c3.Property3);
        }
    }
}
