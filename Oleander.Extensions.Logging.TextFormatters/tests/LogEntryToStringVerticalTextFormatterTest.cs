using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class LogEntryToStringVerticalTextFormatterTest
{
    [Fact]
    public void TestLogEntryToStringVerticalTextFormatter()
    {
        var logHelper = new LogHelper(new LogEntryToStringVerticalTextFormatter(), "LogEntryToStringVerticalTextFormatter.log");
        logHelper.DeleteFile();
        logHelper.LogDebug();
        logHelper.LogTrace();
        logHelper.LogInformation();
        logHelper.LogWarning();
        logHelper.LogError();
        logHelper.LogCritical();
    }
}