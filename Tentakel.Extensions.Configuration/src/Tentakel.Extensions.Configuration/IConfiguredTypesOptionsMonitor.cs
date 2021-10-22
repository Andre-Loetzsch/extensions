using System;
using System.Collections.Generic;

namespace Tentakel.Extensions.Configuration
{
    public interface IConfiguredTypesOptionsMonitor<TOptions>
    {
        IReadOnlyCollection<string> GetKeys();
        IReadOnlyCollection<string> GetKeys(string name);

        TOptions Get(string key);
        TOptions Get(string name, string key);

        bool TryGet(string key, out TOptions value);
        bool TryGet(string name, string key, out TOptions value);

        IDisposable OnChange(Action<TOptions, string> listener);
        IDisposable OnChange(Action<TOptions, string, string> listener);

    }
}