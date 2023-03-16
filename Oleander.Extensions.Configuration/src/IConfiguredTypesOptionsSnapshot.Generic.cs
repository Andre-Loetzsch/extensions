using System.Collections.Generic;

namespace Oleander.Extensions.Configuration
{
    public interface IConfiguredTypesOptionsSnapshot<out TOptions>
    {
        IReadOnlyCollection<string> GetKeys();
        IReadOnlyCollection<string> GetKeys(string? name);

        TOptions? Get(string? key);
        TOptions? Get(string? name, string? key);
    }
}