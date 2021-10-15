using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsMonitor<TOptions> : IConfiguredTypesOptionsMonitor<TOptions> where TOptions : class, new()
    {

        private List<TOptions> _options;
        private readonly IConfiguredTypes _configuredTypes;
        private readonly List<IDisposable> _registrations = new();
        internal event Action<TOptions, string> _onChange;

        public ConfiguredTypesOptionsMonitor(IServiceProvider provider)
        {
            this._configuredTypes = provider.GetRequiredService<IConfiguredTypes>();
            this._options = new List<TOptions>(this._configuredTypes.GetAll<TOptions>());

            this._configuredTypes.ConfigurationChanged += () =>
            {
                this._options = new List<TOptions>(this._configuredTypes.GetAll<TOptions>());





            };
        }





        public TOptions Get(string name)
        {
            return this._configuredTypes.Get<TOptions>(name);
        }

        public IEnumerable<TOptions> GetAll()
        {
            return this._configuredTypes.GetAll<TOptions>();

        }

        public bool TryGet(string key, out TOptions value)
        {
            return this._configuredTypes.TryGet(key, out value);
        }


        internal sealed class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TOptions, string> _listener;
            private readonly ConfiguredTypesOptionsMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
            {
                this._listener = listener;
                this._monitor = monitor;
            }

            public void OnChange(TOptions options, string name) => this._listener.Invoke(options, name);

            public void Dispose() => this._monitor._onChange -= this.OnChange;
        }
    }
}