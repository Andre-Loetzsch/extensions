using System.Text;

namespace Tentakel.Extensions.Logging.TextFormatters
{
    public class AllTextFormatter : ITextFormatter
    {
        public string Format(LogEntry logEntry)
        {
            return logEntry.ToString();
        }
    }

    public class ShortTextFormatter : ITextFormatter
    {
        StringBuilder _formatBuilder = new();

        public string Format(LogEntry logEntry)
        {
            this._formatBuilder.Length = 0;

            this._formatBuilder
                .Append("[").Append(logEntry.LogEntryId.ToString("0000000")).Append("] ")
                .Append("[").Append(logEntry.DateTime.ToString("yyyy-MM-dd HH:mm:ss fff")).AppendLine("]")

                .Append("[").Append(logEntry.LogLevel).Append("]".PadRight(10))
                .Append("[").Append(logEntry.LogCategory).AppendLine("]")
                .AppendLine("  ").Append(logEntry.Message);

            return this._formatBuilder.ToString();
        }
    }
}