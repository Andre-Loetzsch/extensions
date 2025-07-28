using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests;

public class BlockTextFormatterTest
{
    [Fact]
    public void TestBlockTextFormatter()
    {
        new LogHelper(new BlockTextFormatter(), "BlockTextFormatter.log")
            .DeleteFile()
            .LogTrace()
            .LogInformation()
            .LogWarning()
            .LogError()
            .LogCritical()
            .LogDebug();
    }
}