using Tentakel.Extensions.Logging.Providers;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tentakel.Extensions.Logging.JsonFile.Tests
{
    public class Class1
    {
        [Fact]
        public void Test()
        {
            //var options = new ConfiguredTypesOptionsMonitor<ILoggerSink>();

            var loggerSinkProvider = new LoggerSinkProvider();

            loggerSinkProvider.AddOrUpdateLoggerSink(new JsonFileSink { Categories = new[] { "test" }, LogLevel = LogLevel.Debug, Name = "Unit Test Sink" });

            ILogger logger = loggerSinkProvider.CreateLogger("test");

            var now = DateTime.Now;

            for (var i = 0; i < 1000000; ++i)
            {
                logger.LogDebug1("Hello, file logger!");
                //Thread.Sleep(1);
            }

            var diff = DateTime.Now - now;

            logger.LogDebug("ElapsedMilliseconds: {elapsedMilliseconds}", diff.TotalMilliseconds);
            Debug.WriteLine($"ElapsedMilliseconds: {diff.TotalMilliseconds}");

            now = DateTime.Now;

            Assert.Equal(0, loggerSinkProvider.WaitOn(TimeSpan.FromSeconds(300)));

            var diff2 = DateTime.Now - now;
            var total = diff + diff2;

            Debug.WriteLine($"ElapsedSeconds: {diff2.TotalSeconds} Total: {total.TotalSeconds}");

            for (int i = 0; i < 20; i++)
            {
                GC.Collect(0);
                Thread.Sleep(1000);
            }

        }
    }

    public static class Extensions
    {
        public static void LogDebug1(this ILogger logger, string? message, [CallerMemberName] string member = "", [CallerFilePath] string source = "", params object[] args)
        {
            var list = new List<object>(new object[]{member, source });

            if (args != null)  list.AddRange(args);

            logger.Log(LogLevel.Debug, message, list.ToArray());
        }

        public static void LogDebug(this ILogger logger, Exception? exception, string? message, 
            [CallerMemberName] string member = "", [CallerFilePath] string source = "",  params object?[] args)
        {



            logger.Log(LogLevel.Debug, exception, message, args);
        }

    }
}