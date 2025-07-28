using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.TextFormatters.Abstractions.LoggerSinks;

namespace Oleander.Extensions.Logging.Console
{
    public class ColoredConsoleSink : TextLoggerSinkBase
    {
        private static readonly object syncObj = new();
        private bool _textFormatterCreated;
        private static readonly Dictionary<string, ConsoleColor> categoryForegroundColors = new();

        public ColoredConsoleSink() : this(nameof(ColoredConsoleSink))
        {
        }

        public ColoredConsoleSink(string name)
        {
            this.Name = name;
        }

        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;

        public override void Log(LogEntry logEntry)
        {
            try
            {
                lock (syncObj)
                {
                    if (!this._textFormatterCreated)
                    {
                        this._textFormatterCreated = true;
                        this.CreateTextFormatter();
                    }

                    this.ColorizeKeywords(logEntry, this.TextFormatter.Format(logEntry));
                }
            }
            catch (Exception e)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(e.Message);
                System.Console.ResetColor();
            }
        }


        #region private members

        private void ColorizeKeywords(LogEntry logEntry, string formatMessage)
        {
            logEntry.Message ??= string.Empty;

            var messageLines = logEntry.Message.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            var messageLineDict = new Dictionary<string, string>();

            for (var i = 0; i < messageLines.Length; i++)
            {
                messageLineDict[$"Message{i}"] = messageLines[i];
            }

            var formatMessageLines = formatMessage.Split([Environment.NewLine], StringSplitOptions.None);

            for (var i = 0; i < formatMessageLines.Length; i++)
            {
                foreach (var item in messageLineDict)
                {
                    formatMessageLines[i] = string.IsNullOrEmpty(item.Value) ?
                        string.Concat("%%", item.Key, "%%") :
                        formatMessageLines[i].Replace(item.Value, string.Concat("%%", item.Key, "%%"));
                }

                formatMessageLines[i] = formatMessageLines[i].Replace(logEntry.LogLevel.ToString(), "%%LogLevel%%");
                formatMessageLines[i] = formatMessageLines[i].Replace(logEntry.Source, "%%Source%%");
                formatMessageLines[i] = formatMessageLines[i].Replace(logEntry.LogCategory, "%%LogCategory%%");
            }

            foreach (var line in formatMessageLines)
            {
                foreach (var test in line.Split(["%%"], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (messageLineDict.TryGetValue(test, out var msg))
                    {
                        this.SetLogLevelConsoleColor(logEntry.LogLevel);
                        System.Console.Write(msg);
                        continue;
                    }

                    switch (test)
                    {
                        case "LogCategory":
                            System.Console.ForegroundColor = GetCategoryForegroundColors(logEntry.LogCategory);
                            System.Console.Write(logEntry.LogCategory);
                            continue;
                        case "Source":
                            System.Console.ForegroundColor = ConsoleColor.DarkGray;
                            System.Console.Write(logEntry.Source);
                            continue;
                        case "LogLevel":
                            this.SetLogLevelConsoleColor(logEntry.LogLevel);
                            System.Console.Write(logEntry.LogLevel);
                            break;
                        default:
                            System.Console.ForegroundColor = this.ForegroundColor;
                            System.Console.Write(test);
                            break;
                    }
                }

                System.Console.ResetColor();
                System.Console.WriteLine();
            }
        }

        private static ConsoleColor GetCategoryForegroundColors(string logCategory)
        {
            if (categoryForegroundColors.TryGetValue(logCategory, out var color)) return color;

            var colorIndex = categoryForegroundColors.Count % 15;
            color = (ConsoleColor)colorIndex + 1;

            categoryForegroundColors[logCategory] = color;
            return color;
        }

        private void SetLogLevelConsoleColor(LogLevel logLevel)
        {
            System.Console.ForegroundColor = logLevel switch
            {
                LogLevel.Trace => ConsoleColor.DarkGray,
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Information => ConsoleColor.Blue,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => this.ForegroundColor
            };
        }

        #endregion
    }
}