using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Console;
using Tentakel.Extensions.Logging.Providers;

namespace ConsoleApp;

public class ConsoleLoggerFactory : ILoggerFactory
{
    private readonly LoggerSinkProvider _loggerSinkProvider = new();

    public ConsoleLoggerFactory()
    {
        this._loggerSinkProvider.AddOrUpdateLoggerSink(new ColoredConsoleSink
        {
            Name = "C1",
            Categories = new[] { "Test" },
            LogLevel = LogLevel.Trace,
            ForegroundColor = ConsoleColor.White,
            TextFormatter = new TestTextFormatter()
        });

        this._loggerSinkProvider.AddOrUpdateLoggerSink(new ColoredConsoleSink
        {
            Name = "C2",
            Categories = new[] { "ConsoleApp.Program" },
            LogLevel = LogLevel.Trace,
            ForegroundColor = ConsoleColor.DarkCyan,
            TextFormatter = new TestTextFormatter2()
        });
    }


    public void Dispose()
    {
        this._loggerSinkProvider.Dispose();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return this._loggerSinkProvider.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }
}