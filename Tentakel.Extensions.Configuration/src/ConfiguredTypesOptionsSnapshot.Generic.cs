using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsSnapshot<TOptions> : IConfiguredTypesOptionsSnapshot<TOptions> where TOptions : class
    {
        private readonly IOptionsSnapshot<ConfiguredTypes> _optionsSnapshot;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TOptions>> _cache = new();

        public ConfiguredTypesOptionsSnapshot(IOptionsSnapshot<ConfiguredTypes> optionsSnapshot, IConfigurationRoot configurationRoot)
        {
            this._optionsSnapshot = optionsSnapshot ?? throw new ArgumentNullException(nameof(optionsSnapshot));
            this._configurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));
        }

        #region IConfiguredTypesOptionsSnapshot

        public IReadOnlyCollection<string> GetKeys()
        {
            return this.GetKeys(Options.DefaultName);
        }

        public IReadOnlyCollection<string> GetKeys(string name)
        {
            return this.GetConfiguredTypes(name).GetKeys<TOptions>();
        }

        public TOptions Get(string key)
        {
            return this.Get(Options.DefaultName, key);
        }

        public TOptions Get(string name, string key)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (key == null) throw new ArgumentNullException(nameof(key));

            return this.GetInnerCache(name).GetOrAdd(key, k =>
                GetOrCreateInstance(this.GetConfiguredTypes(name).Get<TOptions>(k)));
        }

        #endregion

        #region private members

        private ConfiguredTypes GetConfiguredTypes(string name)
        {
            var configuredTypes = this._optionsSnapshot.Get(name) ?? new ConfiguredTypes();
            configuredTypes.ConfigurationRoot = this._configurationRoot;
            return configuredTypes;
        }

        private ConcurrentDictionary<string, TOptions> GetInnerCache(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return this._cache.GetOrAdd(name, _ => new ConcurrentDictionary<string, TOptions>());
        }

        private static TOptions GetOrCreateInstance(TOptions instance)
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