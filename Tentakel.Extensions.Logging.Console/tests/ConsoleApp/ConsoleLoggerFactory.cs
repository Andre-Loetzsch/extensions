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
            TextFormatter = new TestTextFormatter1()
        });

        this._loggerSinkProvider.AddOrUpdateLoggerSink(new ColoredConsoleSink
        {
            Name = "C2",
            Categories = new[] { "ConsoleApp.Program" },
            LogLevel = LogLevel.Trace,
            ForegroundColor = ConsoleColor.White,
            TextFormatter = new TestTextFormatter2()
        });

        this._loggerSinkProvider.AddOrUpdateLoggerSink(new ColoredConsoleSink
        {
            Name = "C3",
            Categories = new[] { "Test2*" },
            LogLevel = LogLevel.Trace,
            ForegroundColor = ConsoleColor.White,
            TextFormatter = new TestTextFormatter3()
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