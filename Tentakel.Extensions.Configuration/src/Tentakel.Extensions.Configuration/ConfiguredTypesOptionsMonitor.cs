using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsMonitor<TOptions> : IDisposable, IConfiguredTypesOptionsMonitor<TOptions> where TOptions : class, new()
    {
        private readonly IOptionsMonitor<ConfiguredTypes> _optionsMonitor;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TOptions>> _cache = new();

        private readonly List<IDisposable> _registrations = new();
        private event Action<TOptions, string, string> Changed;

        public ConfiguredTypesOptionsMonitor(IOptionsMonitor<ConfiguredTypes> optionsMonitor, IConfigurationRoot configurationRoot)
        {
            this._optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            this._configurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot)); 

            this._optionsMonitor.OnChange((_, name) =>
            {
                var innerCache = this.GetInnerCache(name);

                foreach (var key in innerCache.Keys.ToList())
                {
                    innerCache.TryRemove(key, out var options);
                    options = this.Get(name, key);
                    this.Changed?.Invoke(options, name, key);
                }
            });
        }

        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            this.Changed += disposable.OnChange;
            this._registrations.Add(disposable);

            return disposable;
        }

        public IDisposable OnChange(Action<TOptions, string, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            this.Changed += disposable.OnChange;
            this._registrations.Add(disposable);

            return disposable;
        }




        public IReadOnlyCollection<string> GetKeys()
        {
            return this.GetKeys(Options.DefaultName);
        }

        public IReadOnlyCollection<string> GetKeys(string name)
        {
            return this.GetConfiguredTypes(name).GetKeys<TOptions>();
        }

        public TOptions Get(string key)
        {
            return this.Get(Options.DefaultName, key);
        }

        public TOptions Get(string name, string key)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (key == null) throw new ArgumentNullException(nameof(key));
           
            return this.GetInnerCache(name).GetOrAdd(key, k => 
                GetOrCreateInstance(this.GetConfiguredTypes(name).Get<TOptions>(k)));
        }

        public bool TryGet(string key, out TOptions value)
        {
            return this.TryGet(Options.DefaultName, key, out value);
        }

        public bool TryGet(string name, string key, out TOptions value)
        {
            value = this.Get(name, key);
            return value != null;
        }

        #region private members

        private ConfiguredTypes GetConfiguredTypes(string name)
        {
            var configuredTypes = this._optionsMonitor.Get(name) ?? new ConfiguredTypes();
            configuredTypes.ConfigurationRoot = this._configurationRoot;
            return configuredTypes;
        }

        private ConcurrentDictionary<string, TOptions> GetInnerCache(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return this._cache.GetOrAdd(name, _ => new ConcurrentDictionary<string, TOptions>());
        }

        private static TOptions GetOrCreateInstance(TOptions instance)
        {
            return instance ?? Activator.CreateInstance<TOptions>();
        }

        private sealed class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TOptions, string> _listener;
            private readonly Action<TOptions, string, string> _namedListener;

            private readonly ConfiguredTypesOptionsMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
            {
                this._listener = listener;
                this._monitor = monitor;
            }

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<TOptions, string, string> listener)
            {
                this._namedListener = listener;
                this._monitor = monitor;
            }

            public void OnChange(TOptions options, string name, string key)
            {
                this._listener?.Invoke(options, key);
                this._namedListener?.Invoke(options, name, key);
            }

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

        #endregion
    }
}