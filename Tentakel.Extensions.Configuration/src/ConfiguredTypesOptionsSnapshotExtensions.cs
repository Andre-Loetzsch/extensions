using System.Collections.Generic;
using System.Linq;

namespace Tentakel.Extensions.Configuration
{
    public static class ConfiguredTypesOptionsSnapshotExtensions
    {
        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsSnapshot configuredTypesOptionsSnapshot, string key, out TOptions? options)
        {
            options = configuredTypesOptionsSnapshot.Get<TOptions>(key);
            return options != null;
        }

        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsSnapshot configuredTypesOptionsSnapshot, string name, string key, out TOptions? options)
        {
            options = configuredTypesOptionsSnapshot.Get<TOptions>(name, key);
            return options != null;
        }

        public static IEnumerable<TOptions> GetAll<TOptions>(this IConfiguredTypesOptionsSnapshot configuredTypesOptionsSnapshot)
        {
            return configuredTypesOptionsSnapshot.GetKeys<TOptions>()
                .Select(configuredTypesOptionsSnapshot.Get<TOptions>)
                .Where(x => x != null) .ToList()!;
        }



        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsSnapshot<TOptions> configuredTypesOptionsSnapshot, string key, out TOptions? options)
        {
            options = configuredTypesOptionsSnapshot.Get(key);
            return options != null;
        }

        public static bool TryGet<TOptions>(this IConfiguredTypesOptionsSnapshot<TOptions> configuredTypesOptionsSnapshot, string name, string key, out TOptions? options)
        {
            options = configuredTypesOptionsSnapshot.Get(name, key);
            return options != null;
        }

        public static IEnumerable<TOptions> GetAll<TOptions>(this IConfiguredTypesOptionsSnapshot<TOptions> configuredTypesOptionsSnapshot)
        {
            return configuredTypesOptionsSnapshot.GetKeys().Select(configuredTypesOptionsSnapshot.Get)
                .Where(x => x != null).ToList()!;
        }
    }
}