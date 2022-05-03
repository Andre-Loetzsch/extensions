using System;
using System.Collections.Generic;

namespace Tentakel.Extensions.Configuration
{
    public interface IConfiguredTypesOptionsMonitor : IDisposable
    {
        IReadOnlyCollection<string> GetKeys<TOptions>();
        IReadOnlyCollection<string> GetKeys<TOptions>(string name);

        TOptions? Get<TOptions>(string key);
        TOptions? Get<TOptions>(string name, string key);

        IDisposable OnChange(Action<string> listener);
        IDisposable OnChange<TOptions>(Action<TOptions?, string> listener);
        IDisposable OnChange<TOptions>(Action<TOptions?, string, string> listener);
    }
}