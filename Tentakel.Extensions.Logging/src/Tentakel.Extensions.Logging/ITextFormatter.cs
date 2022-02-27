using System.IO;

namespace Tentakel.Extensions.Logging
{
    public interface ITextFormatter
    {
        string Format(LogEntry logEntry);
    }
}