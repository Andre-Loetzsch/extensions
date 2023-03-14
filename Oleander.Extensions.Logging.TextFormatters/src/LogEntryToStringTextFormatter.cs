using System;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;

namespace Oleander.Extensions.Logging.TextFormatters;

public class LogEntryToStringTextFormatter : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return string.Concat(logEntry.ToString().Replace(Environment.NewLine, "{NewLine}"), Environment.NewLine);
    }
}