using Microsoft.Extensions.Logging;

namespace ConsoleApp;

internal class Program
{
    private static void Main()
    {
        var loggerFactory = new ConsoleLoggerFactory();
        var logger1 = loggerFactory.CreateLogger<Program>();
        var logger2 = loggerFactory.CreateLogger("Test");

        logger1.LogInformation(string.Empty);
        logger1.LogDebug(null);
        logger1.LogWarning($"{Environment.NewLine}Test ConsoleApp.Program.Main and empty line{Environment.NewLine}{Environment.NewLine}{"".PadLeft(2, '\t')}Test Warning, Debug and empty line{Environment.NewLine}{Environment.NewLine}{"".PadLeft(4, '\t')}Test ConsoleApp.Program and empty line{Environment.NewLine}");

        logger1.LogTrace("This is a Trace message!");
        //logger2.LogTrace("This is a Trace message!");

        logger1.LogError("Error");
        //logger2.LogTrace("This is a Trace message!");


        logger1.LogDebug("This is a Debug message!");
        //logger2.LogDebug("This is a Debug message!");


        logger1.LogInformation("This is a Information message!");
        //logger2.LogInformation("This is a Information message!");


        logger1.LogWarning("This is a Warning message!");
        //logger2.LogWarning("This is a Warning message!");


        logger1.LogError("This is a Error message!");
        //logger2.LogError("This is a Error message!");


        logger1.LogCritical("This is a Critical message!");
        //logger2.LogCritical("This is a Critical message!");


        logger1.LogTrace($"This is a Trace message{Environment.NewLine}with multi{Environment.NewLine}lines.");
        //logger2.LogTrace($"This is a Trace message{Environment.NewLine}with multi{Environment.NewLine}lines.");


        logger2.LogInformation(string.Empty);
        logger2.LogDebug(null);
        logger2.LogWarning($"{Environment.NewLine}Test ConsoleApp.Program.Main and empty line{Environment.NewLine}{Environment.NewLine}{"".PadLeft(2, '\t')}Test Warning, Debug and empty line{Environment.NewLine}{Environment.NewLine}{"".PadLeft(4, '\t')}Test ConsoleApp.Program and empty line{Environment.NewLine}");

        logger2.LogTrace("This is a Trace message!");
        logger2.LogDebug("This is a Debug message!");
        logger2.LogInformation("This is a Information message!");
        logger2.LogWarning("This is a Warning message!");
        logger2.LogError("This is a Error message!");
        logger2.LogCritical("This is a Critical message!");
        logger2.LogTrace($"This is a Trace message{Environment.NewLine}with multi{Environment.NewLine}lines.");

        for (var i = 0; i < 30; i++)
        {
            loggerFactory.CreateLogger($"Test{i}").LogInformation("Test category colors...");
        }

        Console.ReadLine();
    }

} 