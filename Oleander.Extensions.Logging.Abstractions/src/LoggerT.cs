using System;
using Microsoft.Extensions.Logging;

namespace Oleander.Extensions.Logging.Abstractions
{
    public class Logger<T> : ILogger
    {
        private readonly ILogger _innerInstance = LoggerFactory.CreateLogger<T>();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return this._innerInstance.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this._innerInstance.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            this._innerInstance.Log(logLevel, eventId, state, exception, formatter);
        }

        public static ILogger Instance()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}