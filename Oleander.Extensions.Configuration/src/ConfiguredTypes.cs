using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Oleander.Extensions.Configuration
{
    public class ConfiguredTypes : Dictionary<string, ConfiguredType>
    {
        public IConfigurationRoot? ConfigurationRoot { get; set; }

        public IEnumerable<T> GetAll<T>()
        {
            foreach (var item in this)
            {
                if (item.Value.Instance == null)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(item.Value.Type)) continue;

                        var type = Type.GetType(item.Value.Type!, true);
                        if (type == null) continue;

                        item.Value.Instance = NotNullConfigurationRoot(this.ConfigurationRoot).GetSection(item.Key.Replace("__", ":")).Get(type);
                    }
                    catch (Exception ex)
                    {
                        item.Value.Instance = ex;
                    }
                }

                if (item.Value.Instance is T instance)
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

                    var type = Type.GetType(item.Type, true);
                    if (type == null) return default;

                    item.Instance = NotNullConfigurationRoot(this.ConfigurationRoot).GetSection(key.Replace("__", ":")).Get(type);
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
                    $"ConfigurationRoot is null! The property must be to a valid instance of the type '{nameof(IConfigurationRoot)}'.");

            return configurationRoot;
        }

    }
}