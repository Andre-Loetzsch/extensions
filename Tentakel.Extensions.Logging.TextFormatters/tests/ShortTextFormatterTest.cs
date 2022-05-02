using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Tentakel.Extensions.Logging.TextFormatters.Tests
{
    public class ShortTextFormatterTest
    {
        public ShortTextFormatterTest()
        {
            DeleteFile();
        }


        [Fact]
        public void TestShortTextFormatter()
        {
            var logEntry = new LogEntry { LogCategory = "TEST", Message = "This is a test message.", LogLevel = LogLevel.Debug };

            var shortTextFormatter = new ShortTextFormatter();

            var result = shortTextFormatter.Format(logEntry);
          
         

            WriteFile(result);
        }






        private static void WriteFile(string logEntry)
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextFormatterTest.txt"), logEntry);
        }

        private static void DeleteFile()
        {
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextFormatterTest.txt"));
        }
    }
}