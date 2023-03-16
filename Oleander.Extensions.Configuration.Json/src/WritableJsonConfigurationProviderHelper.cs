using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Oleander.Extensions.Configuration.Json
{
    internal class WritableJsonConfigurationProviderHelper
    {
        internal static string Set(string json, string key, string value, Action<string, string> baseSetAction)
        {
            #region update current configuration

            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                // update json object
                var buffer = Encoding.UTF8.GetBytes(value);
                var stream = new MemoryStream(buffer) { Position = 0 };
                var configurationRoot = new ConfigurationBuilder().AddWritableJsonStream(stream).Build();

                foreach (var provider in configurationRoot.Providers.OfType<WritableJsonStreamConfigurationProvider>())
                {
                    foreach (var (k, v) in provider.InnerData)
                    {
                        baseSetAction($"{key}:{k}", v ?? string.Empty);
                    }
                }
            }
            else
            {
                // update property
                baseSetAction(key, value);
            }

            #endregion

            #region update json file/stream

            var placeholder = $"______{key}______";
            var path = key.Split(":");
            var sb = new StringBuilder();
            var valueLines = value.Split(Environment.NewLine);

            for (var i = 0; i < valueLines.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(valueLines[i]);
                    continue;
                }

                sb.AppendLine().Append("".PadRight(path.Length * 2)).Append(valueLines[i]);
            }

            value = sb.ToString();

            dynamic jsonObj = JsonConvert.DeserializeObject(json)!;
            var subJsonObj = jsonObj;

            for (var i = 0; i < path.Length; i++)
            {
                if (i < path.Length - 1)
                {
                    subJsonObj = subJsonObj[path[i]];

                    if (subJsonObj == null)
                    {
                        jsonObj[key] = placeholder;
                        break;
                    }

                    continue;
                }

                subJsonObj[path[i]] = placeholder;
            }

            var output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                output = output.Replace($"\"{placeholder}\"", value);
            }
            else
            {
                output = output.Replace(placeholder, value);
            }

            return output;

            #endregion
        }
    }
}