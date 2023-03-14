using System;
using System.Collections.Generic;

namespace Oleander.Extensions.Configuration
{
    public interface IConfiguredTypesOptionsMonitor<out TOptions> : IDisposable
    {
        IReadOnlyCollection<string> GetKeys();
        IReadOnlyCollection<string> GetKeys(string name);

        TOptions? Get(string key);
        TOptions? Get(string name, string key);

        IDisposable OnChange(Action<string> listener);
        IDisposable OnChange(Action<TOptions?, string> listener);
        IDisposable OnChange(Action<TOptions?, string, string> listener);
    }
}