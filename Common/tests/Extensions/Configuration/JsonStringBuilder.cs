using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Oleander.Extensions.Configuration.Tests;

public static class JsonStringBuilder
{
    public static string Build(IDictionary<string, object> configuration, string sectionName)
    {
        var sb = new StringBuilder();
        var typeDescriptions = new ConfiguredTypes();

        foreach (var item in configuration)
        {
            var assemblyQualifiedName = item.Value.GetType().AssemblyQualifiedName;
            if (assemblyQualifiedName == null) continue;
            var typeInfos = assemblyQualifiedName.Split([", "], StringSplitOptions.RemoveEmptyEntries);
            typeDescriptions.Add(item.Key, new ConfiguredType { Type = $"{typeInfos[0]}, {typeInfos[1]}" });
        }

        sb.AppendLine("{")
            .Append($"  \"{sectionName}\":");

        var jsonString = JsonSerializer.Serialize(typeDescriptions, new JsonSerializerOptions { WriteIndented = true });
        var lines = jsonString.Split([Environment.NewLine], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                sb.AppendLine().Append("    ");
            }

            sb.Append(lines[i]);
        }

        foreach (var item in configuration)
        {
            sb.AppendLine(",").Append($"  \"{item.Key}\":");

            jsonString = JsonSerializer.Serialize(item.Value, new JsonSerializerOptions { WriteIndented = true });
            lines = jsonString.Split([Environment.NewLine], StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    sb.AppendLine().Append("    ");
                }

                sb.Append(lines[i]);
            }
        }

        return sb.AppendLine().Append('}').ToString();
    }
}
