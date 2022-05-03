using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsMonitor : IConfiguredTypesOptionsMonitor
    {
        private readonly IOptionsMonitor<ConfiguredTypes> _optionsMonitor;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _cache = new();

        private readonly List<IDisposable> _registrations = new();
        private event Action<object?, string, string>? Changed;
        private event Action<string>? ConfigurationChanged;
        
        public ConfiguredTypesOptionsMonitor(IOptionsMonitor<ConfiguredTypes> optionsMonitor, IConfigurationRoot configurationRoot)
        {
            this._optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            this._configurationRoot = configurationRoot ?? throw new ArgumentNullException(nameof(configurationRoot));

            this._optionsMonitor.OnChange((_, name) =>
            {
                this.ConfigurationChanged?.Invoke(name);
                var innerCache = this.GetInnerCache(name);

                foreach (var key in innerCache.Keys.ToList())
                {
                    innerCache.TryRemove(key, out var options);
                    options = this.Get<object>(name, key);
                    this.Changed?.Invoke(options, name, key);
                }
            });
        }

        #region IConfiguredTypesOptionsMonitor

        public IDisposable OnChange(Action<string> listener)
        {
            var disposable = new ChangeTrackerDisposable<object>(this, listener);
            this.ConfigurationChanged += disposable.ConfigurationChanged;
            this._registrations.Add(disposable);

            return disposable;
        }

        public IDisposable OnChange<TOptions>(Action<TOptions, string> listener)
        {
            var disposable = new ChangeTrackerDisposable<TOptions>(this, listener);

            this.Changed += disposable.OnChange;
            this._registrations.Add(disposable);

            return disposable;
        }

        public IDisposable OnChange<TOptions>(Action<TOptions, string, string> listener)
        {
            var disposable = new ChangeTrackerDisposable<TOptions>(this, listener);
            this.Changed += disposable.OnChange;

            this._registrations.Add(disposable);

            return disposable;
        }

        public IReadOnlyCollection<string> GetKeys<TOptions>()
        {
            return this.GetKeys<TOptions>(Options.DefaultName);
        }

        public IReadOnlyCollection<string> GetKeys<TOptions>(string name)
        {
            return this.GetConfiguredTypes(name).GetKeys<TOptions>();
        }

        public TOptions? Get<TOptions>(string key)
        {
            return this.Get<TOptions>(Options.DefaultName, key);
        }

        public TOptions? Get<TOptions>(string name, string key)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (!this.GetInnerCache(name).TryGetValue(key, out var obj))
            {
                obj = this.GetConfiguredTypes(name).Get<object>(key);
            }

            if (obj != null) this.GetInnerCache(name).TryAdd(key, obj);
            if (obj is TOptions options) return options;
            return default;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // Remove all subscriptions to the change tokens
            foreach (var registration in this._registrations)
            {
                registration.Dispose();
            }

            this._registrations.Clear();
            GC.SuppressFinalize(this);
        }

        #endregion

        #region private members

        private sealed class ChangeTrackerDisposable<TOptions> : IDisposable
        {
            private readonly Action<string>? _configurationChangedListener;
            private readonly Action<TOptions, string>? _optionsKeyListener;
            private readonly Action<TOptions, string, string>? _optionsNameKeyListener;

            private readonly ConfiguredTypesOptionsMonitor _monitor;

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor monitor, Action<string> listener)
            {
                this._configurationChangedListener = listener;
                this._monitor = monitor;
            }

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor monitor, Action<TOptions, string> listener)
            {
                this._optionsKeyListener = listener;
                this._monitor = monitor;
            }

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor monitor, Action<TOptions, string, string> listener)
            {
                this._optionsNameKeyListener = listener;
                this._monitor = monitor;
            }

            public void ConfigurationChanged(string name)
            {
                this._configurationChangedListener?.Invoke(name);
            }
            public void OnChange(object? options, string name, string key)
            {
                if (options is not TOptions op) return;
                this._optionsKeyListener?.Invoke(op, key);
                this._optionsNameKeyListener?.Invoke(op, name, key);
            }

            public void Dispose()
            {
                if (this._configurationChangedListener != null)
                {
                    this._monitor.ConfigurationChanged -= this.ConfigurationChanged;
                    return;
                }

                this._monitor.Changed -= this.OnChange;
            }
        }

        private ConfiguredTypes GetConfiguredTypes(string name)
        {
            var configuredTypes = this._optionsMonitor.Get(name) ?? new ConfiguredTypes();
            configuredTypes.ConfigurationRoot = this._configurationRoot;
            return configuredTypes;
        }

        private ConcurrentDictionary<string, object> GetInnerCache(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return this._cache.GetOrAdd(name, _ => new ConcurrentDictionary<string, object>());
        }

        #endregion
    }
}