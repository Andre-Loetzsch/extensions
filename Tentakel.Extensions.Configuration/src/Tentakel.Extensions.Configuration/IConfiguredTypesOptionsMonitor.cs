using System;
using System.Collections.Generic;

namespace Tentakel.Extensions.Configuration
{
    public interface IConfiguredTypesOptionsMonitor<TOptions>
    {
        IReadOnlyCollection<string> GetKeys();
        TOptions Get(string name);
        bool TryGet(string key, out TOptions value);
        IDisposable OnChange(Action<TOptions, string> listener);
    }
}