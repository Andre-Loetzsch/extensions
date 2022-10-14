using Tentakel.Extensions.Logging;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace ConsoleApp;

public class TestTextFormatter3 : ITextFormatter
{
    private readonly StringBuilder _formatBuilder = new();

    public string Format(LogEntry logEntry)
    {
        this._formatBuilder.Length = 0;

        this._formatBuilder
            .Append('[').Append(logEntry.LogEntryId.ToString("0000000")).Append(' ')
            .Append(logEntry.DateTime.ToString("yyyy-MM-dd HH:mm:ss fff")).Append(' ')
            .Append(logEntry.LogLevel.ToString().PadRight(12))
            .Append(logEntry.LogCategory).Append("] ");

        if (string.IsNullOrEmpty(logEntry.Message)) return this._formatBuilder.ToString();

        var padLeft = this._formatBuilder.Length;
        var messageSplit = logEntry.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        for (var i = 0; i < messageSplit.Length; i++)
        {
            if (i > 0) this._formatBuilder.Append(string.Empty.PadLeft(padLeft));
            this._formatBuilder.Append(messageSplit[i]);


            if (i < messageSplit.Length - 1) this._formatBuilder.AppendLine();
        }

        return this._formatBuilder.ToString();
    }
}