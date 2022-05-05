using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions;

namespace Tentakel.Extensions.Logging.TextFormatters.Tests.Common
{
    internal class LogHelper
    {
        private readonly ITextFormatter _textFormatter;
        private readonly string _fileName;
        public LogHelper(ITextFormatter textFormatter, string fileName)
        {
            this._textFormatter = textFormatter;
            var loggingDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logging");
            if (!Directory.Exists(loggingDir)) Directory.CreateDirectory(loggingDir);
            this._fileName = Path.Combine(loggingDir, fileName);
        }

        public void LogDebug()
        {
            this.Log("Test Debug", "This is a test debug message.", LogLevel.Debug);
        }

        public void LogTrace()
        {
            this.Log("Test Trace", "This is a test trace message.", LogLevel.Trace);
        }

        public void LogInformation()
        {
            this.Log("Test Information", "This is a test information message.", LogLevel.Information);
        }

        public void LogWarning()
        {
            this.Log("Test Warning", "This is a test warning message.", LogLevel.Warning);
        }

        public void LogError()
        {
            this.Log("Test Error", $"This is a test error message.{Environment.NewLine}{Environment.NewLine}Error occurred!", LogLevel.Error);
        }

        public void LogCritical()
        {
            try
            {
                throw new NotImplementedException("Exception occurred!");
            }
            catch (Exception ex)
            {
                this.Log("Test Critical", $"This is a test critical message.{Environment.NewLine}{ex}", LogLevel.Critical);
            }
        }

        public void Log(string logCategory, string message, LogLevel logLevel)
        {
            this.WriteFile(this._textFormatter.Format(new()
            {
                LogCategory = logCategory,
                Message = message,
                LogLevel = logLevel
            }));
        }

        public void WriteFile(string logEntry)
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._fileName), logEntry);
        }

        public void DeleteFile()
        {
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._fileName));
        }
    }
}
