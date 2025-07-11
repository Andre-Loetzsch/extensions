using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Oleander.Extensions.Configuration
{
    public class ConfiguredTypesOptionsSnapshot<TOptions>(IOptionsSnapshot<ConfiguredTypes> optionsSnapshot, IConfigurationRoot configurationRoot)
        : IConfiguredTypesOptionsSnapshot<TOptions>
        where TOptions : class
    {
        private readonly IOptionsSnapshot<ConfiguredTypes> _optionsSnapshot = optionsSnapshot ?? throw new ArgumentNullException(nameof(optionsSnapshot));
        private readonly IConfigurationRoot _configurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TOptions>> _cache = new();

        #region IConfiguredTypesOptionsSnapshot

        public IReadOnlyCollection<string> GetKeys()
        {
            return this.GetKeys(Options.DefaultName);
        }

        public IReadOnlyCollection<string> GetKeys(string? name)
        {
            return this.GetConfiguredTypes(name).GetKeys<TOptions>();
        }

        public TOptions? Get(string? key)
        {
            return this.Get(Options.DefaultName, key);
        }

        public TOptions? Get(string? name, string? key)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (this.GetInnerCache(name).TryGetValue(key, out var options))
            {
                return options;
            }

            options = GetOrCreateInstance(this.GetConfiguredTypes(name).Get<TOptions>(key));
            if (options != null) this.GetInnerCache(name).TryAdd(key, options);

            return options;
        }

        #endregion

        #region private members

        private ConfiguredTypes GetConfiguredTypes(string? name)
        {
            var configuredTypes = this._optionsSnapshot.Get(name);
            configuredTypes.ConfigurationRoot = this._configurationRoot;
            return configuredTypes;
        }

        private ConcurrentDictionary<string, TOptions> GetInnerCache(string name)
        {
            return this._cache.GetOrAdd(name, _ => new());
        }

        private static TOptions? GetOrCreateInstance(TOptions? instance)
        {
            if (instance != null) return instance;

            var constructorInfo = typeof(TOptions).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            return constructorInfo == null ?
                null : Activator.CreateInstance<TOptions>();
        }

        #endregion
    }
}