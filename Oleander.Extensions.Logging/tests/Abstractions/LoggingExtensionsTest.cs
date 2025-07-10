using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oleander.Extensions.Logging.Abstractions;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Extensions.Logging.Tests.Abstractions;

[TestClass]
public class LoggingExtensionsTest
{
    [TestMethod]
    public void TestAddCallerInfos()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test");

        logger.AddCallerInfos().LogInformation("This is test message 1.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));
        Assert.AreEqual(1, loggerSink.Entries.Count);
        Assert.AreEqual(loggerSink.Entries[0].Source, $"{this.GetType().Namespace}.{this.GetType().Name}.TestAddCallerInfos[21]");
    }

    [TestMethod]
    public void TestCallerInfosMayOnlyBeUsedOnce()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test").AddCallerInfos();

        logger.LogInformation("This is test message 1.");
        logger.LogInformation("This is test message 2.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));

        Assert.AreEqual(2, loggerSink.Entries.Count);
        Assert.AreEqual(loggerSink.Entries[0].Source, $"{this.GetType().Namespace}.{this.GetType().Name}.TestCallerInfosMayOnlyBeUsedOnce[35]");
        Assert.AreEqual(loggerSink.Entries[1].Source, $"{this.GetType().Namespace}.{this.GetType().Name}.TestCallerInfosMayOnlyBeUsedOnce");
    }

    [TestMethod]
    public void TestAddCallerInfosWithArguments()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test");

        logger.AddCallerInfos("Tests/Abstractions", "AddCaller", 123).LogInformation("This is test message 1.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));
        Assert.AreEqual(1, loggerSink.Entries.Count);
        Assert.AreEqual("Tests.Abstractions.AddCaller[123]", loggerSink.Entries[0].Source);
    }

    [TestMethod]
    public void TestAddCorrelationId()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test").AddAttribute("1", 1);

        logger.AddCorrelationId(new KeyValuePair<string, int>("X", 1234)).LogInformation("This is test message 1.");
        logger.LogInformation("This is test message 2.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));

        Assert.AreEqual(2, loggerSink.Entries.Count);
        Assert.AreEqual(new KeyValuePair<string, int>("X", 1234), loggerSink.Entries[0].Correlation);
        Assert.IsTrue(loggerSink.Entries[1].Correlation is int);
    }

    [TestMethod]
    public void TestAddCorrelationIdMultipleTimes()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test");

        var logger2 = logger
            .AddCorrelationId("67")
            .AddCorrelationId("76");

        logger
            .AddCorrelationId("ABC")
            .AddCorrelationId(new KeyValuePair<string, int>("X", 1234))
            .LogInformation("This is test message 1.");

        logger.LogInformation("This is test message 2.");
        logger2.LogInformation("This is test message 3.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));
        Assert.AreEqual(3, loggerSink.Entries.Count);
        Assert.AreEqual(new KeyValuePair<string, int>("X", 1234), loggerSink.Entries[0].Correlation);
        Assert.IsTrue(loggerSink.Entries[1].Correlation is int);
        Assert.AreEqual("76", loggerSink.Entries[2].Correlation);
    }

    [TestMethod]
    public void TestCreateCorrelationLogger()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test")
            .AddCorrelationId(new KeyValuePair<string, int>("X", 1234));

        logger.LogInformation("This is test message 1.");
        logger.LogInformation("This is test message 2.");
        logger.AddCorrelationId("TEST").LogInformation("This is test message 3.");
        logger.LogInformation("This is test message 4.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));
        Assert.AreEqual(4, loggerSink.Entries.Count);
        Assert.AreEqual(new KeyValuePair<string, int>("X", 1234), loggerSink.Entries[0].Correlation);
        Assert.AreEqual(new KeyValuePair<string, int>("X", 1234), loggerSink.Entries[1].Correlation);
        Assert.AreEqual("TEST", loggerSink.Entries[2].Correlation);
        Assert.AreEqual(new KeyValuePair<string, int>("X", 1234), loggerSink.Entries[3].Correlation);
    }

    [TestMethod]
    public void TestAddAttributes()
    {
        var loggerSinkProvider = new LoggerSinkProvider();
        var loggerSink = new FakeLoggerSink { Name = "S1", LogLevel = LogLevel.Information, Categories = ["Test"] };

        loggerSinkProvider.AddOrUpdateLoggerSinks([loggerSink]);
        var logger = loggerSinkProvider.CreateLogger("Test");

        logger
            .AddAttribute("key1", 1)
            .AddAttribute("key2", 2)
            .AddAttribute("key3", 3)
            .AddAttribute("key4", 4)
            .LogInformation("This is test message 1.");

        logger.LogInformation("This is test message 1.");

        Assert.AreEqual(0, loggerSinkProvider.WaitOne(3000));
        Assert.AreEqual(2, loggerSink.Entries.Count);
        Assert.IsTrue(loggerSink.Entries[0].Attributes.TryGetValue("key1", out var value));
        Assert.AreEqual(1, value);
        Assert.IsTrue(loggerSink.Entries[0].Attributes.TryGetValue("key2", out value));
        Assert.AreEqual(2, value);
        Assert.IsTrue(loggerSink.Entries[0].Attributes.TryGetValue("key3", out value));
        Assert.AreEqual(3, value);
        Assert.IsTrue(loggerSink.Entries[0].Attributes.TryGetValue("key4", out value));
        Assert.AreEqual(4, value);

        Assert.IsFalse(loggerSink.Entries[1].Attributes.TryGetValue("key1", out value));
        Assert.IsFalse(loggerSink.Entries[1].Attributes.TryGetValue("key2", out value));
        Assert.IsFalse(loggerSink.Entries[1].Attributes.TryGetValue("key3", out value));
        Assert.IsFalse(loggerSink.Entries[1].Attributes.TryGetValue("key4", out value));
    }
}

