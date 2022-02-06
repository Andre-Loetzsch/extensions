using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Abstractions.Performance;

namespace Tentakel.Extensions.Logging.Abstractions
{
    public static class LoggerExtensions
    {
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