using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oleander.Extensions.Configuration.Tests;

namespace Oleander.Extensions.Configuration.Json.Tests
{
    [TestClass]
    public class WritableJsonTest
    {
        [TestMethod]
        public void TestWritableJsonFileUpdateAndAdd()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSettings.json"), jsonStr);

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = configurationBuilder.AddWritableJsonFile("TestSettings.json").Build();

            var value = configuration.GetSection("C3:Property3").Value; 
            Assert.AreEqual("Value3", value);

            configuration.GetSection("C3:Property3").Value = "changed from test";
            value = configuration.GetSection("C3:Property3").Value; 
            Assert.AreEqual("changed from test", value);

            configuration.GetSection("C4").Value = JsonSerializer.Serialize(new Class1_2_3
            {
                Property1 = "P1",
                Property2 = "P2",
                Property3 = "P3"
            }, new JsonSerializerOptions { WriteIndented = true });

            var c4 = configuration.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);

            //var content = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSettings.json"));

            configurationBuilder = new ConfigurationBuilder();
            configuration = configurationBuilder.AddJsonFile("TestSettings.json").Build();

            var c3 = configuration.GetSection("C3").Get<Class3>();
            Assert.IsNotNull(c3);
            Assert.AreEqual("changed from test", c3.Property3);

            c4 = configuration.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);
        }

        [TestMethod]
        public void TestWritableJsonSteamUpdateAndAdd()
        {
            var dict = new Dictionary<string, object>
            {
                ["C1"] = new Class1 { Property1 = "Value1" },
                ["C2"] = new Class2 { Property2 = "Value2" },
                ["C3"] = new Class3 { Property3 = "Value3" }
            };

            var jsonStr = JsonStringBuilder.Build(dict, "types");
            var stream = new MemoryStream();
            var buffer = Encoding.UTF8.GetBytes(jsonStr);

            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = configurationBuilder.AddWritableJsonStream(stream).Build();

            var value = configuration.GetSection("C3:Property3").Value; 
            Assert.AreEqual("Value3", value);

            configuration.GetSection("C3:Property3").Value = "changed from test";
            value = configuration.GetSection("C3:Property3").Value; 
            Assert.AreEqual("changed from test", value);

            configuration.Set("C4", new Class1_2_3
            {
                Property1 = "P1",
                Property2 = "P2",
                Property3 = "P3"
            });

            //buffer = new byte[stream.Length];
            //stream.Position = 0;
            //stream.Read(buffer, 0, buffer.Length);
            //stream.Position = 0;

            var c4 = configuration.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);

            //var content = Encoding.UTF8.GetString(buffer);

            configurationBuilder = new ConfigurationBuilder();
            configuration = configurationBuilder.AddWritableJsonStream(stream).Build();

            var c3 = configuration.GetSection("C3").Get<Class3>();
            Assert.IsNotNull(c3);
            Assert.AreEqual("changed from test", c3.Property3);

            c4 = configuration.GetSection("C4").Get<Class1_2_3>();
            Assert.IsNotNull(c4);
            Assert.AreEqual("P1", c4.Property1);
            Assert.AreEqual("P2", c4.Property2);
            Assert.AreEqual("P3", c4.Property3);
        }
    }
}