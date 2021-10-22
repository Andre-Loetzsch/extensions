using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tentakel.Extensions.Configuration;

namespace Tentakel.Extensions.DependencyInjection
{
    public static class ConfiguredTypesProviderExtensions
    {
        #region IServiceCollection

        public static IServiceCollection AddConfiguredTypes(this IServiceCollection collection, string section = "types", string name = null)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var serviceProvider = collection.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var configurationRoot = (IConfigurationRoot)configuration;

            return collection.AddConfiguredTypes(configurationRoot, section, name);
        }

        public static IServiceCollection AddConfiguredTypes(this IServiceCollection collection, IConfigurationRoot configurationRoot, string section = "types", string name = null)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));

            if (string.IsNullOrEmpty(name))
            {
                collection.Configure<ConfiguredTypes>(configurationRoot.GetSection(section));
            }
            else
            {
                collection.Configure<ConfiguredTypes>(name, configurationRoot.GetSection(section));
            }

            collection.AddSingleton(configurationRoot);
            collection.TryAddSingleton<ConfiguredTypesProvider>();
            collection.TryAddSingleton<IConfiguredTypes>(provider => provider.GetRequiredService<ConfiguredTypesProvider>());
            collection.TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));

            return collection;
        }

        public static IServiceCollection AddConfigureOptions(this IServiceCollection collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            collection.TryAddSingleton<ConfigureOptions>();
            collection.TryAddSingleton(collection);

            var serviceProvider = collection.BuildServiceProvider();
            serviceProvider.GetRequiredService<ConfigureOptions>().Configure();

            return collection;
        }

        #endregion

        #region IServiceProvider

        public static T GetRequiredService<T>(this IServiceProvider provider, string key)
        {
            return provider.GetRequiredService<IConfiguredTypes>().Get<T>(key);
        }


        public static IEnumerable<T> GetRequiredServices<T>(this IServiceProvider provider)
        {
            return provider.GetRequiredService<IConfiguredTypes>().GetAll<T>();
        }

        #endregion
    }
}
