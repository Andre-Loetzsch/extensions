using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypes : Dictionary<string, ConfiguredType>
    {
        public IConfigurationRoot ConfigurationRoot { get; set; }

        public IEnumerable<T> GetAll<T>()
        {
            this.TestConfigurationRootIsNotNull();

            foreach (var (key, value) in this)
            {
                if (value.Instance == null)
                {
                    try
                    {
                        value.Instance = this.ConfigurationRoot.GetSection(key.Replace("__", ":")).Get(Type.GetType(value.Type, true));
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

        public bool TryGet<T>(string key, out T instance)
        {
            instance = this.Get<T>(key);
            return instance != null;
        }

        public T Get<T>(string key)
        {
            this.TestConfigurationRootIsNotNull();

            if (!this.TryGetValue(key, out var item))
            {
                var splitKey = key.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (splitKey.Count < 2) return default;

                splitKey.RemoveAt(splitKey.Count -1);

                var section = string.Join(":", splitKey);
                key = key.Substring(section.Length +1);




                if (!this.ConfigurationRoot.GetSection(section).Exists()) return default;

                var configuredTypes = this.ConfigurationRoot.GetSection(section).Get<ConfiguredTypes>();

                if (!configuredTypes.TryGetValue(key, out item)) return default;
                if (item?.Type == null) return default;

                item.Instance = this.ConfigurationRoot.GetSection(key.Replace("__", ":")).Get(Type.GetType(item.Type, true));

                this.Add(key, item);
            }

            if (item.Instance == null)
            {
                try
                {
                    item.Instance = this.ConfigurationRoot.GetSection(key.Replace("__", ":")).Get(Type.GetType(item.Type, true));
                }
                catch (Exception ex)
                {
                    item.Instance = ex;
                }
            }

            if (item.Instance is T instance) return instance;
            return default;
        }

        private void TestConfigurationRootIsNotNull()
        {
            if (this.ConfigurationRoot == null)
                throw new InvalidOperationException(
                    $"{nameof(this.ConfigurationRoot)} is null! The property must be to a valid instance of the type {nameof(IConfigurationRoot)}.");
        }
    }
}