using Oleander.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Oleander.Extensions.Logging.TextFormatters.Tests
{
    public class ShortTextFormatterTest
    {
        [Fact]
        public void TestShortTextFormatter()
        {
            new LogHelper(new ShortTextFormatter(), "ShortTextFormatter.log")
                .DeleteFile()
                .LogTrace()
                .LogInformation()
                .LogWarning()
                .LogError()
                .LogCritical()
                .LogDebug();
        }
    }
}