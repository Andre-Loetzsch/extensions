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

        public static IServiceCollection AddConfiguredTypes(this IServiceCollection collection, string section = "types")
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            var serviceProvider = collection.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var configurationRoot = (IConfigurationRoot)configuration;

            return collection.AddConfiguredTypes(configurationRoot, section);
        }

        public static IServiceCollection AddConfiguredTypes(this IServiceCollection collection, IConfigurationRoot configurationRoot, string section = "types")
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (configurationRoot == null) throw new ArgumentNullException(nameof(configurationRoot));

            collection.Configure<ConfiguredTypes>(configurationRoot.GetSection(section));
            collection.AddSingleton(configurationRoot);
            collection.TryAddSingleton<ConfiguredTypesProvider>();
            collection.TryAddSingleton<IConfiguredTypes>(provider => provider.GetRequiredService<ConfiguredTypesProvider>());

            collection.TryAddSingleton(typeof(IConfiguredTypesOptionsMonitor<>), typeof(ConfiguredTypesOptionsMonitor<>));


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
