using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Providers;
using Tentakel.Extensions.Logging.SourceHelper;

namespace Tentakel.Extensions.Logging.Loggers
{
    public class Logger : ILogger
    {
        private readonly LoggerSinkProvider _loggerProvider;
        private readonly string _category;
        private readonly SourceCache _sourceCache = new();

        internal Logger(LoggerSinkProvider loggerProvider, string category)
        {
            this._loggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
            this._category = category;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is not LogEntry logEntry)
            {
                logEntry = new LogEntry
                {
                    LogLevel = logLevel,
                    LogCategory = this._category,
                    EventId = eventId.Id,
                    Message = state.ToString(),
                    State = state,
                    Exception = exception
                };

                if (state is IEnumerable<KeyValuePair<string, object>> attributes)
                {
                    logEntry.Attributes = new Dictionary<string, object>(attributes);
                }

                // TODO edit conditions
                if (logEntry.IsSourceNullOrEmpty)
                {
                    var originalFormat = logEntry.Attributes.TryGetValue("{OriginalFormat}", out var value)
                        ? value
                        : null;
                    var callerFilePath = logEntry.Attributes.TryGetValue("{CallerFilePath}", out value)
                        ? value
                        : null;
                    var callerMemberName = logEntry.Attributes.TryGetValue("{CallerMemberName}", out value)
                        ? value
                        : null;
                    var callerLineNumber = logEntry.Attributes.TryGetValue("{CallerLineNumber}", out value)
                        ? value
                        : null;
                    var sourceKey =
                        $"{originalFormat}:{callerFilePath}:{callerMemberName}:{callerLineNumber}:{callerLineNumber}";

                    if (this._sourceCache.TryGetSource(sourceKey, out var source))
                    {
                        logEntry.Source = source;
                    }
                    else
                    {
                        if (SourceResolver.TryFindFromStackTrace(logEntry.LoggerSinkType, new StackTrace(), out source))
                        {
                            logEntry.Source = source;
                            this._sourceCache.AddSource(sourceKey, source);
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

        public IDisposable BeginScope<TState>(TState state)
        {
            return this._loggerProvider.ScopeProvider.Push(state);
        }
    }
}