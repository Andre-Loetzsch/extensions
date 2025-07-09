using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Oleander.Extensions.Logging.Abstractions.Performance;
using System.Reflection;

namespace Oleander.Extensions.Logging.Abstractions;

public static class LoggerExtensions
{
    private static readonly object loggerMapperSync = new();

    public static ILogger AddCallerInfos(this ILogger logger,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        lock (loggerMapperSync)
        {
            return logger.AddAttributes([
                new("{CallingAssembly}", Assembly.GetCallingAssembly().FullName),
                new("{CallerFilePath}", callerFilePath),
                new("{CallerMemberName}", callerMemberName),
                new("{CallerLineNumber}", callerLineNumber)
            ]);
        }
    }

    public static ILogger AddCorrelationId(this ILogger logger, object correlationId)
    {
        lock (loggerMapperSync)
        {
            return logger.AddAttributes([
                new("{CorrelationId}", correlationId)
            ]);
        }
    }

    public static ILogger AddAttribute(this ILogger logger, string key, object value)
    {
        return logger.AddAttributes([new(key, value)]);
    }



    public static ILogger AddAttributes(this ILogger logger, IEnumerable<KeyValuePair<string, object>> attributes)
    {
        lock (loggerMapperSync)
        {
            var keyValueList = attributes.ToList();
            if (!keyValueList.Any()) return logger;

            //if (logger is not LoggerMapper loggerMapper) loggerMapper = new(logger);
            //// TODO new LoggerMapper(logger);
            ////var loggerMapper = new LoggerMapper(logger);

            var loggerMapper = new LoggerMapper(logger);
            if (logger is LoggerMapper mapper)
            {
                foreach (var attribute in mapper.AdditionalData)
                {
                    loggerMapper.AdditionalData[attribute.Key] = attribute.Value;
                }
            }

            foreach (var attribute in keyValueList)
            {
                loggerMapper.AdditionalData[attribute.Key] = attribute.Value;
            }

            return loggerMapper;
        }
    }

    public static PerformanceScope BeginPerformanceScope<TState>(this ILogger logger,
        IEnumerable<PerformanceControlPointPolicy> policies, TState state) where TState : notnull
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (policies == null) throw new ArgumentNullException(nameof(policies));

        return new(logger, policies, logger.BeginScope(state));
    }

    public static PerformanceScope BeginPerformanceScope(this ILogger logger,
        IEnumerable<PerformanceControlPointPolicy> policies, string messageFormat, params object?[] args)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (policies == null) throw new ArgumentNullException(nameof(policies));

        return new(logger, policies, logger.BeginScope(messageFormat, args));
    }

    #region LogCorrelation

    public static void LogCorrelationDebug(this ILogger logger, object correlationId, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Debug, eventId, exception, message, args);
    }

    public static void LogCorrelationDebug(this ILogger logger, object correlationId, EventId eventId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Debug, eventId, message, args);
    }

    public static void LogCorrelationDebug(this ILogger logger, object correlationId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Debug, exception, message, args);
    }

    public static void LogCorrelationDebug(this ILogger logger, object correlationId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Debug, message, args);
    }

    public static void LogCorrelationTrace(this ILogger logger, object correlationId, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Trace, eventId, exception, message, args);
    }

    public static void LogCorrelationTrace(this ILogger logger, object correlationId, EventId eventId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Trace, eventId, message, args);
    }

    public static void LogCorrelationTrace(this ILogger logger, object correlationId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Trace, exception, message, args);
    }

    public static void LogCorrelationTrace(this ILogger logger, object correlationId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Trace, message, args);
    }

    public static void LogCorrelationInformation(this ILogger logger, object correlationId, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Information, eventId, exception, message, args);
    }

    public static void LogCorrelationInformation(this ILogger logger, object correlationId, EventId eventId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Information, eventId, message, args);
    }

    
    public static void LogCorrelationInformation(this ILogger logger, object correlationId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Information, exception, message, args);
    }

    public static void LogCorrelationInformation(this ILogger logger, object correlationId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Information, message, args);
    }

    public static void LogCorrelationWarning(this ILogger logger, object correlationId, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Warning, eventId, exception, message, args);
    }

    public static void LogCorrelationWarning(this ILogger logger, object correlationId, EventId eventId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Warning, eventId, message, args);
    }

    public static void LogCorrelationWarning(this ILogger logger, object correlationId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Warning, exception, message, args);
    }

    public static void LogCorrelationWarning(this ILogger logger, object correlationId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Warning, message, args);
    }

    public static void LogCorrelationError(this ILogger logger, object correlationId, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Error, eventId, exception, message, args);
    }

    public static void LogCorrelationError(this ILogger logger, object correlationId, EventId eventId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Error, eventId, message, args);
    }

    public static void LogCorrelationError(this ILogger logger, object correlationId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Error, exception, message, args);
    }

    public static void LogCorrelationError(this ILogger logger, object correlationId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Error, message, args);
    }

    public static void LogCorrelationCritical(this ILogger logger, object correlationId, EventId eventId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Critical, eventId, exception, message, args);
    }

    public static void LogCorrelationCritical(this ILogger logger, object correlationId, EventId eventId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Critical, eventId, message, args);
    }

    public static void LogCorrelationCritical(this ILogger logger, object correlationId, Exception? exception, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Critical, exception, message, args);
    }

    public static void LogCorrelationCritical(this ILogger logger, object correlationId, string? message, params object?[] args)
    {
        logger.AddCorrelationId(correlationId).Log(LogLevel.Critical, message, args);
    }

    #endregion
}
