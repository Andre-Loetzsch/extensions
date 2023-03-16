using System.IO;
using Microsoft.Extensions.Configuration.Json;

namespace Oleander.Extensions.Configuration.Json
{
    public class WritableJsonConfigurationProvider : JsonConfigurationProvider
    {
        public WritableJsonConfigurationProvider(JsonConfigurationSource source) : base(source)
        {
        }

        public override void Set(string key, string? value)
        {
            if (this.Source.FileProvider == null) return;
            if (this.Source.Path == null) return;

            var fileFullPath = this.Source.FileProvider.GetFileInfo(this.Source.Path).PhysicalPath;
            if (fileFullPath == null) return;
            value ??= string.Empty;

            File.WriteAllText(fileFullPath, WritableJsonConfigurationProviderHelper.Set(
                File.ReadAllText(fileFullPath), key, value, (k, v) => { base.Set(k, v); }));
        }
    }
}
