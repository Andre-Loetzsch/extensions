using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Oleander.Extensions.Configuration
{
    public class ConfiguredTypesOptions<TOptions> : IConfiguredTypesOptions<TOptions> where TOptions : class
    {
        private readonly ConfiguredTypes _configuredTypes;

        public ConfiguredTypesOptions(IOptions<ConfiguredTypes> options, IConfigurationRoot configurationRoot)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            this._configuredTypes = options.Value;
            this._configuredTypes.ConfigurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));
        }

        #region IConfiguredTypesOptionsSnapshot

        public IReadOnlyCollection<string> GetKeys()
        {
            return this._configuredTypes.GetKeys<TOptions>();
        }

        public TOptions? Get(string? key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return this._configuredTypes.Get<TOptions>(key);
        }

        #endregion
    }
}