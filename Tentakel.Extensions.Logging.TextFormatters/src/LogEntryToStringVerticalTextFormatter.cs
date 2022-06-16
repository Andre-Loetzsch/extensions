using System;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.TextFormatters;

public class LogEntryToStringVerticalTextFormatter : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return string.Concat(Environment.NewLine, logEntry.ToString().Replace(Environment.NewLine, "{NewLine}").Replace("|", Environment.NewLine), Environment.NewLine);
    }
}