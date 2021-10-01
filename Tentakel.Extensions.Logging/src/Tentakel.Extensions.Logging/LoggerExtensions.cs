using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Performance;

namespace Tentakel.Extensions.Logging
{
    public static class LoggerSinkExtensions
    {
        #region LogDebug

        #region message

        public static void LogDebug(this ILogger logger, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Debug, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region correlation, message

        public static void LogDebug(this ILogger logger, object correlation, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Debug, 0, null, message, logCategory, attributes);
        }

        #endregion

        #endregion

        #region LogTrace

        #region message

        public static void LogTrace(this ILogger logger, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Trace, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region correlation, message

        public static void LogTrace(this ILogger logger, object correlation, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Trace, 0, null, message, logCategory, attributes);
        }

        #endregion

        #endregion

        #region LogInformation

        #region message

        public static void LogInformation(this ILogger logger, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Information, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region correlation, message

        public static void LogInformation(this ILogger logger, object correlation, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Information, 0, null, message, logCategory, attributes);
        }

        #endregion

        #endregion

        #region LogWarning

        #region message

        public static void LogWarning(this ILogger logger, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Warning, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region exception

        public static void LogWarning(this ILogger logger, Exception exception, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Warning, 0, exception, string.Empty, logCategory, attributes);
        }

        #endregion

        #region exception, message

        public static void LogWarning(this ILogger logger, Exception exception, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Warning, 0, exception, message, logCategory, attributes);
        }

        #endregion

        #region correlation, message

        public static void LogWarning(this ILogger logger, object correlation, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Warning, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region correlation, exception

        public static void LogWarning(this ILogger logger, object correlation, Exception exception, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Warning, 0, exception, string.Empty, logCategory, attributes);
        }

        #endregion

        #region correlation, exception, message

        public static void LogWarning(this ILogger logger, object correlation, Exception exception, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Warning, 0, exception, message, logCategory, attributes);
        }

        #endregion

        #endregion

        #region LogError

        #region message

        public static void LogError(this ILogger logger, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Error, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region exception

        public static void LogError(this ILogger logger, Exception exception, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Error, 0, exception, string.Empty, logCategory, attributes);
        }

        #endregion

        #region exception, message

        public static void LogError(this ILogger logger, Exception exception, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Error, 0, exception, message, logCategory, attributes);
        }

        #endregion

        #region correlation, message

        public static void LogError(this ILogger logger, object correlation, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Error, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region correlation, exception

        public static void LogError(this ILogger logger, object correlation, Exception exception, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Error, 0, exception, string.Empty, logCategory, attributes);
        }

        #endregion

        #region correlation, exception, message

        public static void LogError(this ILogger logger, object correlation, Exception exception, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Error, 0, exception, message, logCategory, attributes);
        }

        #endregion

        #endregion

        #region LogCritical

        #region message

        public static void LogCritical(this ILogger logger, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Critical, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region exception

        public static void LogCritical(this ILogger logger, Exception exception, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Critical, 0, exception, string.Empty, logCategory, attributes);
        }

        #endregion

        #region exception, message

        public static void LogCritical(this ILogger logger, Exception exception, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, null, LogLevel.Critical, 0, exception, message, logCategory, attributes);
        }

        #endregion

        #region correlation, message

        public static void LogCritical(this ILogger logger, object correlation, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Critical, 0, null, message, logCategory, attributes);
        }

        #endregion

        #region correlation, exception

        public static void LogCritical(this ILogger logger, object correlation, Exception exception, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Critical, 0, exception, string.Empty, logCategory, attributes);
        }

        #endregion

        #region correlation, exception, message

        public static void LogCritical(this ILogger logger, object correlation, Exception exception, string message, string logCategory = "", params KeyValuePair<string, object>[] attributes)
        {
            Log(logger, correlation, LogLevel.Critical, 0, exception, message, logCategory, attributes);
        }

        #endregion










        #endregion

        #region Log

        public static void Log(this ILogger logger, object correlation, LogLevel logLevel, EventId eventId,
            Exception exception, string message, string logCategory, params KeyValuePair<string, object>[] attributes)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            var logEntry = new LogEntry
            {
                EventId = eventId.Id,
                Exception = exception,
                Message = message,
                State = null,
                LogLevel = logLevel,
                LogCategory = logCategory,
                Attributes = new Dictionary<string, object>(attributes),
                Correlation = correlation,

                // TODO Fill properties!

                LogicalOperationId = "",
                LogicalOperationStack = "",
                LogicalOperationStackNesting = 0,
                Source = "",
                StackTrace = new StackTrace() 

            };

            logger.Log(logEntry);
        }

        public static void Log(this ILogger logger, LogEntry logEntry)
        {
            logger.Log(logEntry.LogLevel, new EventId(logEntry.EventId), logEntry, logEntry.Exception, (entry, _) => entry.ToString());
        }

        #endregion




        //public static IDisposable BeginScope(this ILogger logger, object scopeId, string messageFormat, params object[] args)
        //{
        //    if (logger == null) throw new ArgumentNullException(nameof(logger));

        //    var list = new List<object>(args);

        //    list.Insert(0, scopeId);
        //    messageFormat = string.Concat("ScopeId:{ScopeId} ", messageFormat);

        //    return logger.BeginScope(messageFormat, list.ToArray());
        //}



        public static PerformanceScope BeginPerformanceScope<TState>(this ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, TState state)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (policies == null) throw new ArgumentNullException(nameof(policies));

            return new PerformanceScope(logger, policies, logger.BeginScope(state));
        }

        public static PerformanceScope BeginPerformanceScope(this ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, string messageFormat, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (policies == null) throw new ArgumentNullException(nameof(policies));

            return new PerformanceScope(logger, policies, logger.BeginScope(messageFormat, args));
        }

        #region IDictionary<string, object>.ToLogString()

        public static string ToLogString(this IDictionary<string, object> attributes)
        {
            return attributes.ToArray().ToLogString();
        }

        public static string ToLogString(this IEnumerable<KeyValuePair<string, object>> attributes)
        {
            var sb = new StringBuilder();

            if (attributes == null) return sb.ToString();

            foreach (var (k, v) in attributes)
            {
                if (sb.Length > 0) sb.Append("; ");
                var key = k.Replace("; ", ";").Replace(", ", ",");
                var value = v == null ? string.Empty : v?.ToString()?.Replace("; ", ";").Replace(", ", ","); ;
                sb.Append(key).Append(", ").Append(value);
            }

            return sb.ToString();
        }

        #endregion

        //public static ILoggingBuilder AddTentakelLoggerProvider(this ILoggingBuilder builder)
        //{
        //    builder.AddConfiguration();




        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());
        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<LoggerSinkProviderOptions>, LoggerSinkProviderOptionsSetup>());
        //    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<LoggerSinkProviderOptions>, LoggerProviderOptionsChangeTokenSource<LoggerSinkProviderOptions, LoggerSinkProvider>>());
        //    return builder;
        //}


        //public static ILoggingBuilder AddTentakelLoggerProvider(this ILoggingBuilder builder, Action<LoggerSinkProviderOptions> configure)
        //{
        //    if (configure == null) throw new ArgumentNullException(nameof(configure));

        //    builder.AddTentakelLoggerProvider();
        //    builder.Services.Configure(configure);

        //    return builder;
        //}
    }
}