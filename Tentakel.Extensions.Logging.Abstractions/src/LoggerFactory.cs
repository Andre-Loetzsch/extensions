using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tentakel.Extensions.Logging.Abstractions
{
    public static class LoggerFactory
    {
        private static ILoggerFactory instance = new NullLoggerFactory();

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
            LoggerFactory.instance = provider.GetService(typeof(ILoggerFactory)) as ILoggerFactory ?? new NullLoggerFactory();
        }
    }
}