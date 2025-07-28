using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class LogEntryToStringTextFormatterTest
{
    [Fact]
    public void TestLogEntryToStringTextFormatter()
    {
        new LogHelper(new LogEntryToStringTextFormatter(), "LogEntryToStringTextFormatter.log")
            .DeleteFile()
            .LogTrace()
            .LogInformation()
            .LogWarning()
            .LogError()
            .LogCritical()
            .LogDebug();
    }
}