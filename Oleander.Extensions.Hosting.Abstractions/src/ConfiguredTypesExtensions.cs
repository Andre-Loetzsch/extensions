using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.DependencyInjection;
using Oleander.Extensions.Logging.Abstractions;

namespace Oleander.Extensions.Configuration.Hosting.Abstractions
{
    public static class ConfiguredTypesExtensions
    {
        public static IHost LogConfiguredTypesExceptions(this IHost host, string loggerCategoryName, bool ignoreException = false)
        {
            var exceptions = host.Services.GetRequiredServices<Exception>().ToList();
            if (!exceptions.Any()) return host;

            var logger = host.Services.GetService<ILoggerFactory>()?.CreateLogger(loggerCategoryName);

            if (logger == null)
            {
                return ignoreException ? host : host.ThrowConfiguredTypesExceptions();
            }

            return host.LogConfiguredTypesExceptions(logger, ignoreException);
        }

        public static IHost LogConfiguredTypesExceptions<T>(this IHost host, bool ignoreException = false)
        {
            var exceptions = host.Services.GetRequiredServices<Exception>().ToList();
            if (!exceptions.Any()) return host;

            var logger = host.Services.GetService<ILoggerFactory>()?.CreateLogger<T>();

            if (logger == null)
            {
                return ignoreException ? host : host.ThrowConfiguredTypesExceptions();
            }

            return host.LogConfiguredTypesExceptions(logger, ignoreException);
        }

        public static IHost LogConfiguredTypesExceptions(this IHost host, ILogger logger, bool ignoreException = false)
        {
            var exceptions = host.Services.GetRequiredServices<Exception>().ToList();
            if (!exceptions.Any()) return host;

            try
            {
                return host.ThrowConfiguredTypesExceptions();
            }
            catch (AggregateException ex)
            {
                logger.LogError("{exception}", ex.GetAllMessages());
                if (!ignoreException) throw;
            }

            return host;
        }

        public static IHost ThrowConfiguredTypesExceptions(this IHost host)
        {
            var exceptions = host.Services.GetRequiredServices<Exception>().ToList();
            if (!exceptions.Any()) return host;

            throw new AggregateException(exceptions);
        }
    }
}