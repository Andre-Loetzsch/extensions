using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Tentakel.Extensions.Configuration
{
    public class ConfiguredTypesOptionsMonitor<TOptions> : IDisposable, IConfiguredTypesOptionsMonitor<TOptions> where TOptions : class
    {
        private readonly IOptionsMonitor<ConfiguredTypes> _optionsMonitor;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TOptions>> _cache = new();

        private readonly List<IDisposable> _registrations = new();
        private event Action<TOptions, string, string> Changed;
        private event Action<string> ConfigurationChanged;

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
                    options = this.Get(name, key);
                    this.Changed?.Invoke(options, name, key);
                }
            });
        }

        #region IConfiguredTypesOptionsMonitor

        public IDisposable OnChange(Action<string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            this.ConfigurationChanged += disposable.ConfigurationChanged;
            this._registrations.Add(disposable);

            return disposable;
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
        }

        #endregion

        #region private members

        private sealed class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<string> _configurationChangedListener;
            private readonly Action<TOptions, string> _optionsKeyListener;
            private readonly Action<TOptions, string, string> _optionsNameKeyListener;

            private readonly ConfiguredTypesOptionsMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<string> listener)
            {
                this._configurationChangedListener = listener;
                this._monitor = monitor;
            }

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
            {
                this._optionsKeyListener = listener;
                this._monitor = monitor;
            }

            public ChangeTrackerDisposable(ConfiguredTypesOptionsMonitor<TOptions> monitor, Action<TOptions, string, string> listener)
            {
                this._optionsNameKeyListener = listener;
                this._monitor = monitor;
            }

            public void ConfigurationChanged(string name)
            {
                this._configurationChangedListener?.Invoke(name);
            }
            public void OnChange(TOptions options, string name, string key)
            {
                this._optionsKeyListener?.Invoke(options, key);
                this._optionsNameKeyListener?.Invoke(options, name, key);
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

        private ConcurrentDictionary<string, TOptions> GetInnerCache(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return this._cache.GetOrAdd(name, _ => new ConcurrentDictionary<string, TOptions>());
        }

        private static TOptions GetOrCreateInstance(TOptions instance)
        {
            if (instance != null) return instance;

            var constructorInfo = typeof(TOptions).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            return constructorInfo == null ? 
                null : Activator.CreateInstance<TOptions>();
        }

        #endregion
    }
}