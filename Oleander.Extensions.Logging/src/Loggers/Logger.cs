using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Providers;
using Oleander.Extensions.Logging.SourceHelper;

namespace Oleander.Extensions.Logging.Loggers
{
    public class Logger : ILogger
    {
        private readonly LoggerSinkProvider _loggerProvider;
        private readonly string _category;
        private readonly SourceCache _sourceCache = new();

        private static int instanceId;
        private readonly int _instanceId;

        internal Logger(LoggerSinkProvider loggerProvider, string category)
        {
            this._loggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
            this._category = category;

            this._instanceId = Interlocked.Increment(ref instanceId);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (state is not LogEntry logEntry)
            {
                logEntry = new()
                {
                    LogLevel = logLevel,
                    LogCategory = this._category,
                    EventId = eventId.Id,
                    Message = formatter(state, exception),
                    State = state,
                    Exception = exception
                };

                if (state is IEnumerable<KeyValuePair<string, object>> attributes)
                {
                    logEntry.Attributes = new Dictionary<string, object>();

                    foreach (var item in attributes)
                    {
                        logEntry.Attributes[item.Key] = item.Value;
                    }
                }

                if (logEntry.Attributes.TryGetValue("{CorrelationId}", out var correlationId))
                {
                    logEntry.Correlation = correlationId;
                }
                else
                {
                    logEntry.Correlation = this._instanceId;
                    logEntry.Attributes["{CorrelationId}"] = this._instanceId;
                }

                if (logEntry.IsSourceNullOrEmpty)
                {
                    var sb = new StringBuilder();
                    string? sourceKey;

                    if (logEntry.Attributes.TryGetValue("{CallerFilePath}", out var callerFilePath) &&     
                        logEntry.Attributes.TryGetValue("{CallerMemberName}", out var callerMemberName) &&
                        logEntry.Attributes.TryGetValue("{CallerLineNumber}", out var callerLineNumber))
                    {
                        sb.Append(callerFilePath).Append(':')
                            .Append(callerMemberName).Append(':')
                            .Append(callerLineNumber);

                        sourceKey = sb.ToString();

                        if (SourceResolver.TryFindFromAttributes(logEntry.Attributes, out var sourceValue))
                        {
                            this._sourceCache.AddSource(sourceKey, sourceValue);
                        }
                    }
                    else
                    {
                        sourceKey = logEntry.Attributes.TryGetValue("{OriginalFormat}", out var value) ?
                                value.ToString() : logEntry.Message;
                    }

                    if (string.IsNullOrEmpty(sourceKey)) sourceKey = Guid.NewGuid().ToString();

                    if (this._sourceCache.TryGetSource(sourceKey!, out var source))
                    {
                        logEntry.Source = source ?? string.Empty;
                    }
                    else
                    {
                        if (SourceResolver.TryFindFromStackTrace(logEntry.LoggerSinkType, new(), out source))
                        {
                            logEntry.Source = source;
                            this._sourceCache.AddSource(sourceKey!, logEntry.Source);
                        }
                    }
                }
            }

            this._loggerProvider.Log(logEntry);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this._loggerProvider.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return this._loggerProvider.ScopeProvider.Push(state);
        }
    }
}