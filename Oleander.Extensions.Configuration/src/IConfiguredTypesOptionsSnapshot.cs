using System.Collections.Generic;

namespace Oleander.Extensions.Configuration
{
    public interface IConfiguredTypesOptionsSnapshot
    {
        IReadOnlyCollection<string> GetKeys<TOptions>();
        IReadOnlyCollection<string> GetKeys<TOptions>(string name);

        TOptions? Get<TOptions>(string key);
        TOptions? Get<TOptions>(string name, string key);
    }
}