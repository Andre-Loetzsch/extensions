using System;
using System.Collections.Generic;

namespace Tentakel.Extensions.Configuration
{
    public interface IConfiguredTypes
    {
        event Action ConfigurationChanged;
        IEnumerable<T> GetAll<T>();
        bool TryGet<T>(string key, out T value);
        T Get<T>(string key);
    }
}