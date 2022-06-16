using System;
using System.Text;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.TextFormatters;

public class ShortVerticalTextFormatter : ITextFormatter
{
    private readonly StringBuilder _formatBuilder = new();

    public string Format(LogEntry logEntry)
    {
        this._formatBuilder.Length = 0;

        this._formatBuilder
            .Append('[').Append(logEntry.LogEntryId.ToString("0000000")).Append(' ')
            .Append(logEntry.DateTime.ToString("yyyy-MM-dd HH:mm:ss fff")).Append(' ')
            .Append(logEntry.LogLevel.ToString().PadRight(12)).Append(' ')
            .Append(logEntry.LogCategory).AppendLine("]");

        if (string.IsNullOrEmpty(logEntry.Message)) return this._formatBuilder.ToString();

        foreach (var line in logEntry.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
        {
            this._formatBuilder.Append("  ").Append(line).AppendLine();
        }

        return this._formatBuilder.ToString();
    }
}