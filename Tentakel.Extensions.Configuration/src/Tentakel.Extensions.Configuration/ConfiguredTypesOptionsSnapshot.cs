using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsSnapshot : IConfiguredTypesOptionsSnapshot
    {
        private readonly IOptionsSnapshot<ConfiguredTypes> _optionsSnapshot;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _cache = new();

        public ConfiguredTypesOptionsSnapshot(IOptionsSnapshot<ConfiguredTypes> optionsSnapshot, IConfigurationRoot configurationRoot)
        {
            this._optionsSnapshot = optionsSnapshot ?? throw new ArgumentNullException(nameof(optionsSnapshot));
            this._configurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));
        }

        #region IConfiguredTypesOptionsSnapshot

        public IReadOnlyCollection<string> GetKeys<TOptions>()
        {
            return this.GetKeys<TOptions>(Options.DefaultName);
        }

        public IReadOnlyCollection<string> GetKeys<TOptions>(string name)
        {
            return this.GetConfiguredTypes(name).GetKeys<TOptions>();
        }

        public TOptions Get<TOptions>(string key)
        {
            return this.Get<TOptions>(Options.DefaultName, key);
        }

        public TOptions Get<TOptions>(string name, string key)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (key == null) throw new ArgumentNullException(nameof(key));

            var obj = this.GetInnerCache(name).GetOrAdd(key, k =>
                this.GetConfiguredTypes(name).Get<object>(k));

            if (obj is TOptions options) return options;
            return default;
        }

        #endregion

        #region private members

        private ConfiguredTypes GetConfiguredTypes(string name)
        {
            var configuredTypes = this._optionsSnapshot.Get(name) ?? new ConfiguredTypes();
            configuredTypes.ConfigurationRoot = this._configurationRoot;
            return configuredTypes;
        }

        private ConcurrentDictionary<string, object> GetInnerCache(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return this._cache.GetOrAdd(name, _ => new ConcurrentDictionary<string, object>());
        }

        #endregion
    }
}