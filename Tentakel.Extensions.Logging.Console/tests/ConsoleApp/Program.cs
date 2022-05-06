using Microsoft.Extensions.Logging;

namespace ConsoleApp;

internal class Program
{
    private static void Main()
    {

        var logger = new ConsoleLoggerFactory().CreateLogger<Program>();

        logger.LogTrace("This is a Trace message!");
        logger.LogDebug("This is a Debug message!");
        logger.LogInformation("This is a Information message!");
        logger.LogWarning("This is a Warning message!");
        logger.LogError("This is a Error message!");
        logger.LogCritical("This is a Critical message!");

        logger.LogTrace($"This is a Trace message{Environment.NewLine}with multi{Environment.NewLine}lines.");

        Console.ReadLine();
    }
}