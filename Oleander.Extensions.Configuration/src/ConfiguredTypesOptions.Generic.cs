﻿using System;
using System.Collections.Generic;
using System.Reflection;
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
            return GetOrCreateInstance(this._configuredTypes.Get<TOptions>(key));
        }

        #endregion

        #region private members

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