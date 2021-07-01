using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tentakel.Extensions.Configuration;

namespace Tentakel.Extensions.DependencyInjection
{
    public static class ConfiguredTypesProviderExtensions
    {
        public static IServiceCollection AddConfiguredServices<TService>(this IServiceCollection collection)
        {
            return collection.AddTransient(provider => provider.GetRequiredService<IConfiguredTypes>().GetAll<TService>());
        }

        public static IServiceCollection AddConfiguredService<TService>(this IServiceCollection collection, string key) where TService : class
        {
            return collection.AddTransient(provider => provider.GetRequiredService<IConfiguredTypes>().Get<TService>(key));
        }

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

            return collection;
        }
    }
}
