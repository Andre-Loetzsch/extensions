using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class ShortVerticalTextFormatterTest
{
    [Fact]
    public void TestShortVerticalTextFormatter()
    {
        new LogHelper(new ShortVerticalTextFormatter(), "ShortVerticalTextFormatter.log")
            .DeleteFile()
            .LogTrace()
            .LogInformation()
            .LogWarning()
            .LogError()
            .LogCritical()
            .LogDebug();
    }
}