using System;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;

namespace Oleander.Extensions.Logging.File.Tests;

public class Format2 : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return $"------------------{Environment.NewLine}  Format2: DateTime:{logEntry.DateTime}{Environment.NewLine}  Message:{logEntry.Message}{Environment.NewLine}";
    }
}