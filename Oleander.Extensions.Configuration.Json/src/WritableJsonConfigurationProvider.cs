using System.IO;
using Microsoft.Extensions.Configuration.Json;

namespace Oleander.Extensions.Configuration.Json
{
    public class WritableJsonConfigurationProvider : JsonConfigurationProvider
    {
        private readonly WritableJsonConfigurationProviderHelper _providerHelper = new();

        public WritableJsonConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
        }

        public override void Set(string key, string value)
        {
            var fileFullPath = base.Source.FileProvider.GetFileInfo(base.Source.Path).PhysicalPath;
                File.WriteAllText(fileFullPath, this._providerHelper.Set(
                File.ReadAllText(fileFullPath), key, value, (k, v) => { base.Set(k, v); }));
        }
    }
}
