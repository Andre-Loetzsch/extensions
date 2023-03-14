using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class ShortVerticalTextFormatterTest
{
    [Fact]
    public void TestShortVerticalTextFormatter()
    {
        var logHelper = new LogHelper(new ShortVerticalTextFormatter(), "ShortVerticalTextFormatter.log");
        logHelper.DeleteFile();
        logHelper.LogDebug();
        logHelper.LogTrace();
        logHelper.LogInformation();
        logHelper.LogWarning();
        logHelper.LogError();
        logHelper.LogCritical();
    }
}