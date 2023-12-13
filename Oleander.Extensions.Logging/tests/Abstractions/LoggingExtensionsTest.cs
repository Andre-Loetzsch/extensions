using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oleander.Extensions.Logging.Abstractions;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Extensions.Logging.Tests.Abstractions
{
    [TestClass]
    public class LoggingExtensionsTest
    {
        [TestMethod]
        public void TestAddCallerInfos()
        {
            var loggerSinkProvider = new LoggerSinkProvider();
            var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = new[] { "Test" } };

            loggerSinkProvider.AddOrUpdateLoggerSinks(new[] { loggerSink });
            var logger = loggerSinkProvider.CreateLogger("Test");

            logger.AddCallerInfos().LogInformation("This is test message 1.");
            Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));

            Assert.AreEqual(1, loggerSink.Entries.Count);
            Assert.IsTrue(loggerSink.Entries[0].Source.StartsWith($"{this.GetType().Namespace}.{this.GetType().Name}.TestAddCallerInfos["));
        }

        [TestMethod]
        public void TestAddCallerInfosWithArguments()
        {
            var loggerSinkProvider = new LoggerSinkProvider();
            var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = new[] { "Test" } };

            loggerSinkProvider.AddOrUpdateLoggerSinks(new[] { loggerSink });
            var logger = loggerSinkProvider.CreateLogger("Test");

            logger.AddCallerInfos("Tests/Abstractions", "AddCaller", 123).LogInformation("This is test message 1.");
            Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));

            Assert.AreEqual(1, loggerSink.Entries.Count);
            Assert.AreEqual("Tests.Abstractions.AddCaller[123]", loggerSink.Entries[0].Source);
        }
    }
}
