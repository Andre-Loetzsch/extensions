using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Console;
using Oleander.Extensions.Logging.Providers;

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
            TextFormatterType = "ConsoleApp.TestTextFormatter1, ConsoleApp"
        });

        this._loggerSinkProvider.AddOrUpdateLoggerSink(new ColoredConsoleSink
        {
            Name = "C2",
            Categories = new[] { "ConsoleApp.Program" },
            LogLevel = LogLevel.Trace,
            ForegroundColor = ConsoleColor.White,
            TextFormatterType = "ConsoleApp.TestTextFormatter2, ConsoleApp"
        });

        this._loggerSinkProvider.AddOrUpdateLoggerSink(new ColoredConsoleSink
        {
            Name = "C3",
            Categories = new[] { "Test1*", "Test2*" },
            LogLevel = LogLevel.Trace,
            ForegroundColor = ConsoleColor.White,
            TextFormatterType = "ConsoleApp.TestTextFormatter3, ConsoleApp"
        });
    }


    public void Dispose()
    {
        this._loggerSinkProvider.Dispose();
        GC.SuppressFinalize(this);
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