﻿using System.Collections.Generic;
using System.Linq;

namespace Oleander.Extensions.Configuration
{
    public static class ConfiguredTypesOptionsMonitorExtensions
    {
        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsMonitor configuredTypesOptionsMonitor, string key, out TOptions? options)
        {
            options = configuredTypesOptionsMonitor.Get<TOptions>(key);
            return options != null;
        }

        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsMonitor configuredTypesOptionsMonitor, string name, string key, out TOptions? options)
        {
            options = configuredTypesOptionsMonitor.Get<TOptions>(name, key);
            return options != null;
        }

        public static IEnumerable<TOptions> GetAll<TOptions>(this IConfiguredTypesOptionsMonitor configuredTypesOptionsMonitor)
        {
            return configuredTypesOptionsMonitor.GetKeys<TOptions>()
                .Select(configuredTypesOptionsMonitor.Get<TOptions>)
                .Where(x => x != null).ToList()!;
        }

        public static IEnumerable<TOptions> GetAll<TOptions>(this IConfiguredTypesOptionsMonitor configuredTypesOptionsMonitor, string name)
        {
            return configuredTypesOptionsMonitor.GetKeys<TOptions>(name)
                .Select(configuredTypesOptionsMonitor.Get<TOptions>).Where(x => x != null).ToList()!;
        }

        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsMonitor<TOptions> configuredTypesOptionsMonitor, string key, out TOptions? options)
        {
            options = configuredTypesOptionsMonitor.Get(key);
            return options != null;
        }

        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsMonitor<TOptions> configuredTypesOptionsMonitor, string name, string key, out TOptions? options)
        {
            options = configuredTypesOptionsMonitor.Get(name, key);
            return options != null;
        }

        public static IEnumerable<TOptions> GetAll<TOptions>(this IConfiguredTypesOptionsMonitor<TOptions> configuredTypesOptionsMonitor)
        {
            return configuredTypesOptionsMonitor.GetKeys()
                .Select(configuredTypesOptionsMonitor.Get)
                .Where(x => x != null).ToList()!;
        }

        public static IEnumerable<TOptions> GetAll<TOptions>(this IConfiguredTypesOptionsMonitor<TOptions> configuredTypesOptionsMonitor, string name)
        {
            return configuredTypesOptionsMonitor.GetKeys(name)
                .Select(key => configuredTypesOptionsMonitor.Get(name, key)).Where(x => x != null).ToList()!;
        }
    }
}