using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfigureOptions
    {
        private readonly ConfiguredTypes _configuredTypes;
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _serviceCollection;

        public ConfigureOptions(IOptionsMonitor<ConfiguredTypes> optionsMonitor, IConfiguration configuration, IServiceCollection collection)
        {
            this._configuredTypes = optionsMonitor.CurrentValue ?? new ConfiguredTypes();
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._serviceCollection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public void Configure()
        {
            const string methodName = nameof(InnerConfigure);
            var methodInfo = typeof(ConfigureOptions).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

            if (methodInfo == null) throw new InvalidOperationException($"Method {methodName} not found!");

            foreach (var (key, value) in this._configuredTypes)
            {
                methodInfo.MakeGenericMethod(Type.GetType(value.Type, true))
                    .Invoke(null, new object[] { this._serviceCollection, this._configuration, key, key });
            }
        }

        private static void InnerConfigure<T>(IServiceCollection collection, IConfiguration configuration, string key, string name) where T : class
        {
            collection.Configure<T>(name, configuration.GetSection(key));
        }
    }
}