using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Oleander.Extensions.Logging.Abstractions
{
    internal class LoggerMapper(ILogger logger) : ILogger
    {
        private readonly ILogger _logger = logger;
        private static readonly Func<FormattedLogValues, Exception?, string> messageFormatter = MessageFormatter;


        private static string MessageFormatter(FormattedLogValues state, Exception? error)
        {
            return state.ToString();
        }

        public Dictionary<string, object?> AdditionalData { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
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
            var logger = this._logger;

            while (logger is LoggerMapper loggerMapper)
            {
                logger = loggerMapper._logger;
            }

            logger.Log(logLevel, eventId, formattedLogValues, exception, messageFormatter);

            // may only be used once, otherwise the information may be incorrect
            this.AdditionalData.Remove("{CallingAssembly}");
            this.AdditionalData.Remove("{CallerFilePath}");
            this.AdditionalData.Remove("{CallerMemberName}");
            this.AdditionalData.Remove("{CallerLineNumber}");
        }
    }
}