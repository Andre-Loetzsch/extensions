using Tentakel.Extensions.Logging.TextFormatters.Tests.Common;
using Xunit;

namespace Tentakel.Extensions.Logging.TextFormatters.Tests
{
    public class ShortTextFormatterTest
    {
        [Fact]
        public void TestShortTextFormatter()
        {
            var logHelper = new LogHelper(new ShortTextFormatter(), "ShortTextFormatter.log");
            logHelper.DeleteFile();
            logHelper.LogDebug();
            logHelper.LogTrace();
            logHelper.LogInformation();
            logHelper.LogWarning();
            logHelper.LogError();
            logHelper.LogCritical();
        }
    }
}