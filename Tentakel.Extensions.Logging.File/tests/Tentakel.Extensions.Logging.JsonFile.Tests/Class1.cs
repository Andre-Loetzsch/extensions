using Tentakel.Extensions.Logging.Providers;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Tentakel.Extensions.Logging.JsonFile.Tests
{
    public class Class1
    {
        [Fact]
        public void Test()
        {

            var loggerSinkProvider = new LoggerSinkProvider();

            loggerSinkProvider.AddOrUpdateLoggerSink(new FileSink
            {
                Categories = new[] { "test" },
                LogLevel = LogLevel.Debug,
                Name = "Unit Test Sink",
                OverrideExistingFile = true,
                MaxFileSize = 500_000_000,
                FileNameTemplate = "{baseDirectory}/Logging/{dateTime:yyyy}/{dateTime:MM}/{processName}/{dateTime:yyyy-MM-dd}.Trace.log"
            });

            ILogger logger = loggerSinkProvider.CreateLogger("test");

            logger.LogDebug("Hello, file logger! Init");
            var now = DateTime.Now;

            for (var i = 0; i < 1000000; ++i)
            {
                //logger.AddCallerInfos().LogDebug("Hello, file logger! {index}", i);
                logger.LogDebug("Hello, file logger! {index}", i);
            }

            var diff = DateTime.Now - now;

            logger.LogDebug("ElapsedMilliseconds: {elapsedMilliseconds}", diff.TotalMilliseconds);
            Debug.WriteLine($"ElapsedMilliseconds: {diff.TotalMilliseconds}");

            now = DateTime.Now;

            Assert.Equal(0, loggerSinkProvider.WaitOn(TimeSpan.FromSeconds(300)));

            var diff2 = DateTime.Now - now;
            var total = diff + diff2;

            logger.LogDebug($"ElapsedSeconds: {diff2.TotalSeconds} Total: {total.TotalSeconds}");
            Debug.WriteLine($"ElapsedSeconds: {diff2.TotalSeconds} Total: {total.TotalSeconds}");
        }
    }
}