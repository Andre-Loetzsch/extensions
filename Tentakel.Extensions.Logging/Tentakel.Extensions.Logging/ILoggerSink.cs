using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging
{
    public interface ILoggerSink
    {
        string Name { get; }

        string[] Categories { get; }

        bool IsEnabled(LogLevel logLevel);

        void Log(LogEntry logEntry);
    }
}