using Tentakel.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Tentakel.Extensions.Logging.TextFormatters.Tests;

public class LogEntryToStringTextFormatterTest
{
    [Fact]
    public void TestLogEntryToStringTextFormatter()
    {
        var logHelper = new LogHelper(new LogEntryToStringTextFormatter(), "LogEntryToStringTextFormatter.log");
        logHelper.DeleteFile();
        logHelper.LogDebug();
        logHelper.LogTrace();
        logHelper.LogInformation();
        logHelper.LogWarning();
        logHelper.LogError();
        logHelper.LogCritical();
    }
}