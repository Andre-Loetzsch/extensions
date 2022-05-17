using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypes : Dictionary<string, ConfiguredType>
    {
        public IConfigurationRoot? ConfigurationRoot { get; set; }

        public IEnumerable<T> GetAll<T>()
        {
            foreach (var (key, value) in this)
            {
                if (value.Instance == null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(value.Type)) continue;
                        value.Instance = NotNullConfigurationRoot(this.ConfigurationRoot).GetSection(key.Replace("__", ":")).Get(Type.GetType(value.Type, true));
                    }
                    catch (Exception ex)
                    {
                        value.Instance = ex;
                    }
                }

                if (value.Instance is T instance)
                {
                    yield return instance;
                }
            }
        }

        public bool TryGet<T>(string key, [MaybeNullWhen(false)] out T instance)
        {
            instance = this.Get<T>(key);
            return instance != null;
        }

        public T? Get<T>(string key)
        {
            if (!this.TryGetValue(key, out var item)) return default;

            if (item.Instance == null)
            {
                try
                {
                    if (string.IsNullOrEmpty(item.Type)) return default;
                    item.Instance = NotNullConfigurationRoot(this.ConfigurationRoot).GetSection(key.Replace("__", ":")).Get(Type.GetType(item.Type, true));
                }
                catch (Exception ex)
                {
                    item.Instance = ex;
                }
            }

            if (item.Instance is T instance) return instance;
            return default;
        }

        public IReadOnlyCollection<string> GetKeys<T>()
        {
            return new ReadOnlyCollection<string>(
                this.Where(x => x.Value.Instance is T || typeof(T).IsAssignableFrom(GetType(x.Value)))
                    .Select(x => x.Key).ToList());
        }


        private static Type? GetType(ConfiguredType configuredType)
        {
            try
            {
                return string.IsNullOrEmpty(configuredType.Type) ? 
                    default : Type.GetType(configuredType.Type, true);
            }
            catch (Exception ex)
            {
                configuredType.Instance = ex;
                return ex.GetType();
            }
        }

        private static IConfigurationRoot NotNullConfigurationRoot(IConfigurationRoot? configurationRoot)
        {
            if (configurationRoot == null)
                throw new InvalidOperationException(
                    $"ConfigurationRoot is null! The property must be to a valid instance of the type {nameof(IConfigurationRoot)}.");

            return configurationRoot;
        }

    }
}