using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Tentakel.Extensions.Logging.Performance;

namespace Tentakel.Extensions.Logging
{
    public static class LoggerSinkExtensions
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

        #region IDictionary<string, object>.ToLogString()

        public static string ToLogString(this IDictionary<string, object> attributes)
        {
            return attributes.ToArray().ToLogString();
        }

        public static string ToLogString(this IEnumerable<KeyValuePair<string, object>> attributes)
        {
            var sb = new StringBuilder();

            if (attributes == null) return sb.ToString();

            foreach (var (k, v) in attributes)
            {
                if (sb.Length > 0) sb.Append("; ");
                var key = k.Replace("; ", ";").Replace(", ", ",");
                var value = v == null ? string.Empty : v?.ToString()?.Replace("; ", ";").Replace(", ", ","); 
                sb.Append(key).Append(", ").Append(value);
            }

            return sb.ToString();
        }

        #endregion
    }
}