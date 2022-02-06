using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.Abstractions
{
    public class Logger<T> : ILogger
    {
        private ILogger _innerInstance;

        public Logger()
        {
            this._innerInstance = LoggerFactory.CreateLogger<T>();
        }

        public IDisposable BeginScope<TState>(TState state)
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

        public static ILogger Instance(string category)
        {
            return LoggerFactory.CreateLogger(category);
        }

        public static ILogger Instance<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }

    }

}