using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging.File.Benchmarks
{
    internal class Program
    {
        private static readonly object syncObj = new();

        private static void Main()
        {

#if RELEASE
            BenchmarkRunner.Run<FileLoggingBenchmark>();
#endif

            Console.WriteLine("Press [ENTER] to start long running test.");
            Console.ReadLine();

            LongRunningTest("{baseDirectory}/Logging/FileLoggingBenchmark1.log", false);
            LongRunningTest("{baseDirectory}/Logging/FileLoggingBenchmark2.log", true);

            Console.WriteLine("Press [ENTER] to get rem free.");
            Console.ReadLine();
            GC.Collect();

            Console.WriteLine("Press [ENTER] to exit.");
            Console.ReadLine();
        }

        public static void LongRunningTest(string fileNameTemplate, bool parameterized)
        {
            lock (syncObj)
            {
                Console.WriteLine();
                Console.WriteLine($"// Start LongRunningTest: fileNameTemplate={fileNameTemplate}, parameterized={parameterized}");

                var loggerSinkProvider = new LoggerSinkProvider();

                loggerSinkProvider.AddOrUpdateLoggerSink(new FileSink
                {
                    Categories = new[] { "test" },

                    LogLevel = LogLevel.Debug,
                    Name = "Unit Test Sink",
                    OverrideExistingFile = true,
                    MaxFileSize = 500_000_000,
                    FileNameTemplate = fileNameTemplate
                });

                var logger = loggerSinkProvider.CreateLogger("test");

                logger.LogDebug("Hello, file logger! Init");
                var now = DateTime.Now;

                for (var i = 0; i < 1000000; ++i)
                {
                    if (parameterized)
                    {
                        logger.LogDebug("Hello, file logger! {index}", i);
                    }
                    else
                    {
                        logger.LogDebug($"Hello, file logger! {i}");
                    }
                }

                var diff = DateTime.Now - now;

                logger.LogDebug("ElapsedMilliseconds: {elapsedMilliseconds}", diff.TotalMilliseconds);
                Console.WriteLine($"* ElapsedMilliseconds: {diff.TotalMilliseconds}");

                now = DateTime.Now;

                var waitOneResult = loggerSinkProvider.WaitOne(TimeSpan.FromSeconds(300));
                var diff2 = DateTime.Now - now;
                var total = diff + diff2;

                logger.LogDebug($"ElapsedSeconds={diff2.TotalMilliseconds}, Total={total.TotalMilliseconds}, WaitOneResult={waitOneResult}");
                Console.WriteLine($"* ElapsedSeconds={diff2.TotalMilliseconds}, Total={total.TotalMilliseconds}, WaitOneResult={waitOneResult}");

                loggerSinkProvider.Dispose();
            }
        }
    }
}
