using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions.LoggerSinks;

namespace Tentakel.Extensions.Logging.Console
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
                    if (this._textFormatterCreated)
                    {
                        this._textFormatterCreated = true;
                        this.CreateTextFormatter();
                    }
                    
                    this.ColorizeKeywords(logEntry, this.TextFormatter.Format(logEntry));
                    //System.Console.WriteLine(this.TextFormatter.Format(logEntry));
                }

            }
            catch (Exception e)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(e.Message);
                System.Console.ResetColor();
            }
        }

        private void ColorizeKeywords(LogEntry logEntry, string formatMessage)
        {
            logEntry.Message ??= string.Empty;

            var messageLines = logEntry.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var messageLineDict = new Dictionary<string, string>();

            for (var i = 0; i < messageLines.Length; i++)
            {
                messageLineDict[$"Message{i}"] = messageLines[i];
            }

            var formatMessageLines = formatMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (var i = 0; i < formatMessageLines.Length; i++)
            {
                foreach (var (key, value) in messageLineDict)
                {
                    formatMessageLines[i] = formatMessageLines[i].Replace(value, string.Concat("%%", key, "%%"));
                }

                formatMessageLines[i] = formatMessageLines[i].Replace(logEntry.LogLevel.ToString(), string.Concat("%%", logEntry.LogLevel, "%%"));
                formatMessageLines[i] = formatMessageLines[i].Replace(logEntry.LogCategory, "%%LogCategory%%");
            }

            foreach (var line in formatMessageLines)
            {
                foreach (var test in line.Split("%%"))
                {
                    if (messageLineDict.TryGetValue(test, out var msg))
                    {
                        System.Console.ForegroundColor = logEntry.LogLevel switch
                        {
                            LogLevel.Trace => ConsoleColor.DarkGray,
                            LogLevel.Debug => ConsoleColor.Gray,
                            LogLevel.Information => ConsoleColor.Blue,
                            LogLevel.Warning => ConsoleColor.Yellow,
                            LogLevel.Error => ConsoleColor.Red,
                            LogLevel.Critical => ConsoleColor.DarkRed,
                            _ => this.ForegroundColor
                        };

                        //System.Console.ForegroundColor = this.ForegroundColor;
                        System.Console.Write(msg);
                        continue;
                    }

                    if (test == "LogCategory")
                    {
                        System.Console.ForegroundColor = GetCategoryForegroundColors(logEntry.LogCategory);
                        System.Console.Write(logEntry.LogCategory);
                        continue;
                    }

                    System.Console.ForegroundColor = test switch
                    {
                        "Trace" => ConsoleColor.DarkGray,
                        "Debug" => ConsoleColor.Gray,
                        "Information" => ConsoleColor.Blue,
                        "Warning" => ConsoleColor.Yellow,
                        "Error" => ConsoleColor.Red,
                        "Critical" => ConsoleColor.DarkRed,
                        _ => this.ForegroundColor
                    };

                    System.Console.Write(test);
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
    }
}