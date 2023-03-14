using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Oleander.Extensions.Configuration
{
    public class ConfiguredTypesOptions : IConfiguredTypesOptions
    {
        private readonly ConfiguredTypes _configuredTypes;

        public ConfiguredTypesOptions(IOptions<ConfiguredTypes> options, IConfigurationRoot configurationRoot)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            this._configuredTypes = options.Value ?? new ConfiguredTypes();
            this._configuredTypes.ConfigurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));
        }

        #region IConfiguredTypesOptions

        public IReadOnlyCollection<string> GetKeys<TOptions>()
        {
            return this._configuredTypes.GetKeys<TOptions>();
        }

        public TOptions? Get<TOptions>(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var obj = this._configuredTypes.Get<object>(key);

            if (obj is TOptions options) return options;
            return default;
        }

        #endregion
    }
}