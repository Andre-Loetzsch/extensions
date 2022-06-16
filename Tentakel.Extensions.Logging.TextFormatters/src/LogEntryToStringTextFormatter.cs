using System;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.TextFormatters;

public class LogEntryToStringTextFormatter : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return string.Concat(logEntry.ToString().Replace(Environment.NewLine, "{NewLine}"), Environment.NewLine);
    }
}