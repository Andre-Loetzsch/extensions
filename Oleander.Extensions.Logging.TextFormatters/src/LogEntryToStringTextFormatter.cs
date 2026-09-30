using System;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;

namespace Oleander.Extensions.Logging.TextFormatters;

public class LogEntryToStringTextFormatter : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return string.Concat(logEntry.ToString()
            .Replace("\r\n", "{CRLF}")
            .Replace("\r", "{CR}")
            .Replace("\n", "{LF}"), Environment.NewLine);
    }
}