using Tentakel.Extensions.Logging.Providers;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections;

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
                logger.AddCallerInfos().LogDebug("Hello, file logger!");
                //logger.LogDebug("Hello, file logger!");
                //logger.LogDebug("{callerFilePath} Hello, file logger!", $"Tentakel.Extensions.Logging.JsonFile.Tests.Class1.Test");

                //Thread.Sleep(1);
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

            for (int i = 0; i < 10; i++)
            {
                GC.Collect(0);
                Thread.Sleep(1000);
            }

            logger.AddCallerInfos().LogError("Test");

            loggerSinkProvider.CreateLogger("test").AddCallerInfos().LogError("Test");

        }
    }

    public static class Extensions
    {
        //public static LoggerMapper LoggerMapper = new LoggerMapper();
        private static object loggerMapperSync = new();

        //public static ILogger Logger<T>(this T logger, [CallerFilePath] string source = "", [CallerMemberName] string member = "") where T : class
        //{

        //    LoggerMapper.Source = $"{typeof(T).Name}.{member}";
        //    return LoggerMapper;
        //}

        //public static ILogger Logger(this Type type, [CallerMemberName] string member = "", [CallerFilePath] string source = "")
        //{

        //    LoggerMapper.Source = $"{type.Name}.{member}";
        //    return LoggerMapper;
        //}


        public static ILogger AddCallerInfos(this ILogger logger,
            [CallerArgumentExpression("logger")] string thisExpression = "",
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {

            lock (loggerMapperSync)
            {
                //var source = callerFilePath;
                var assembly = Assembly.GetCallingAssembly();

                //if (!string.IsNullOrEmpty(assembly.FullName))
                //{
                //    var assemblyName = assembly.FullName.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).First();
                //    var indexOf = callerFilePath.IndexOf(assemblyName);

                //    if (indexOf > 0)
                //    {
                //        source = string
                //            .Join(".", callerFilePath
                //            .Replace("\\", ".")
                //            .Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries)[..^1])[indexOf..];
                //    }
                //}

                var LoggerMapper = new LoggerMapper();

                LoggerMapper.Logger = logger;

                LoggerMapper.LogData.Clear();
                LoggerMapper.LogData["thisExpression"] = thisExpression;
                LoggerMapper.LogData["callerMemberName"] = callerMemberName;
                LoggerMapper.LogData["callerFilePath"] = callerFilePath;

                LoggerMapper.LogData["callerLineNumber"] = callerLineNumber;
                LoggerMapper.LogData["assembly.FullName"] = assembly.FullName ?? string.Empty;

                return LoggerMapper;
            }
            //return new LoggerMapper { Logger = logger, Source = $"{source}.{callerMemberName}[{callerLineNumber}]" };
        }
    }

    public class LoggerMapper : ILogger
    {
        public ILogger Logger { get; set; }
        //public string? Source { get; set; }
        private static readonly Func<FormattedLogValues, Exception?, string> messageFormatter = MessageFormatter;

        private static string MessageFormatter(FormattedLogValues state, Exception? error)
        {
            return state.ToString() ?? "";
        }

        public Dictionary<string, object?> LogData { get; set; } = new();

        public IDisposable BeginScope<TState>(TState state)
        {
            return this.Logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.Logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var values = new List<KeyValuePair<string, object?>>();

            if (state is IReadOnlyList<KeyValuePair<string, object?>> readOnlyList)
            {
                values.AddRange(readOnlyList);
            }

            if (this.LogData != null) values.AddRange(this.LogData );


            var formattedLogValues = new FormattedLogValues(state?.ToString() ?? "", values);

            this.Logger.Log(logLevel, eventId, formattedLogValues, exception, messageFormatter);
        }
    }

    internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object?>>
    {
        private readonly List<KeyValuePair<string, object?>> _values;

        public FormattedLogValues(string logMessage, IEnumerable<KeyValuePair<string, object?>> values)
        {
            this._values = new List<KeyValuePair<string, object?>>(values);
            this.LogMessage = logMessage;
        }

        public KeyValuePair<string, object?> this[int index] => this._values[index];

        public int Count => this._values.Count;

        public string LogMessage { get; }

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._values.GetEnumerator();
        }
        public override string ToString()
        {
            return this.LogMessage;
        }
    }
}