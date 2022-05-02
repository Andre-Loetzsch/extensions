using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptions<TOptions> : IConfiguredTypesOptions<TOptions> where TOptions : class
    {
        private readonly ConfiguredTypes _configuredTypes;

        public ConfiguredTypesOptions(IOptions<ConfiguredTypes> options, IConfigurationRoot configurationRoot)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));

            this._configuredTypes = options.Value ?? new ConfiguredTypes();
            this._configuredTypes.ConfigurationRoot = configurationRoot;
        }

        #region IConfiguredTypesOptionsSnapshot

        public IReadOnlyCollection<string> GetKeys()
        {
            return this._configuredTypes.GetKeys<TOptions>();
        }

        public TOptions Get(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return GetOrCreateInstance(this._configuredTypes.Get<TOptions>(key));
        }

        #endregion

        #region private members

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