using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class ShortBlockTextFormatterTest
{
    [Fact]
    public void TestBlockTextFormatter()
    {
        new LogHelper(new ShortBlockTextFormatter(), "ShortBlockTextFormatter.log")
            .DeleteFile()
            .LogTrace()
            .LogInformation()
            .LogWarning()
            .LogError()
            .LogCritical()
            .LogDebug();
    }
}