using System.Collections.Generic;
using Oleander.Extensions.Logging.LoggerSinks;

namespace Oleander.Extensions.Logging.Tests;

public class FakeLoggerSink : LoggerSinkBase
{
    public List<LogEntry> Entries { get; } = [];

    public override void Log(LogEntry logEntry)
    {
        this.Entries.Add(logEntry);
    }
}