using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;
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
            var shortTextFormatter = new ShortTextFormatter();

            LogDebug(shortTextFormatter);
            LogTrace(shortTextFormatter);
            LogInformation(shortTextFormatter);
            LogWarning(shortTextFormatter);
            LogError(shortTextFormatter);
            LogCritical(shortTextFormatter);
        }

        private static void LogDebug(ITextFormatter textFormatter)
        {
            Log(textFormatter, "Test Debug", "This is a test debug message.", LogLevel.Debug);
        }

        private static void LogTrace(ITextFormatter textFormatter)
        {
            Log(textFormatter, "Test Trace", "This is a test trace message.", LogLevel.Trace);
        }

        private static void LogInformation(ITextFormatter textFormatter)
        {
            Log(textFormatter, "Test Information", "This is a test information message.", LogLevel.Information);
        }

        private static void LogWarning(ITextFormatter textFormatter)
        {
            Log(textFormatter, "Test Warning", "This is a test warning message.", LogLevel.Warning);
        }

        private static void LogError(ITextFormatter textFormatter)
        {
            Log(textFormatter, "Test Error", "This is a test error message.", LogLevel.Error);
        }

        private static void LogCritical(ITextFormatter textFormatter)
        {
            Log(textFormatter, "Test Critical", "This is a test critical message.", LogLevel.Critical);
        }


        private static void Log(ITextFormatter textFormatter, string logCategory, string message, LogLevel logLevel)
        {
            WriteFile(textFormatter.Format(new()
            {
                LogCategory = logCategory, 
                Message = message, 
                LogLevel = logLevel
            }));
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