using System;
using Microsoft.Extensions.Logging;

namespace Oleander.Extensions.Logging
{
    public interface ILoggerSink : IDisposable
    {
        string Name { get; }

        string[] Categories { get; }

        bool IsEnabled(LogLevel logLevel);
        bool IsDisposed { get; }
        void Log(LogEntry logEntry);
    }
}