namespace Tentakel.Extensions.Logging.TextFormatters.Abstractions
{
    public interface ITextFormatter
    {
        string Format(LogEntry logEntry);
    }
}