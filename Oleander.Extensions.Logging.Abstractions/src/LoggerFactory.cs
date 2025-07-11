using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Oleander.Extensions.Logging.Abstractions
{
    public static class LoggerFactory
    {
        private static ILoggerFactory instance = new NullLoggerFactory();

        public static bool IsInitialized { get; private set; }

        public static ILogger CreateLogger(string categoryName)
        {
            return instance.CreateLogger(categoryName);
        }

        public static ILogger CreateLogger<T>()
        {
            return instance.CreateLogger<T>();
        }

        public static ILogger CreateLogger(Type type)
        {
            return instance.CreateLogger(type);
        }

        public static void InitLoggerFactory(this IServiceProvider provider)
        {
            var factory = provider.GetService(typeof(ILoggerFactory)) as ILoggerFactory;

            IsInitialized = factory != null;
            LoggerFactory.instance = factory ?? new NullLoggerFactory();
        }
    }
}