using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Oleander.Extensions.Logging.Abstractions.Performance;
using System.Reflection;

namespace Oleander.Extensions.Logging.Abstractions
{
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
                var loggerMapper = new LoggerMapper(logger);
                loggerMapper.AdditionalData.Clear();
                loggerMapper.AdditionalData["{CallingAssembly}"] = Assembly.GetCallingAssembly().FullName;
                loggerMapper.AdditionalData["{CallerFilePath}"] = callerFilePath;
                loggerMapper.AdditionalData["{CallerMemberName}"] = callerMemberName;
                loggerMapper.AdditionalData["{CallerLineNumber}"] = callerLineNumber;
                return loggerMapper;
            }
        }

        public static PerformanceScope BeginPerformanceScope<TState>(this ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, TState state) where TState : notnull
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (policies == null) throw new ArgumentNullException(nameof(policies));

            return new(logger, policies, logger.BeginScope(state));
        }

        public static PerformanceScope BeginPerformanceScope(this ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, string messageFormat, params object?[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (policies == null) throw new ArgumentNullException(nameof(policies));

            return new(logger, policies, logger.BeginScope(messageFormat, args));
        }
    }
}