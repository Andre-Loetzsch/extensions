using Microsoft.Extensions.Logging;
using System;

namespace Oleander.Extensions.Logging.LoggerSinks
{
    public abstract class LoggerSinkBase : ILoggerSink
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
        public string[] Categories { get; set; } = [];
        public LogLevel LogLevel { get; set; }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.LogLevel <= logLevel;
        }

        public abstract void Log(LogEntry logEntry);

        #region IDisposable

        public bool IsDisposed { get; private set; }

        ~LoggerSinkBase()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);

            if (this.IsDisposed) return;
            this.IsDisposed = true;

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}