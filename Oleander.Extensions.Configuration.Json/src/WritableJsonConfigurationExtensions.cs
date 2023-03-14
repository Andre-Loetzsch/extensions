using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Oleander.Extensions.Configuration.Json
{
    public static class WritableJsonConfigurationExtensions
    {
        public static IConfigurationBuilder AddWritableJsonFile(this IConfigurationBuilder builder, string path)
        {
            return AddWritableJsonFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddWritableJsonFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddWritableJsonFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddWritableJsonFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddWritableJsonFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddWritableJsonFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, bool optional, bool reloadOnChange)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid file path!", nameof(path));
            }

            return builder
                .AddJsonFile(provider, path, optional, reloadOnChange)
                .AddWritableJsonFile(s =>
                {
                    s.FileProvider = provider;
                    s.Path = path;
                    s.Optional = optional;
                    s.ReloadOnChange = reloadOnChange;
                    s.ResolveFileProvider();
                });
        }

        public static IConfigurationBuilder AddWritableJsonFile(this IConfigurationBuilder builder, Action<WritableJsonConfigurationSource> configureSource)
            => builder.Add(configureSource);

        public static IConfigurationBuilder AddWritableJsonStream(this IConfigurationBuilder builder, Stream stream)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var buffer = new byte[stream.Length];
            _ = stream.Read(buffer, 0, buffer.Length);
            stream.Position = 0;

            var ms = new MemoryStream(buffer) { Position = 0 };
            return builder.AddJsonStream(ms).Add<WritableJsonStreamConfigurationSource>(s => s.Stream = stream);
        }

        public static IConfiguration Set<T>(this IConfiguration configuration, string key, T value)
        {
            configuration.GetSection(key).Value = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
            return configuration;
        }
    }
}