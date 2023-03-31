using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Oleander.Extensions.Configuration;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Extensions.Logging
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

            foreach (var (k, v) in attributes)
            {
                if (sb.Length > 0) sb.Append("; ");
                var key = k.Replace("; ", ";").Replace(", ", ",");
                var value = v.ToString()?.Replace("; ", ";").Replace(", ", ",");
                sb.Append(key).Append(", ").Append(value);
            }

            return sb.ToString();
        }

        #endregion

        #region ILoggerProvider.WaitOne()

        public static int WaitOne(this ILoggerProvider provider, int millisecondsTimeout)
        {
            return provider.WaitOne(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public static int WaitOne(this ILoggerProvider provider, TimeSpan timeout)
        {
            return provider is LoggerSinkProvider lsp ? lsp.WaitOne(timeout) : 0;
        }

        public static Task<int> WaitOneAsync(this ILoggerProvider provider, int millisecondsTimeout)
        {
            return provider.WaitOneAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public static async Task<int> WaitOneAsync(this ILoggerProvider provider, TimeSpan timeout)
        {
            return await Task.Run(() => provider is LoggerSinkProvider lsp ? lsp.WaitOne(timeout) : 0);
        }

        #endregion

        #region IHost.WaitForLogging()

        public static int WaitForLogging(this IHost host, int millisecondsTimeout)
        {
            return host.WaitForLogging(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public static int WaitForLogging(this IHost host, TimeSpan timeout)
        {
            return host.Services.GetRequiredService<ILoggerProvider>().WaitOne(timeout);
        }

        public static async Task<int> WaitForLoggingAsync(this IHost host, int millisecondsTimeout)
        {
            return await host.WaitForLoggingAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public static async Task<int> WaitForLoggingAsync(this IHost host, TimeSpan timeout)
        {
            return await Task.Run(() => host.Services.GetRequiredService<ILoggerProvider>().WaitOne(timeout));
        }

        #endregion
    }
}