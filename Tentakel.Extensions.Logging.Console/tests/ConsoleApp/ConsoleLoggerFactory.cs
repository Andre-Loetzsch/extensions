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
            Categories = new []{"*"}, 
            LogLevel = LogLevel.Trace, 
            TextFormatter = new TestTextFormatter()
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