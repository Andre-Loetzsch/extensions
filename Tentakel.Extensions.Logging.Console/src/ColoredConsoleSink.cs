using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.TextFormatters.Abstractions.LoggerSinks;

namespace Tentakel.Extensions.Logging.Console
{
    public class ColoredConsoleSink : TextLoggerSinkBase
    {
        public ColoredConsoleSink() : this(nameof(LoggerSinks.ConsoleColorSink))
        {
        }

        public ColoredConsoleSink(string name)
        {
            this.Name = name;
        }


        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;


        public override void Log(LogEntry logEntry)
        {
            try
            {
                var log = this.TextFormatter.Format(logEntry);

                System.Console.WriteLine(log);
                System.Console.WriteLine();

                this.ColorizeKeywords(logEntry, log);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }


        private void ColorizeKeywords(LogEntry logEntry, string formatMessage)
        {
            logEntry.Message ??= string.Empty;

            var messageLines = logEntry.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var messageLineDict = new Dictionary<string, string>();

            for (var i = 0; i < messageLines.Length; i++)
            {
                messageLineDict[$"{{Message{i}}}"] = messageLines[i];
            }

            var formatMessageLines = formatMessage.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (var i = 0; i < formatMessageLines.Length; i++)
            {
                var test = formatMessageLines[i];

                foreach (var messageLineItem in messageLineDict)
                {
                    test = test.Replace(messageLineItem.Value, $" {messageLineItem.Key} " );
                }

                test = test.Replace(logEntry.LogCategory, $"{{LogCategory{i}}}");

                foreach (var word in test.Split(" "))
                {
                    if (word == string.Empty)
                    {
                        System.Console.Write(" ");
                        continue;
                    }

                    if (messageLineDict.TryGetValue(word, out var msg))
                    {
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        System.Console.Write(msg);
                        continue;
                    }

                    if (word.Contains($"{{LogCategory{i}}}"))
                    {
                        System.Console.ForegroundColor = ConsoleColor.Magenta;
                        System.Console.Write(logEntry.LogCategory);
                        System.Console.Write(" ");
                        continue;
                    }

                    System.Console.ForegroundColor = word switch
                    {
                        "Trace" => ConsoleColor.Gray,
                        "Debug" => ConsoleColor.DarkGray,
                        "Information" => ConsoleColor.Blue,
                        "Warning" => ConsoleColor.Yellow,
                        "Error" => ConsoleColor.Red,
                        "Critical" => ConsoleColor.DarkRed,
                        _ => this.ForegroundColor
                    };

                    System.Console.Write(word);
                    System.Console.Write(" ");
                }

                System.Console.WriteLine();
            }
        }
    }
}