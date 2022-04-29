using System.Text;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.TextFormatters
{
    public class ShortTextFormatter : ITextFormatter
    {
        private readonly StringBuilder _formatBuilder = new();

        public string Format(LogEntry logEntry)
        {
            this._formatBuilder.Length = 0;

            this._formatBuilder
                .Append('[').Append(logEntry.LogEntryId.ToString("0000000")).Append(' ')
                .Append(logEntry.DateTime.ToString("yyyy-MM-dd HH:mm:ss fff")).Append(' ')
                .Append(logEntry.LogLevel.ToString().PadRight(12)).Append(' ')
                .Append(logEntry.LogCategory).AppendLine("]")
                .Append("  ").Append(logEntry.Message);

            return this._formatBuilder.ToString();
        }
    }
}