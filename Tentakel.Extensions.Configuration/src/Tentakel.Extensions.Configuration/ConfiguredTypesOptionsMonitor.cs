using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsMonitor<TOptions> : IDisposable, IConfiguredTypesOptionsMonitor<TOptions> where TOptions : class, new()
    {
        private readonly ConcurrentDictionary<string, TOptions> _cache = new(); 
        private readonly IConfiguredTypes _configuredTypes;
        private readonly List<IDisposable> _registrations = new();
        private event Action<TOptions, string> Changed;

        public ConfiguredTypesOptionsMonitor(IConfiguredTypes configuredTypes)
        {
            this._configuredTypes = configuredTypes;

            this._configuredTypes.ConfigurationChanged += () =>
            {
                foreach (var key in this._cache.Keys.ToList())
                {
                    this._cache.TryRemove(key, out _);
                    var options = this.Get(key);
                    this.Changed?.Invoke(options, key);
                }
            };
        }

        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            this.Changed += disposable.OnChange;
            this._registrations.Add(disposable);

            return disposable;
        }

        public IReadOnlyCollection<string> GetKeys()
        {
            return this._configuredTypes.GetKeys<TOptions>();
        }


        public TOptions Get(string name)
        {
            name ??= Options.DefaultName;
            return this._cache.GetOrAdd(name, key => this._configuredTypes.Get<TOptions>(key));
        }
       

        public bool TryGet(string key, out TOptions value)
        {
            if (!this._configuredTypes.TryGet(key, out value)) return false;
            var value1 = value;
            this._cache.AddOrUpdate(key, value, (_, _) => value1);
            return true;
        }


        private sealed class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TOptions, string> _listener;
            private readonly ConfiguredTypesOptionsMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
            {
                this._listener = listener;
                this._monitor = monitor;
            }

            public void OnChange(TOptions options, string name) => this._listener.Invoke(options, name);

            public void Dispose() => this._monitor.Changed -= this.OnChange;
        }


        public void Dispose()
        {
            // Remove all subscriptions to the change tokens
            foreach (var registration in this._registrations)
            {
                registration.Dispose();
            }

            this._registrations.Clear();
        }

    }
}