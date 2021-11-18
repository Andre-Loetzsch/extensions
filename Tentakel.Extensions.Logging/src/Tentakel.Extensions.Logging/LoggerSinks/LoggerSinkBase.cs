using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.LoggerSinks
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
        public string[] Categories { get; set; }
        public LogLevel LogLevel { get; set; }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.LogLevel <= logLevel;
        }

        public abstract void Log(LogEntry logEntry);
    }
}