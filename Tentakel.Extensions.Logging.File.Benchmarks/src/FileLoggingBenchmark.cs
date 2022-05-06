using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Abstractions;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging.File.Benchmarks
{
    [MemoryDiagnoser]
    public class FileLoggingBenchmark
    {
        private readonly LoggerSinkProvider _loggerSinkProvider = new();

        [GlobalSetup]
        public void GlobalSetup()
        {
            this._loggerSinkProvider.AddOrUpdateLoggerSink(new FileSink
            {
                Categories = new[] { "ParameterizedLogFormat", "UnparameterizedLogFormat" },
                LogLevel = LogLevel.Debug,
                Name = "Unit Test Sink",
                OverrideExistingFile = true,
                MaxFileSize = 500_000_000,
                FileNameTemplate = "{baseDirectory}/Logging/FileLoggingBenchmark.log"
            });
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            var now = DateTime.Now;
            var waitOneResult = this._loggerSinkProvider.WaitOne(TimeSpan.FromSeconds(30));
            var totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;

            Console.WriteLine("===============================================");
            Console.WriteLine($"// WaitOne takes {totalMilliseconds} ms. waitOneResult={waitOneResult}");
            Console.WriteLine("===============================================");
        }

        private ILogger CreateLogger(string categoryName)
        {
            return this._loggerSinkProvider.CreateLogger(categoryName);
        }

        [Benchmark(Baseline = true)]
        public void ParameterizedLogFormat()
        {
            var logger = this.CreateLogger("ParameterizedLogFormat");

            logger.LogDebug("Hello, file logger! Init");

            var now = DateTime.Now;

            for (var i = 0; i < 100; ++i)
            {
                logger.LogDebug("Hello, file logger! {dateTime} {index}", DateTime.Now.ToString("yyyy-MM-DD HH:mm:ss fff"), i);
            }

            var totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;

            logger.LogDebug("TotalMilliseconds: {totalMilliseconds}", totalMilliseconds);
        }

        [Benchmark]
        public void UnparameterizedLogFormat()
        {
            var logger = this.CreateLogger("UnparameterizedLogFormat");

            logger.LogDebug("Hello, file logger! Init");

            var now = DateTime.Now;

            for (var i = 0; i < 100; ++i)
            {
                logger.LogDebug($"Hello, file logger! {DateTime.Now:yyyy-MM-DD HH:mm:ss fff} {i}");
            }

            var totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;

            logger.LogDebug($"TotalMilliseconds: {totalMilliseconds}");
        }

        [Benchmark]
        public void ParameterizedLogFormatAddCallerInfos()
        {
            var logger = this.CreateLogger("ParameterizedLogFormat");

            logger.AddCallerInfos().LogDebug("Hello, file logger! Init");

            var now = DateTime.Now;

            for (var i = 0; i < 100; ++i)
            {
                logger.AddCallerInfos().LogDebug("Hello, file logger! {dateTime} {index}", DateTime.Now.ToString("yyyy-MM-DD HH:mm:ss fff"), i);
            }

            var totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;

            logger.AddCallerInfos().LogDebug("TotalMilliseconds: {totalMilliseconds}", totalMilliseconds);
        }

        [Benchmark]
        public void UnparameterizedLogFormatAddCallerInfos()
        {
            var logger = this.CreateLogger("UnparameterizedLogFormat");

            logger.AddCallerInfos().LogDebug("Hello, file logger! Init");

            var now = DateTime.Now;

            for (var i = 0; i < 100; ++i)
            {
                logger.AddCallerInfos().LogDebug($"Hello, file logger! {DateTime.Now:yyyy-MM-DD HH:mm:ss fff} {i}");
            }

            var totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;

            logger.AddCallerInfos().LogDebug($"TotalMilliseconds: {totalMilliseconds}");
        }
    } 
    
    
}