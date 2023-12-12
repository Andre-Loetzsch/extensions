using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Oleander.Extensions.Configuration
{
    public class ConfigureOptions
    {
        private readonly IOptionsMonitor<ConfiguredTypes> _optionsMonitor;
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _serviceCollection;

        public ConfigureOptions(IOptionsMonitor<ConfiguredTypes> optionsMonitor, IConfiguration configuration, IServiceCollection collection)
        {
            this._optionsMonitor = optionsMonitor;
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._serviceCollection = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        public bool Configure(string? sectionKey = null)
        {
            var configuredTypes = string.IsNullOrEmpty(sectionKey) ? 
                this._optionsMonitor.CurrentValue : 
                this._optionsMonitor.Get(sectionKey);
         
            if (configuredTypes.Count == 0) return true;

            const string methodName = nameof(InnerConfigure);
            var methodInfo = typeof(ConfigureOptions).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static) ?? 
                throw new InvalidOperationException($"Method {methodName} not found!");

            foreach (var item in configuredTypes)
            {
                if (string.IsNullOrEmpty(item.Value.Type)) continue;

                methodInfo.MakeGenericMethod(Type.GetType(item.Value.Type, true)!)
                    .Invoke(null, new object[] { this._serviceCollection, this._configuration, item.Key, item.Key });
            }

            return true;
        }

        private static void InnerConfigure<T>(IServiceCollection collection, IConfiguration configuration, string key, string name) where T : class
        {
            collection.Configure<T>(name, configuration.GetSection(key));
        }
    }
}