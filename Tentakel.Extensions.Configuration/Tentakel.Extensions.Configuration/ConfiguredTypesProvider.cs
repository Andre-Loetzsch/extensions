using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesProvider : IConfiguredTypes
    {
        public event Action ConfigurationChanged;

        private ConfiguredTypes _configuredTypes;
        public ConfiguredTypesProvider(IOptionsMonitor<ConfiguredTypes> optionsMonitor, IConfigurationRoot configurationRoot)
        {
            if (optionsMonitor == null) throw new ArgumentNullException(nameof(optionsMonitor));
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));

            this._configuredTypes = optionsMonitor.CurrentValue ?? new ConfiguredTypes();
            this._configuredTypes.ConfigurationRoot = configurationRoot;

            optionsMonitor.OnChange((serviceTypeConfig, _) =>
            {
                serviceTypeConfig ??= new ConfiguredTypes();
                serviceTypeConfig.ConfigurationRoot = configurationRoot;
                this._configuredTypes = serviceTypeConfig;

                this.ConfigurationChanged?.Invoke();
            });
        }

        public IEnumerable<T> GetAll<T>()
        {
            return this._configuredTypes.GetAll<T>();
        }

        public bool TryGet<T>(string key, out T value)
        {
            return this._configuredTypes.TryGet(key, out value);
        }

        public T Get<T>(string key)
        {
            return this._configuredTypes.Get<T>(key);
        }
    }
}