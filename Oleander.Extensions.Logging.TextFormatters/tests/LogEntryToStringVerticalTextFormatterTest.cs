using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class LogEntryToStringVerticalTextFormatterTest
{
    [Fact]
    public void TestLogEntryToStringVerticalTextFormatter()
    {
        new LogHelper(new LogEntryToStringVerticalTextFormatter(), "LogEntryToStringVerticalTextFormatter.log")
            .DeleteFile()
            .LogTrace()
            .LogInformation()
            .LogWarning()
            .LogError()
            .LogCritical()
            .LogDebug();
    }
}