using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging.Loggers
{
    public class Logger : ILogger
    {
        private readonly LoggerSinkProvider _loggerProvider;
        private readonly string _category;

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
                    EventId = eventId.Id,
                    Message = state.ToString(),
                    State = state,
                    Exception = exception
                };

                if (state is IEnumerable<KeyValuePair<string, object>> attributes)
                {
                    logEntry.Attributes = new Dictionary<string, object>(attributes);
                }
            }

            logEntry.SourceCategory = this._category;
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