using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using Tentakel.Extensions.Logging.Abstractions.Performance;

namespace Tentakel.Extensions.Logging.Abstractions
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
                var loggerMapper = new LoggerMapper { Logger = logger };
                loggerMapper.AdditionalData.Clear();
                loggerMapper.AdditionalData["{CallerFilePath}"] = callerFilePath;
                loggerMapper.AdditionalData["{CallerMemberName}"] = callerMemberName;
                loggerMapper.AdditionalData["{CallerLineNumber}"] = callerLineNumber;
                return loggerMapper;
            }
        }


        public static PerformanceScope BeginPerformanceScope<TState>(this ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, TState state)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (policies == null) throw new ArgumentNullException(nameof(policies));

            return new PerformanceScope(logger, policies, logger.BeginScope(state));
        }

        public static PerformanceScope BeginPerformanceScope(this ILogger logger, IEnumerable<PerformanceControlPointPolicy> policies, string messageFormat, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (policies == null) throw new ArgumentNullException(nameof(policies));

            return new PerformanceScope(logger, policies, logger.BeginScope(messageFormat, args));
        }
    }
}