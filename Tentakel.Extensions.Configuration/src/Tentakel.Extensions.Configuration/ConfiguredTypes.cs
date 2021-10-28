using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            if (!this.TryGetValue(key, out var item)) return default;

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

        public IReadOnlyCollection<string> GetKeys<T>()
        {
            //return new ReadOnlyCollection<string>(
            //    this.Where(x => Type.GetType(x.Value.Type) is T).Select(x => x.Key).ToList());
            return new ReadOnlyCollection<string>(
                this.Where(x => x.Value.Instance is T ||this.GetType(x.Value).IsAssignableFrom(typeof(T))).Select(x => x.Key).ToList());
        }


        private Type GetType(ConfiguredType configuredType)
        {
            try
            {
                return Type.GetType(configuredType.Type, true);
            }
            catch (Exception ex)
            {
                configuredType.Instance = ex;
                return ex.GetType();
            }
        }


        private void TestConfigurationRootIsNotNull()
        {
            if (this.ConfigurationRoot == null)
                throw new InvalidOperationException(
                    $"{nameof(this.ConfigurationRoot)} is null! The property must be to a valid instance of the type {nameof(IConfigurationRoot)}.");
        }
    }
}