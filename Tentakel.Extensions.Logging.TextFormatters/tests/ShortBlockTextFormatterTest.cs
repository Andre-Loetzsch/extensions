using Tentakel.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Tentakel.Extensions.Logging.TextFormatters.Tests;

public class ShortBlockTextFormatterTest
{
    [Fact]
    public void TestBlockTextFormatter()
    {
        var logHelper = new LogHelper(new ShortBlockTextFormatter(), "ShortBlockTextFormatter.log");
        logHelper.DeleteFile();
        logHelper.LogDebug();
        logHelper.LogTrace();
        logHelper.LogInformation();
        logHelper.LogWarning();
        logHelper.LogError();
        logHelper.LogCritical();
    }
}