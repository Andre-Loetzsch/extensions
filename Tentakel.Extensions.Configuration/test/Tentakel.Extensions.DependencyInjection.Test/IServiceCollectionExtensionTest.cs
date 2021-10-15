using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Configuration.Test.Common;

namespace Tentakel.Extensions.DependencyInjection.Test
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class IServiceCollectionExtensionTest
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
        public void TestAddTransientConfiguredType()
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
            Assert.AreSame(serviceB, serviceB);

            var services = host.Services.GetRequiredServices<IInterface1>().ToList();

            Assert.AreEqual(2, services.Count);
            Assert.AreEqual("Value1A", services[0].Property1);
            Assert.AreEqual("Value1B", services[1].Property1);
        }






        // TODO move to class
        [TestMethod]
        public void TestCreateHostConfigurationFromJsonStream1()
        {
            var host = new HostBuilder().ConfigureServices(collection =>
            {
                collection.AddConfiguredTypes();
            }).Build();

            var c1OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class1>>();

            var c2OptionsMonitor = host.Services.GetRequiredService<IConfiguredTypesOptionsMonitor<Class2>>();

            Assert.IsNotNull(c1OptionsMonitor.Get("c1"));
            Assert.IsNotNull(c2OptionsMonitor.Get("c2"));



        }
    }
}
