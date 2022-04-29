using Microsoft.Extensions.Logging;

namespace Tentakel.Extensions.Logging.Abstractions
{
    internal class LoggerMapper : ILogger
    {
        private readonly ILogger _logger;

        private static readonly Func<FormattedLogValues, Exception?, string> messageFormatter = MessageFormatter;

        public LoggerMapper(ILogger logger)
        {
            this._logger = logger;
        }

        private static string MessageFormatter(FormattedLogValues state, Exception? error)
        {
            return state.ToString();
        }

        public Dictionary<string, object?> AdditionalData { get; set; } = new();

        public IDisposable BeginScope<TState>(TState state)
        {
            return this._logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this._logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var values = new List<KeyValuePair<string, object?>>();

            if (state is IReadOnlyList<KeyValuePair<string, object?>> readOnlyList)
            {
                values.AddRange(readOnlyList);
            }

            values.AddRange(this.AdditionalData);
            var formattedLogValues = new FormattedLogValues(state?.ToString() ?? string.Empty, values);
            this._logger.Log(logLevel, eventId, formattedLogValues, exception, messageFormatter);
        }
    }
}