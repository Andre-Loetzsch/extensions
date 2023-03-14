using System;
using System.Text;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;

namespace Oleander.Extensions.Logging.TextFormatters;

public class ShortTextFormatter : ITextFormatter
{
    private readonly StringBuilder _formatBuilder = new();
    private int _categoryPadRight;
    public string Format(LogEntry logEntry)
    {

        if (logEntry.LogCategory.Length > this._categoryPadRight)
        {
            this._categoryPadRight = logEntry.LogCategory.Length +2;
        }

        this._formatBuilder.Length = 0;
        this._formatBuilder
            .Append('[').Append(logEntry.LogEntryId.ToString("0000000")).Append(' ')
            .Append(logEntry.DateTime.ToString("yyyy-MM-dd HH:mm:ss fff")).Append(' ')
            .Append(logEntry.LogLevel.ToString().PadRight(12)).Append(' ')
            .Append(logEntry.LogCategory).Append("] ".PadRight(this._categoryPadRight - logEntry.LogCategory.Length));

        return string.IsNullOrEmpty(logEntry.Message) ? 
            this._formatBuilder.ToString() : 
            this._formatBuilder.Append("  ")
                .Append(logEntry.Message.Replace(Environment.NewLine, "{NewLine}"))
                .AppendLine().ToString();
    }
}