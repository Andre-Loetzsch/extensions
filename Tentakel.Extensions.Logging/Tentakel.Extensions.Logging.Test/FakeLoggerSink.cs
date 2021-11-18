using System.Collections.Generic;
using Tentakel.Extensions.Logging.LoggerSinks;

namespace Tentakel.Extensions.Logging.Test
{
    public class FakeLoggerSink : LoggerSinkBase
    {
        public List<LogEntry> Entries { get; } = new();

        public override void Log(LogEntry logEntry)
        {
            this.Entries.Add(logEntry);
        }
    }
}