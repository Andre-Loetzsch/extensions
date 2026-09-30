using System;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;

namespace Oleander.Extensions.Logging.TextFormatters;

public class LogEntryToStringVerticalTextFormatter : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return string.Concat(Environment.NewLine, 
            
            logEntry.ToString()
                .Replace("\r\n", "{CRLF}")
                .Replace("\n", "{CR}")
                .Replace("\r", "{LF}")
                .Replace("|", Environment.NewLine), Environment.NewLine);
    }
}