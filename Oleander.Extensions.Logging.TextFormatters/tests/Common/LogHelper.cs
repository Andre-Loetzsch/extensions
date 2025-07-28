using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.TextFormatters.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Oleander.Extensions.Logging.TextFormatters.Tests.Common
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

        public LogHelper LogDebug()
        {
            this.Log(null, "Test Debug", "This is a test debug message.", LogLevel.Debug);
            return this;
        }

        public LogHelper LogTrace()
        {
            this.Log(null, "Test Trace", "This is a test trace message.", LogLevel.Trace);
            return this;
        }

        public LogHelper LogInformation()
        {
            this.Log(null, "Test Information", "This is a test information message.", LogLevel.Information);
            return this;
        }

        public LogHelper LogWarning()
        {
            this.Log(null, "Test Warning", "This is a test warning message.", LogLevel.Warning);
            return this;
        }

        public LogHelper LogError()
        {
            this.Log(null, "Test Error", $"This is a test error message.{Environment.NewLine}{Environment.NewLine}Error occurred!", LogLevel.Error);
            return this;
        }

        public LogHelper LogCritical()
        {
            try
            {
                throw new NotImplementedException("This operation is not supported!", 
                    new ArgumentException("The argument is not valid in this context!", "arg1"));
            }
            catch (Exception ex)
            {
                this.Log(ex, "Test Critical", $"This is a  test critical message: {ex.Message}", LogLevel.Critical);
            }
            return this;
        }

        private void Log(Exception? ex, string logCategory, string message, LogLevel logLevel)
        {
            var logEnty = new LogEntry
            {
                LogCategory = logCategory,
                Message = message,
                LogLevel = logLevel,
                Correlation = new KeyValuePair<string, int>("TEST", 1234), 
                Exception = ex
            };

            logEnty.Attributes["{CorrelationId}"] = logEnty.Correlation;
            logEnty.Attributes["{CallingAssembly}"] = Assembly.GetCallingAssembly().FullName ?? "";
            logEnty.Attributes["{CallerFilePath}"] = Assembly.GetCallingAssembly().Location;

            this.WriteFile(this._textFormatter.Format(logEnty));
        }

        private void WriteFile(string logEntry)
        {
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._fileName), logEntry);
        }

        public LogHelper DeleteFile()
        {
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._fileName));
            return this;
        }
    }
}
