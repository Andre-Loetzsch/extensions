using System;
using System.Text;

namespace Tentakel.Extensions.Logging.TextFormatters.Abstractions;

public class DefaultTextFormatter : ITextFormatter
{
    private readonly StringBuilder _stringBuilder = new();

    public string Format(LogEntry logEntry)
    {
        this._stringBuilder.Length = 0;
        this._stringBuilder.Append('[')
            .Append(logEntry.LogEntryId.ToString("0000000")).Append(' ')
            .Append(logEntry.DateTime.ToString("yyyy-MM-dd HH: mm:ss")).Append(' ')
            .Append(logEntry.LogLevel).Append(' ')
            .Append(logEntry.LogCategory).Append(']')
            .Append(logEntry.Source).Append(" - ")
            .Append(logEntry.Message).Append(Environment.NewLine);

        return this._stringBuilder.ToString();
    }
}