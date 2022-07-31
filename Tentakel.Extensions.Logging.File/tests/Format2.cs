﻿using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.File.Tests;

public class Format2 : ITextFormatter
{
    public string Format(LogEntry logEntry)
    {
        return $"------------------{Environment.NewLine}  DateTime:{logEntry.DateTime}{Environment.NewLine}  Message:{logEntry.Message}{Environment.NewLine}";
    }
}