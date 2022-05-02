using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Tentakel.Extensions.Configuration;
using Tentakel.Extensions.Logging.Providers;

namespace Tentakel.Extensions.Logging
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder AddLoggerSinkProvider(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());
            return builder;
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