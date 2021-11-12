using System.Collections.Generic;

namespace Tentakel.Extensions.Configuration
{
    public interface IConfiguredTypesOptions<out TOptions>
    {
        IReadOnlyCollection<string> GetKeys();

        TOptions Get(string key);
    }
}