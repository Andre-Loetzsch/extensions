using System.Collections.Generic;

namespace Tentakel.Extensions.Configuration
{
    public interface IConfiguredTypesOptions
    {
        IReadOnlyCollection<string> GetKeys<TOptions>();

        TOptions? Get<TOptions>(string key);
    }
}