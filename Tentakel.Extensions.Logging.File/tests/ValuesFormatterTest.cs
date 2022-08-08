using Xunit;
using System.Linq;

namespace Tentakel.Extensions.Logging.File.Tests
{
    public class ValuesFormatterTest
    {
        private const string defaultFileNameTemplate = "{baseDirectory}/Logging/{dateTime:yyyy}/{dateTime:MM}/{processName}/{dateTime:yyyy-MM-dd}.{processId}.log";

        [Fact]
        public void TestExtractDateTimeFormats_yy()
        {
            var result = ValuesFormatter.ExtractDateTimeFormats("{dateTime:yy}").ToList();

            Assert.Single(result);
            Assert.Equal("yy", result[0]);
        }

        [Fact]
        public void TestExtractDateTimeFormats_defaultFileNameTemplate()
        {
            var result = ValuesFormatter.ExtractDateTimeFormats(defaultFileNameTemplate).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal("yyyy", result[0]);
            Assert.Equal("MM", result[1]);
            Assert.Equal("yyyy-MM-dd", result[2]);
        }

        [Fact]
        public void TestExtractKeys_defaultFileNameTemplate()
        {
            var result = ValuesFormatter.ExtractKeys(defaultFileNameTemplate).ToList();

            Assert.Equal(6, result.Count);
            Assert.Equal("baseDirectory", result[0]);
            Assert.Equal("dateTime:yyyy", result[1]);
            Assert.Equal("dateTime:MM", result[2]);
            Assert.Equal("processName", result[3]);
            Assert.Equal("dateTime:yyyy-MM-dd", result[4]);
            Assert.Equal("processId", result[5]);
        }

        [Fact]
        public void TestExtractKeysWithTwoConsecutiveBraces()
        {
            var result = ValuesFormatter.ExtractKeys("{{baseDirectory}").ToList();

            Assert.Single(result);
            Assert.Equal("baseDirectory", result[0]);

            result = ValuesFormatter.ExtractKeys("{  {baseDirectory}").ToList();

            Assert.Single(result);
            Assert.Equal("baseDirectory", result[0]);
        }
    }
}