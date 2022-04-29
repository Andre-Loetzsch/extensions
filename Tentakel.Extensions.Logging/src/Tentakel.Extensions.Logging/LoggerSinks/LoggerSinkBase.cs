using Microsoft.Extensions.Logging;
using System;

namespace Tentakel.Extensions.Logging.LoggerSinks
{
    public abstract class LoggerSinkBase : ILoggerSink, IDisposable
    {
        protected LoggerSinkBase()
        {
            this.Name = this.GetType().Name;
        }

        protected LoggerSinkBase(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public string[] Categories { get; set; } = {"*"};
        public LogLevel LogLevel { get; set; }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.LogLevel <= logLevel;
        }

        public abstract void Log(LogEntry logEntry);

        #region IDisposable

        protected bool IsDisposed;

        ~LoggerSinkBase()
        {
            this?.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed) return;
            this.IsDisposed = true;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}