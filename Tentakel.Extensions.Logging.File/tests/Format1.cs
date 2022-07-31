﻿using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.File.Tests;

public class Format1 : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return $"{logEntry.DateTime} {logEntry.Message}{Environment.NewLine}";
    }
}